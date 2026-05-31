using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Meta_xi.Application;

[ApiController]
[Route("api/[controller]")]
public class ReferController : ControllerBase
{
    private readonly DBContext context;
    private ReferService referService;
    public ReferController(DBContext dbcontext, ReferService _referService)
    {
        context = dbcontext;
        referService = _referService;
    }
    public class TeamInfo
    {
        public int CountLVL1 { get; set; }
        public int CountLVL2 { get; set; }
        public int CountLVL3 { get; set; }
        public double EarnLVL1 { get; set; }
        public double EarnLVL2 { get; set; }
        public double EarnLVL3 { get; set; }
        public int CountTotal { get; set; }
        public int CountToday { get; set; }
        public double TeamRecharge { get; set; }
        public double TeamWithdraw { get; set; }



    }
    [HttpGet("GetReferrer/{id}")]
    public async Task<IActionResult> GetRefered(string id)
    {
        var user = await context.Users.Where(u => u.PhoneNumber == id).FirstOrDefaultAsync();
        if (user == null)
        {
            return NotFound("Usuario no encontrado");
        }
        var refer = await referService.GetCurrentRef(user.Id, 0);
        var referToday = await referService.GetCurrentRef(user.Id, 3600 * 24);
        double TotalDeposit = 0, TotalWithdraw = 0;
        Dictionary<int, double> referEarn = new Dictionary<int, double>
        {
            { 1, 0 },
            { 2, 0 },
            { 3, 0 }
        };
        Dictionary<int, double> Comision = new Dictionary<int, double>
        {
            { 1, 0.08 },
            { 2, 0.05 },
            { 3, 0.02 }
        };
        for (int i = 1; i <= 3; i++)
        {
            foreach (var u in refer[i])
            {
                var userWallet = await context.Wallets.FirstOrDefaultAsync(option => option.Email == u.Email);
                if (userWallet == null) continue;

                TotalDeposit += userWallet.TotalRecharged;
                TotalWithdraw += userWallet.TotalWithdrawn;
                referEarn[i] += userWallet.TotalRecharged * Comision[i];
            }

        }



        TeamInfo quantityRefersLevelsBack = new TeamInfo
        {
            CountLVL1 = refer[1].Count,
            CountLVL2 = refer[2].Count,
            CountLVL3 = refer[3].Count,
            EarnLVL1 = referEarn[1],
            EarnLVL2 = referEarn[2],
            EarnLVL3 = referEarn[3],
            CountTotal = refer[1].Count + refer[2].Count + refer[3].Count,
            CountToday = referToday[1].Count + referToday[2].Count + referToday[3].Count,
            TeamRecharge = TotalDeposit,
            TeamWithdraw = TotalWithdraw

        };
        return Ok(quantityRefersLevelsBack);
    }
}