using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Meta_xi.Application;
[ApiController]
[Route("api/[controller]")]
public class UserPlansController : ControllerBase
{
    private readonly DBContext context;
    public UserPlansController(DBContext dBContext)
    {
        context = dBContext;
    }
    [HttpPost("UserBuyPlans")]
    public async Task<IActionResult> UserBuyPlans(BuyPlans buyPlans)
    {
        var user = await context.Users.FirstOrDefaultAsync(option => option.Email == buyPlans.Username);
        if (user == null)
        {
            return NotFound(new { message = "Usuario no encontrado" });
        }
        var plan = await context.Plans.FirstOrDefaultAsync(option => option.Name == buyPlans.IdPlan);
        if (plan == null)
        {
            return NotFound(new { message = "Plan no encontrado" });
        }
        var userPlans = await context.UserPlans.Where(option => option.Username == user.Email && option.NamePlan == plan.Name).ToListAsync();
        Console.WriteLine(userPlans.Count);
        if (userPlans.Count >= plan.MaxQuantity)
        {
            return NotFound(new { message = "El usuario ya tiene el maximo de compras para este plan" });
        }
        Console.WriteLine("Puede comprar un plan de este tipo");
        var wallet = await context.Wallets.FirstOrDefaultAsync(option => option.Email == buyPlans.Username);
        if (wallet == null)
        {
            return NotFound(new { message = "Wallet no encontrada" });
        }
        if (wallet.Balance > plan.Price)
        {
            Console.WriteLine("Tiene saldo suficiente para comprar este plan");
            UserPlans userPlans1 = new UserPlans
            {
                Username = buyPlans.Username,
                NamePlan = plan.Name,
                DatePlan = DateTime.UtcNow,
                Percentage = 0
            };
            await context.UserPlans.AddAsync(userPlans1);
            await context.SaveChangesAsync();
            float balance = (float)plan.Price;
            wallet.Balance = wallet.Balance - balance;
            Console.WriteLine(wallet.Balance);
            context.Entry(wallet).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }
        else
        {
            return NotFound(new { message = "No tiene saldo suficiente" });
        }
        return Ok(new { message = "Plan comprado con exito" });
    }
    [HttpGet("GetUserPlans/{username}")]
    public async Task<IActionResult> UserPlans(string username)
    {
        var user = await context.Users.FirstOrDefaultAsync(option => option.Email == username || option.PhoneNumber == username);
        if (user == null)
        {
            return NotFound(new { message = "Usuario no encontrado" });
        }
        var userPlans = await context.UserPlans.Where(i => i.Username == username).ToListAsync();
        if (!userPlans.Any())
        {
            return NotFound(new { message = "El usuario no ha comprado ningún plan" });
        }
        var plans = await context.Plans.ToListAsync();
        List<MyPlan> myPlans = new List<MyPlan>();
        foreach (var i in userPlans)
        {
            foreach (var j in plans)
            {
                if (j.Name == i.NamePlan)
                {
                    DateTime dateTime = DateTime.UtcNow;
                    TimeSpan diference = dateTime.Subtract(i.DatePlan);
                    int daysRemaining = diference.Days;
                    double HourBenefit = Math.Round(j.DailyBenefit / 24, 2);
                    MyPlan myPlan = new MyPlan
                    {
                        DailyBenefit = j.DailyBenefit,
                        DaysRemaining = j.DaysActive - daysRemaining,
                        HourBenefit = HourBenefit,
                        Percentage = i.Percentage,
                        TotalBenefit = j.TotalBenefit,
                        IdPlan = j.IDPlan,
                        Name = j.Name
                    };
                    myPlans.Add(myPlan);
                }
            }

        }
        return Ok(myPlans);
    }
    [HttpGet("GetBalaceToUser/{username}")]
    public async Task<IActionResult> GetBalanceToUser(string username){
        var benefit = await context.UpdatePlansForUser.FirstOrDefaultAsync(option => option.Username == username);
        if(benefit == null){
            return NotFound(new { message = "El usuario no tiene ninguna información de beneficios" });
        }else{
            return Ok(benefit);
        }
    }

}