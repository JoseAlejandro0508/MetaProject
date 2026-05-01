using Microsoft.EntityFrameworkCore;

namespace Meta_xi.Application;

public class UpdatePlans : IUpdatePlansPerHour
{
    public readonly DBContext context;

    public UpdatePlans(DBContext dbcontext)
    {
        context = dbcontext;
    }

    public async Task UpdatePlansPerHour()
    {
        var userplans = await context.UserPlans.ToListAsync();
        var userBenefitDailies = new List<UserBenefitDaily>();

        foreach (var userPlan in userplans)
        {
            var plan = await context.Plans.FirstOrDefaultAsync(option => option.Name == userPlan.NamePlan);

            if (plan == null) continue;

            var userBenefitDaily = userBenefitDailies.FirstOrDefault(option => option.Username == userPlan.Username);

            if (userBenefitDaily != null)
            {
                userBenefitDaily.AcumulatedBenefitperDay = 
                    (float)Math.Round(userBenefitDaily.AcumulatedBenefitperDay + Math.Round(plan.DailyBenefit, 2), 2);

                Console.WriteLine($"AcumulatedBenefitperDay actualizado para {userPlan.Username}: {userBenefitDaily.AcumulatedBenefitperDay}");
            }
            else
            {
                var newBenefitDaily = new UserBenefitDaily
                {
                    Username = userPlan.Username,
                    AcumulatedBenefitperDay = (float)Math.Round(plan.DailyBenefit, 2)
                };

                userBenefitDailies.Add(newBenefitDaily);
                Console.WriteLine($"Nuevo UserBenefitDaily agregado para {userPlan.Username}");
            }
        }

        foreach (var userPlan in userplans)
        {
            Console.WriteLine($"Entrando a la actualización del usuario {userPlan.Username}");
            var plan = await context.Plans.FirstOrDefaultAsync(option => option.Name == userPlan.NamePlan);

            if (plan == null) continue;

            if (userPlan.Percentage < 100)
            {
                var existingUpdatePlan = await context.UpdatePlansForUser
                    .FirstOrDefaultAsync(option => option.Username == userPlan.Username);

                var benefitPerHour = Math.Round(plan.DailyBenefit / 24, 2);

                if (existingUpdatePlan == null)
                {
                    await AddNewPlan(userPlan, plan, benefitPerHour);
                }
                else{
                    await UpdateExistingPlan(userPlan, plan, benefitPerHour, userBenefitDailies, existingUpdatePlan);
                }
            }
            else
            {
                context.UserPlans.Remove(userPlan);
                await context.SaveChangesAsync();
                Console.WriteLine($"Plan de usuario {userPlan.Username} eliminado porque alcanzó el 100% del beneficio.");
            }
        }
        userBenefitDailies.Clear();
        Console.WriteLine("Lista de beneficios diarios limpia.");
    }

    private async Task AddNewPlan(UserPlans userPlan, Plan plan, double benefitPerHour)
    {
        var newUpdatePlan = new UpdatePlansForUser
        {
            Username = userPlan.Username,
            AcumulatedBenefitperHour = benefitPerHour,
            AcumulatedTotalBenefit = benefitPerHour
        };

        await context.UpdatePlansForUser.AddAsync(newUpdatePlan);
        await context.SaveChangesAsync();

        Console.WriteLine($"Nuevo UpdatePlansForUser creado para {userPlan.Username} con beneficio por hora: {benefitPerHour}");

        await UpdateUserPlan(userPlan, plan, benefitPerHour);
    }

    private async Task UpdateExistingPlan(
        UserPlans userPlan,
        Plan plan,
        double benefitPerHour,
        List<UserBenefitDaily> userBenefitDailies,
        UpdatePlansForUser existingUpdatePlan)
    {
        var userBenefitDaily = userBenefitDailies.FirstOrDefault(option => option.Username == userPlan.Username);

        if (userBenefitDaily != null && existingUpdatePlan.AcumulatedBenefitperHour < userBenefitDaily.AcumulatedBenefitperDay)
        {
            existingUpdatePlan.AcumulatedBenefitperHour = 
                Math.Round(existingUpdatePlan.AcumulatedBenefitperHour + benefitPerHour, 2);
            existingUpdatePlan.AcumulatedTotalBenefit = 
                Math.Round(existingUpdatePlan.AcumulatedTotalBenefit + benefitPerHour, 2);

            Console.WriteLine($"AcumulatedBenefitperHour : {existingUpdatePlan.AcumulatedBenefitperHour} y AcumulatedTotalBenefit : {existingUpdatePlan.AcumulatedTotalBenefit} actualizados para {userPlan.Username}");
        }
        else
        {
            existingUpdatePlan.AcumulatedBenefitperHour = 
                Math.Round(benefitPerHour, 2);
            existingUpdatePlan.AcumulatedTotalBenefit = 
                Math.Round(existingUpdatePlan.AcumulatedTotalBenefit + benefitPerHour, 2);

            Console.WriteLine($"AcumulatedBenefitperHour restablecido para {userPlan.Username}: {existingUpdatePlan.AcumulatedBenefitperHour}");
        }
        context.Entry(existingUpdatePlan).State = EntityState.Modified;
        await context.SaveChangesAsync();

        await UpdateUserPlan(userPlan, plan, benefitPerHour);
    }

    private async Task UpdateUserPlan(UserPlans userPlan, Plan plan, double benefitPerHour)
    {
        userPlan.Percentage = Math.Round(userPlan.Percentage + benefitPerHour / plan.TotalBenefit * 100, 2);

        context.Entry(userPlan).State = EntityState.Modified;
        await context.SaveChangesAsync();

        Console.WriteLine($"Porcentaje actualizado para {userPlan.Username}: {userPlan.Percentage}");

        var wallet = await context.Wallets.FirstOrDefaultAsync(option => option.Email == userPlan.Username);
        if (wallet != null)
        {
            wallet.Balance += (float)benefitPerHour;

            context.Entry(wallet).State = EntityState.Modified;
            await context.SaveChangesAsync();

            Console.WriteLine($"Balance de wallet actualizado para {userPlan.Username}: {wallet.Balance}");
        }
    }
}



