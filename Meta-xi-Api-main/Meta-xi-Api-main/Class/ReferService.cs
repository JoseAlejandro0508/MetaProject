using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Meta_xi.Application;

public class ReferService
{

    private readonly DBContext context;


    public ReferService(DBContext dBContext)
    {
        context = dBContext;
    }

    public async Task ReferEarnExecute(int UID, double Amount,int lvl = 1)
    {
        if(lvl > 3) return;
   
        Dictionary<int, float> Comision = new Dictionary<int, float>
        {
            { 1, 0.08f },
            { 2, 0.05f },
            { 3, 0.02f }
        };

        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == UID);
        if(user == null) return;
        var father = await context.Users.FirstOrDefaultAsync(u => u.Code == user.ReferCode);
        if(father == null) return;
        var wallet = await context.Wallets.FirstOrDefaultAsync(w => w.Email == father.Email||w.Email == father.PhoneNumber);
        if(wallet == null) return;
        wallet.Balance += (float)Amount * Comision[lvl];
        await context.SaveChangesAsync();
        await ReferEarnExecute(father.Id, Amount, lvl + 1);
        



    }
    public async Task<Dictionary<int, List<User>>> GetCurrentRef(int UID, int SecondsTemporality)
    {
        List<User> allUser = await context.Users.ToListAsync();

        var selectedUser = allUser.FirstOrDefault(x => x.Id == UID);


        Dictionary<int, List<User>> _refers = new Dictionary<int, List<User>>
        {

            { 1,new List<User>()},
            { 2,new List<User>()},
            { 3,new List<User>()},
        };
        if (selectedUser == null)
        {
            return _refers;
        }
        async Task GetReferHierarchy(string Code, int lvl = 1)
        {
            if (lvl > 3) return;
            List<User> RefLVL = allUser.Where(u => u.ReferCode == Code).ToList();
            foreach (User u in RefLVL)
            {
                if (SecondsTemporality == 0 || u.CreatedAt > DateTimeOffset.Now.AddSeconds(-SecondsTemporality))
                {
                    _refers[lvl].Add(u);
                }
                await GetReferHierarchy(u.Code!, lvl + 1);
            }

        }
        await GetReferHierarchy(selectedUser.Code!);


        return _refers;
    }
}