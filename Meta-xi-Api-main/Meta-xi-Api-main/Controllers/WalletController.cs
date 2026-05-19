using Meta.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Meta_xi.Application;

[ApiController]
[Route("api/[controller]")]
public class WalletController : ControllerBase
{
    private readonly DBContext context;
    private readonly UserService userService;
    public WalletController(DBContext _context, UserService _userService)
    {
        context = _context;
        userService = _userService;
    }

    // ── Hardcoded task definitions (mirrors TasksController) ──────────
    private static readonly List<TaskObject> taskPrizes = new()
    {
        new TaskObject{ TaskId = 1, friends = 1, time = 2, prize = 5000 },
        new TaskObject{ TaskId = 2, friends = 10, time = 12, prize = 100000 },
        new TaskObject{ TaskId = 3, friends = 5, time = 10, prize = 50000 },
        new TaskObject{ TaskId = 4, friends = 8, time = 24, prize = 90000 },
        new TaskObject{ TaskId = 5, friends = 15, time = 24, prize = 200000 },
        new TaskObject{ TaskId = 6, friends = 20, time = 48, prize = 300000 }
    };

    // ── DTOs ───────────────────────────────────────────────────────────
    public class AccountSummaryDTO
    {
        public float TotalEarned { get; set; }
        public string TotalInvested { get; set; } = "N/A";
        public float TotalRecharged { get; set; }
        public float TotalWithdrawn { get; set; }
        public float TaskEarnings { get; set; }
        public float PlanEarnings { get; set; }
        public float ReferralEarnings { get; set; }
        public string AccountStatus { get; set; } = "VERIFICADA";
    }

    public class WithdrawalRequestDTO
    {
        public string Email { get; set; } = string.Empty;
        public float Amount { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // ── GET: api/Wallet/GetAccountSummary/{username} ───────────────────
    [HttpGet("GetAccountSummary/{username}")]
    public async Task<IActionResult> GetAccountSummary(string username)
    {
        // 1. Find user by email or phone
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == username || u.PhoneNumber == username);
        if (user == null)
        {
            return NotFound(new { message = "Usuario no encontrado" });
        }

        // 2. Find wallet
        var wallet = await context.Wallets.FirstOrDefaultAsync(w => w.Email == username);
        if (wallet == null)
        {
            return NotFound(new { message = "Cartera no encontrada" });
        }

        // 3. Compute Task Earnings
        var completedTaskRegisters = await context.TaskRegisters
            .Where(tr => tr.UID == user.Id && tr.Completed == true)
            .ToListAsync();
        float taskEarnings = 0;
        foreach (var tr in completedTaskRegisters)
        {
            var taskObj = taskPrizes.FirstOrDefault(t => t.TaskId == tr.TaskId);
            if (taskObj != null)
            {
                taskEarnings += (float)taskObj.prize;
            }
        }

        // 4. Compute Plan Earnings
        var userPlans = await context.UpdatePlansForUser
            .Where(p => p.Username == username)
            .ToListAsync();
        float planEarnings = (float)userPlans.Sum(p => p.AcumulatedTotalBenefit);

        // 5. Compute Referral Earnings from DepositHistory
        // Level 1: 5%
        var lvl1Refs = await context.ReferLevel1s.Where(r => r.IDUserReferrer == user.Id).ToListAsync();
        var lvl1Codes = lvl1Refs.Select(r => r.UniqueCodeReFerred).ToList();
        var lvl1Users = await context.Users.Where(u => lvl1Codes.Contains(u.Code)).ToListAsync();
        var lvl1Emails = lvl1Users.Select(u => u.Email).ToList();
        var lvl1Deposits = await context.DepositHistories.Where(d => lvl1Emails.Contains(d.Email)).ToListAsync();
        float referralEarnings = lvl1Deposits.Sum(d => d.Amount) * 0.05f;

        // Level 2: 3%
        var lvl2Refs = await context.ReferLevel2s.Where(r => r.IDUserReferrer == user.Id).ToListAsync();
        var lvl2Codes = lvl2Refs.Select(r => r.UniqueCodeReFerred).ToList();
        var lvl2Users = await context.Users.Where(u => lvl2Codes.Contains(u.Code)).ToListAsync();
        var lvl2Emails = lvl2Users.Select(u => u.Email).ToList();
        var lvl2Deposits = await context.DepositHistories.Where(d => lvl2Emails.Contains(d.Email)).ToListAsync();
        referralEarnings += lvl2Deposits.Sum(d => d.Amount) * 0.03f;

        // Level 3: 1%
        var lvl3Refs = await context.ReferLevel3s.Where(r => r.IDUserReferrer == user.Id).ToListAsync();
        var lvl3Codes = lvl3Refs.Select(r => r.UniqueCodeReFerred).ToList();
        var lvl3Users = await context.Users.Where(u => lvl3Codes.Contains(u.Code)).ToListAsync();
        var lvl3Emails = lvl3Users.Select(u => u.Email).ToList();
        var lvl3Deposits = await context.DepositHistories.Where(d => lvl3Emails.Contains(d.Email)).ToListAsync();
        referralEarnings += lvl3Deposits.Sum(d => d.Amount) * 0.01f;

        // 6. Build response
        var summary = new AccountSummaryDTO
        {
            TaskEarnings = taskEarnings,
            PlanEarnings = planEarnings,
            ReferralEarnings = referralEarnings,
            TotalRecharged = wallet.TotalRecharged,
            TotalWithdrawn = wallet.TotalWithdrawn,
            TotalInvested = "N/A",
            AccountStatus = "VERIFICADA"
        };
        summary.TotalEarned = summary.TaskEarnings + summary.PlanEarnings + summary.ReferralEarnings;

        return Ok(summary);
    }

    // ── POST: api/Wallet/RequestWithdrawal ────────────────────────────
    [HttpPost("RequestWithdrawal")]
    public async Task<IActionResult> RequestWithdrawal([FromBody] WithdrawalRequestDTO request)
    {
        // Validate amount limits
        if (request.Amount < 20000)
        {
            return BadRequest(new { message = "Monto mínimo de retiro: 20,000 COP" });
        }
        if (request.Amount > 10000000)
        {
            return BadRequest(new { message = "Monto máximo de retiro: 10,000,000 COP" });
        }

        // Find user
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email || u.PhoneNumber == request.Email);
        if (user == null)
        {
            return NotFound(new { message = "Usuario no encontrado" });
        }

        // Verify password
        if (!userService.verifyPassword(request.Password, user.Password))
        {
            return BadRequest(new { message = "Contraseña incorrecta" });
        }

        // Find wallet
        var wallet = await context.Wallets.FirstOrDefaultAsync(w => w.Email == request.Email);
        if (wallet == null)
        {
            return NotFound(new { message = "Cartera no encontrada" });
        }

        // Check sufficient balance
        if (request.Amount > wallet.Balance)
        {
            return BadRequest(new { message = "Saldo insuficiente" });
        }

        // Compute fee (8%)
        float fee = request.Amount * 0.08f;
        float netAmount = request.Amount - fee;

        // Deduct from wallet
        wallet.Balance -= request.Amount;
        wallet.TotalWithdrawn += request.Amount;
        context.Entry(wallet).State = EntityState.Modified;

        // Create withdrawal history record
        var withdrawal = new WithdrawalHistory
        {
            Email = request.Email,
            Amount = request.Amount,
            Fee = fee,
            NetAmount = netAmount,
            Token = request.Token,
            AccountNumber = request.AccountNumber
        };
        await context.WithdrawalHistories.AddAsync(withdrawal);
        await context.SaveChangesAsync();

        return Ok(new
        {
            message = "Retiro procesado correctamente",
            amount = request.Amount,
            fee = fee,
            netAmount = netAmount,
            token = request.Token
        });
    }

    // ── POST: api/Wallet/UpdateBalance (MODIFIED) ─────────────────────
    [HttpPost("UpdateBalance")]
    public async Task<IActionResult> UpdateBalance(UpdateBalance updateBalance){
        GetMoneyValues getMoneyValues = new GetMoneyValues();
        var wallet = await context.Wallets.FirstOrDefaultAsync(option => option.Email == updateBalance.Email);
        if(wallet == null){
            return NotFound(new { message = "No existe ninguna cartera con ese correo" });
        }
        string token = updateBalance.Token.ToLower();
        float depositAmountCop = 0;

        switch (token){
            case "nequi":
                depositAmountCop = updateBalance.Balance;
                wallet.Balance = updateBalance.Balance;
                break;
            case "trx":
                decimal balance = await getMoneyValues.GetMoneyValueAsync("trx");
                float value = (float)balance;
                Console.WriteLine(value);
                decimal usdToCop = await getMoneyValues.GetMoneyValueAsync("cop");
                float usd = (float)usdToCop;
                Console.WriteLine(usd);
                depositAmountCop = value * updateBalance.Balance * usd;
                wallet.Balance = wallet.Balance + depositAmountCop;
                break;
            case "usdt_trc20":
                decimal balance2 = await getMoneyValues.GetMoneyValueAsync("tether");
                float value2 = (float)balance2;
                decimal usdToCop2 = await getMoneyValues.GetMoneyValueAsync("cop");
                float usd2 = (float)usdToCop2;
                depositAmountCop = value2 * updateBalance.Balance * usd2;
                wallet.Balance = wallet.Balance + depositAmountCop;
                break;
            case "paypal":
                decimal balance3 = await getMoneyValues.GetMoneyValueAsync("cop");
                float value3 = (float)balance3;
                depositAmountCop = value3 * updateBalance.Balance;
                wallet.Balance = wallet.Balance + depositAmountCop;
                break;
            case "usdt_bep20":
                decimal balance4 = await getMoneyValues.GetMoneyValueAsync("tether");
                float value4 = (float)balance4;
                decimal usdToCop4 = await getMoneyValues.GetMoneyValueAsync("cop");
                float usd4 = (float)usdToCop4;
                depositAmountCop = value4 * updateBalance.Balance * usd4;
                wallet.Balance = wallet.Balance + depositAmountCop;
                break;
            case "breb":
                depositAmountCop = updateBalance.Balance;
                wallet.Balance = updateBalance.Balance;
                break;
            default:
                return NotFound(new { message = "Token no soportado" });
        }

        // Update wallet tracking and create deposit history
        wallet.TotalRecharged += depositAmountCop;
        context.Entry(wallet).State = EntityState.Modified;

        var deposit = new DepositHistory
        {
            Email = updateBalance.Email,
            Amount = depositAmountCop,
            Token = token
        };
        await context.DepositHistories.AddAsync(deposit);
        await context.SaveChangesAsync();

        return Ok(new { message = "Balance actualizado correctamente" });
    }

    [HttpGet("GetBalance/{username}")]
    public async Task<IActionResult> GetBalance(string username){
        var wallet = await context.Wallets.FirstOrDefaultAsync(option => option.Email == username);
        if(wallet == null){
            return NotFound(new { message = "El usuario no posee ninguna cartera"});
        }
        return Ok(wallet.Balance);
    }

    //Obtener balance en COP y USD
    [HttpGet("GetBalanceUsdAndCop/{username}")]
    public async Task<IActionResult> GetBalanceUsdAndCop(string username){
        var wallet = await context.Wallets.FirstOrDefaultAsync(option => option.Email == username);
        if(wallet == null){
            return NotFound(new { message = "El usuario no posee ninguna cartera"});
        }

        float balanceInCop = wallet.Balance;
        float balanceInUsd = 0;

        try {
            GetMoneyValues getMoneyValues = new GetMoneyValues();
            decimal usdToCop = await getMoneyValues.GetMoneyValueAsync("cop");
            if(usdToCop > 0){
                balanceInUsd = (float)Math.Round(balanceInCop / (float)usdToCop, 2);
            }
        } catch (Exception) {
            // Si no se puede obtener la tasa, devolver 0
            balanceInUsd = 0;
        }

        return Ok(new { balanceInCop, balanceInUsd });
    }
}