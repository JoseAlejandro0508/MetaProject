using Microsoft.EntityFrameworkCore;

namespace Meta_xi.Application;
public class RegisteredToReferLevels : IRegisteredToReferLevel
{
    private readonly DBContext context;

    //Constructor
    public RegisteredToReferLevels(DBContext dBContext)
    {
        context = dBContext;
    }

    public async Task VerifyToReferLevel1(string UniqueCodeReferrer, string UniqueCodeReFerred)
    {
        var user = await context.Users.FirstOrDefaultAsync(option => option.Code == UniqueCodeReferrer);
        if (user == null)
        {
            return ;
        }
        ReferLevel1 referLevel1 = new ReferLevel1
        {
            IDUserReferrer = user.Id,
            UniqueCodeReFerred = UniqueCodeReFerred,
            UniqueCodeReferrer = UniqueCodeReferrer,
            User = null
        };
        await context.ReferLevel1s.AddAsync(referLevel1);
        await context.SaveChangesAsync();
        Console.WriteLine("Se ha registrado el nivel 1");
        return ;
    }

    public async Task VerifyToReferLevel2(string UniqueCodeReferrer, string UniqueCodeReFerred)
    {
        var user = await context.Users.FirstOrDefaultAsync(option => option.Code == UniqueCodeReferrer);
        if (user == null)
        {
            return ;
        }
        ReferLevel2 referLevel2 = new ReferLevel2
        {
            IDUserReferrer = user.Id,
            UniqueCodeReFerred = UniqueCodeReFerred,
            UniqueCodeReferrer = UniqueCodeReferrer,
            User = null
        };
        await context.ReferLevel2s.AddAsync(referLevel2);
        await context.SaveChangesAsync();
        return ;
    }

    public async Task VerifyToReferLevel3(string UniqueCodeReferrer, string UniqueCodeReFerred)
    {
        var user = await context.Users.FirstOrDefaultAsync(option => option.Code == UniqueCodeReferrer);
        if (user == null)
        {
            return ;
        }
        ReferLevel3 referLevel3 = new ReferLevel3
        {
            IDUserReferrer = user.Id,
            UniqueCodeReFerred = UniqueCodeReFerred,
            UniqueCodeReferrer = UniqueCodeReferrer,
            User = null
        };
        await context.ReferLevel3s.AddAsync(referLevel3);
        await context.SaveChangesAsync();
        return;
    }


}