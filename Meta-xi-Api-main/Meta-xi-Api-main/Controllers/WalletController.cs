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
    private readonly ReferService referService;
    public WalletController(DBContext _context, UserService _userService, ReferService _referService)
    {
        context = _context;
        userService = _userService;
        referService = _referService;
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
        public float TotalInvested { get; set; }
        public float TotalRecharged { get; set; }
        public float TotalWithdrawn { get; set; }
        public float TaskEarnings { get; set; }
        public float PlanEarnings { get; set; }
        public float MissionEarnings { get; set; }
        public float ReferralEarnings { get; set; }
        public string AccountStatus { get; set; } = "VERIFICADA";
    }

    public class WithdrawalRequestDTO
    {
        public string OrdenId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public float Amount { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class WithdrawalScheduleDTO
    {
        public bool CanWithdraw { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Days { get; set; } = "Lunes a Viernes";
        public string Hours { get; set; } = "8:00 AM - 10:00 PM";
        public string TimeZone { get; set; } = "Hora de Cuba (CET)";
    }

    // ── Helper: Check if current time is within withdrawal hours (Colombia timezone) ──
    private WithdrawalScheduleDTO GetWithdrawalStatus()
    {
        try
        {
            var colombiaTz = TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");
            var colombiaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, colombiaTz);

            // Check weekday (Monday = 1, Friday = 5)
            var isWeekday = colombiaTime.DayOfWeek >= DayOfWeek.Monday && colombiaTime.DayOfWeek <= DayOfWeek.Friday;
            // Check hours: 8 AM to 10 PM (22:00)
            var isBusinessHours = colombiaTime.Hour >= 8 && colombiaTime.Hour < 22;

            var canWithdraw = isWeekday && isBusinessHours;

            return new WithdrawalScheduleDTO
            {
                CanWithdraw = true,
                Message = canWithdraw
                    ? "Horario operativo activo"
                    : "Fuera de horario operativo. Los retiros solo están disponibles de lunes a viernes, de 8:00 AM a 10:00 PM (hora de Colombia).",
                Days = "Lunes a Viernes",
                Hours = "8:00 AM - 10:00 PM",
                TimeZone = "Hora de Colombia (COT)"
            };
        }
        catch
        {
            // Fallback if timezone not found: allow withdrawals
            return new WithdrawalScheduleDTO
            {
                CanWithdraw = true,
                Message = "Horario operativo activo",
                Days = "Lunes a Viernes",
                Hours = "8:00 AM - 10:00 PM",
                TimeZone = "Hora de Colombia (COT)"
            };
        }
    }

    private async Task<string> GenerateOrderIdAsync(string token)
    {
        var t = (token ?? string.Empty).ToLower();
        string prefix = "ORD";
        if (t.Contains("nequi")) prefix = "NEQ";
        else if (t.Contains("bep") || t.Contains("usdt")) prefix = "USDT";

        var rng = new Random();
        string candidate;
        do
        {
            candidate = $"{prefix}{DateTime.UtcNow:yyyyMMddHHmmssfff}{rng.Next(1000, 9999)}";
        }
        while (await context.WithdrawalHistories.AnyAsync(w => w.OrdenId == candidate));

        return candidate;
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

        // 6. Compute Total Invested from UserPlans
        var userPlansPurchased = await context.UserPlans
            .Where(up => up.Username == username)
            .ToListAsync();
        var planNames = userPlansPurchased.Select(up => up.NamePlan).ToList();
        var plansInfo = await context.Plans
            .Where(p => planNames.Contains(p.Name))
            .ToListAsync();
        float totalInvested = 0;
        foreach (var up in userPlansPurchased)
        {
            var plan = plansInfo.FirstOrDefault(p => p.Name == up.NamePlan);
            if (plan != null)
            {
                totalInvested += (float)plan.Price;
            }
        }

        // 7. Compute Mission Earnings (VIP missions claimed)
        var missionEarnings = await context.UserMissions
            .Where(um => um.Email == user.Email && um.ClaimedAt != null)
            .Join(
                context.Missions,
                um => um.MissionId,
                m => m.Id,
                (um, m) => m.Gift
            )
            .SumAsync();

        // 8. Build response
        var summary = new AccountSummaryDTO
        {
            TaskEarnings = taskEarnings,
            PlanEarnings = planEarnings,
            MissionEarnings = (float)missionEarnings,
            ReferralEarnings = referralEarnings,
            TotalRecharged = wallet.TotalRecharged,
            TotalWithdrawn = wallet.TotalWithdrawn,
            TotalInvested = totalInvested,
            AccountStatus = "VERIFICADA"
        };
        summary.TotalEarned = summary.TaskEarnings + summary.PlanEarnings + summary.MissionEarnings + summary.ReferralEarnings;

        return Ok(summary);
    }

    // ── GET: api/Wallet/CanWithdraw ────────────────────────────────────
    [HttpGet("CanWithdraw")]
    public IActionResult CanWithdraw()
    {
        var status = GetWithdrawalStatus();
        return Ok(status);
    }

    // ── POST: api/Wallet/RequestWithdrawal ────────────────────────────
    [HttpPost("RequestWithdrawal")]
    public async Task<IActionResult> RequestWithdrawal([FromBody] WithdrawalRequestDTO request)
    {
        // Check withdrawal hours (Cuba timezone)
        var schedule = GetWithdrawalStatus();
        if (!schedule.CanWithdraw)
        {
            return BadRequest(new { message = schedule.Message, schedule = new { schedule.Days, schedule.Hours, schedule.TimeZone } });
        }

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

        context.Entry(wallet).State = EntityState.Modified;

        // Generate OrdenId if not provided
        if (string.IsNullOrWhiteSpace(request.OrdenId))
        {
            request.OrdenId = await GenerateOrderIdAsync(request.Token);
        }

        // Create withdrawal history record
        var withdrawal = new WithdrawalHistory
        {
            OrdenId = request.OrdenId,
            Email = request.Email,
            Amount = request.Amount,
            Fee = fee,
            NetAmount = netAmount,
            Token = request.Token,
            AccountNumber = request.AccountNumber,
            Status = "Pendiente"
        };
        await context.WithdrawalHistories.AddAsync(withdrawal);
        await context.SaveChangesAsync();

        return Ok(new
        {
            message = "Retiro procesado correctamente",
            amount = request.Amount,
            fee = fee,
            netAmount = netAmount,
            token = request.Token,
            ordenId = withdrawal.OrdenId
        });
    }

    // ── POST: api/Wallet/UpdateBalance (MODIFIED) ─────────────────────
    [HttpPost("UpdateBalance")]
    public async Task<IActionResult> UpdateBalance(UpdateBalance updateBalance)
    {
        GetMoneyValues getMoneyValues = new GetMoneyValues();
        var wallet = await context.Wallets.FirstOrDefaultAsync(option => option.Email == updateBalance.Email);
        if (wallet == null)
        {
            return NotFound(new { message = "No existe ninguna cartera con ese correo" });
        }
        string token = updateBalance.Token.ToLower();
        float depositAmountCop = 0;

        switch (token)
        {
            case "nequi":
                depositAmountCop = updateBalance.Balance;
                //wallet.Balance += updateBalance.Balance;
                break;
            case "trx":
                decimal balance = await getMoneyValues.GetMoneyValueAsync("trx");
                float value = (float)balance;
                Console.WriteLine(value);
                decimal usdToCop = await getMoneyValues.GetMoneyValueAsync("cop");
                float usd = (float)usdToCop;
                Console.WriteLine(usd);
                depositAmountCop = value * updateBalance.Balance * usd;
                //wallet.Balance = wallet.Balance + depositAmountCop;
                break;
            case "usdt_trc20":
                decimal balance2 = await getMoneyValues.GetMoneyValueAsync("tether");
                float value2 = (float)balance2;
                decimal usdToCop2 = await getMoneyValues.GetMoneyValueAsync("cop");
                float usd2 = (float)usdToCop2;
                depositAmountCop = value2 * updateBalance.Balance * usd2;
                //wallet.Balance = wallet.Balance + depositAmountCop;
                break;
            case "paypal":
                decimal balance3 = await getMoneyValues.GetMoneyValueAsync("cop");
                float value3 = (float)balance3;
                depositAmountCop = value3 * updateBalance.Balance;
                //wallet.Balance = wallet.Balance + depositAmountCop;
                break;
            case "usdt_bep20":
                decimal balance4 = await getMoneyValues.GetMoneyValueAsync("tether");
                float value4 = (float)balance4;
                decimal usdToCop4 = await getMoneyValues.GetMoneyValueAsync("cop");
                float usd4 = (float)usdToCop4;
                depositAmountCop = value4 * updateBalance.Balance * usd4;
                //wallet.Balance = wallet.Balance + depositAmountCop;
                break;
            case "breb":
                depositAmountCop = updateBalance.Balance;
                //wallet.Balance += depositAmountCop;
                break;
            default:
                return NotFound(new { message = "Token no soportado" });
        }



        var deposit = new DepositHistory
        {
            OrdenId = updateBalance.OrdenId,
            Email = updateBalance.Email,
            Amount = depositAmountCop,
            Token = token,
            Status = "Pendiente"
        };
        await context.DepositHistories.AddAsync(deposit);
        await context.SaveChangesAsync();

        return Ok(new { message = "Balance actualizado correctamente" });
    }

    [HttpGet("GetBalance/{username}")]
    public async Task<IActionResult> GetBalance(string username, [FromQuery] string coin = "COP")
    {
        var wallet = await context.Wallets.FirstOrDefaultAsync(option => option.Email == username);
        if (wallet == null)
        {
            return NotFound(new { message = "El usuario no posee ninguna cartera" });
        }

        float balance = wallet.Balance;

        if (coin.ToUpper() == "USDT")
        {
            balance = balance / 3600f;
        }

        return Ok(balance);
    }

    // ── POST: api/Wallet/AdminUpdateBalance ─────────────────────────────
    [HttpPost("AdminUpdateBalance")]
    public async Task<IActionResult> AdminUpdateBalance([FromBody] AdminUpdateBalanceDTO request)
    {
        // Validate API Key (simple header check)
        var apiKey = Request.Headers["X-Api-Key"].FirstOrDefault();
        if (string.IsNullOrEmpty(apiKey) || apiKey != Environment.GetEnvironmentVariable("ADMIN_API_KEY"))
        {
            return Unauthorized(new { message = "API Key invalida" });
        }

        var wallet = await context.Wallets.FirstOrDefaultAsync(option => option.Email == request.PhoneOrEmail);

        if (wallet == null)
        {
            return NotFound(new { message = "No existe ninguna cartera con ese usuario" });
        }



        if (wallet.Balance + request.Amount < 0)
        {
            return BadRequest(new { message = "Saldo insuficiente para realizar esta operacion" });
        }
        // Update balance (add or subtract)
        wallet.Balance += request.Amount;
        context.Entry(wallet).State = EntityState.Modified;


        await context.SaveChangesAsync();

        return Ok(new
        {
            message = "Balance actualizado correctamente",
            newBalance = wallet.Balance
        });
    }
    public class ConfirmTransaction
    {
        public string PhoneOrEmail { get; set; } = string.Empty;
        public string OrdenId { get; set; } = string.Empty;
        public string Type { get; set; } = "DEPOSIT";
    }
    [HttpPost("AdminConfirmTransaction")]
    public async Task<IActionResult> AdminConfirmTransaction([FromBody] ConfirmTransaction request)
    {

        // Validate API Key (simple header check)
        var apiKey = Request.Headers["X-Api-Key"].FirstOrDefault();
        if (string.IsNullOrEmpty(apiKey) || apiKey == Environment.GetEnvironmentVariable("ADMIN_API_KEY"))
        {
            return Unauthorized(new { message = "API Key invalida" });
        }

        var user = await context.Users.FirstOrDefaultAsync(option => option.Email == request.PhoneOrEmail || option.PhoneNumber == request.PhoneOrEmail);
        var wallet = await context.Wallets.FirstOrDefaultAsync(option => option.Email == request.PhoneOrEmail);
        if (wallet == null)
        {
            return NotFound(new { message = "No existe ninguna cartera con ese usuario" });
        }
        if (request.Type.ToUpper() == "DEPOSIT")
        {
            var DepositRegister = await context.DepositHistories.FirstOrDefaultAsync(option => option.OrdenId == request.OrdenId && option.Email == request.PhoneOrEmail);

            if (DepositRegister == null)
            {
                return NotFound(new { message = "No se encontró ningún registro de depósito para este usuario" });
            }
            DepositRegister.Status = "Éxito";
            wallet.TotalRecharged += DepositRegister.Amount;
            wallet.Balance += DepositRegister.Amount;
            context.Entry(wallet).State = EntityState.Modified;


            await context.SaveChangesAsync();
            await referService.ReferEarnExecute(user!.Id, DepositRegister.Amount);
        }
        else
        {

            var WithdrawalRegister = await context.WithdrawalHistories.FirstOrDefaultAsync(option => option.OrdenId == request.OrdenId && option.Email == request.PhoneOrEmail);

            if (WithdrawalRegister == null)
            {
                return NotFound(new { message = "No se encontró ningún registro de retiro para este usuario" });
            }

            WithdrawalRegister.Status = "Completado";
            wallet.TotalWithdrawn += WithdrawalRegister.Amount;
        }



        // Update balance (add or subtract)

        context.Entry(wallet).State = EntityState.Modified;


        await context.SaveChangesAsync();

        return Ok(new
        {
            message = "Balance actualizado correctamente",
            newBalance = wallet.Balance
        });
    }

    // ── DTO for AdminUpdateBalance ──────────────────────────────────────
    public class AdminUpdateBalanceDTO
    {
        public string PhoneOrEmail { get; set; } = string.Empty;
        public float Amount { get; set; }
    }


    // ── DTO for Transaction History ──────────────────────────────────────
    public class TransactionHistoryDTO
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public float Amount { get; set; }
        public string SignedAmount { get; set; } = string.Empty;
        public string Currency { get; set; } = "COP";
        public string Date { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public float? Fee { get; set; }
        public float? NetAmount { get; set; }
    }

    private string FormatRelativeDate(DateTimeOffset timestamp)
    {
        var now = DateTimeOffset.Now;
        var diff = now - timestamp;

        if (diff.TotalDays < 1 && timestamp.Date == now.Date)
        {
            return $"Hoy, {timestamp:hh:mm tt}";
        }
        else if (diff.TotalDays < 2 && timestamp.Date == now.Date.AddDays(-1))
        {
            return $"Ayer, {timestamp:hh:mm tt}";
        }
        else
        {
            return timestamp.ToString("dd/MM/yyyy");
        }
    }

    private string CapitalizeToken(string token)
    {
        if (string.IsNullOrEmpty(token)) return token;
        return char.ToUpper(token[0]) + token.Substring(1);
    }

    // ── GET: api/Wallet/History/{username} ──────────────────────────────
    [HttpGet("History/{username}")]
    public async Task<IActionResult> GetHistory(string username)
    {
        // Find user by email or phone
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == username || u.PhoneNumber == username);
        if (user == null)
        {
            return NotFound(new { message = "Usuario no encontrado" });
        }

        // Query deposits
        var deposits = await context.DepositHistories
            .Where(d => d.Email == username)
            .ToListAsync();

        // Query withdrawals
        var withdrawals = await context.WithdrawalHistories
            .Where(w => w.Email == username)
            .ToListAsync();

        // Map deposits to DTO
        var depositDtos = deposits.Select(d => new TransactionHistoryDTO
        {
            Id = d.Id,
            Type = "deposit",
            Title = $"Recarga {CapitalizeToken(d.Token)}",
            Amount = d.Amount,
            SignedAmount = $"+ {d.Amount:N0} COP",
            Currency = "COP",
            Date = FormatRelativeDate(d.Timestamp),
            Status = d.Status,
            Fee = null,
            NetAmount = null
        });

        // Map withdrawals to DTO
        var withdrawalDtos = withdrawals.Select(w => new TransactionHistoryDTO
        {
            Id = w.Id,
            Type = "withdrawal",
            Title = "Retiro de Saldo",
            Amount = w.Amount,
            SignedAmount = $"- {w.Amount:N0} COP",
            Currency = "COP",
            Date = FormatRelativeDate(w.Timestamp),
            Status = w.Status,
            Fee = w.Fee,
            NetAmount = w.NetAmount
        });

        // Combine and sort by date descending (using original timestamp for sorting)
        var combined = depositDtos
            .Concat(withdrawalDtos)
            .OrderByDescending(t =>
            {
                // Find original timestamp for sorting
                var deposit = deposits.FirstOrDefault(d => d.Id == t.Id && t.Type == "deposit");
                if (deposit != null) return deposit.Timestamp;
                var withdrawal = withdrawals.FirstOrDefault(w => w.Id == t.Id && t.Type == "withdrawal");
                if (withdrawal != null) return withdrawal.Timestamp;
                return DateTimeOffset.MinValue;
            })
            .ToList();

        return Ok(combined);
    }

    //Obtener balance en COP y USD
    [HttpGet("GetBalanceUsdAndCop/{username}")]
    public async Task<IActionResult> GetBalanceUsdAndCop(string username)
    {
        var wallet = await context.Wallets.FirstOrDefaultAsync(option => option.Email == username);
        if (wallet == null)
        {
            return NotFound(new { message = "El usuario no posee ninguna cartera" });
        }

        float balanceInCop = wallet.Balance;
        float balanceInUsd = 0;

        try
        {
            GetMoneyValues getMoneyValues = new GetMoneyValues();
            decimal usdToCop = await getMoneyValues.GetMoneyValueAsync("cop");
            if (usdToCop > 0)
            {
                balanceInUsd = (float)Math.Round(balanceInCop / (float)usdToCop, 2);
            }
        }
        catch (Exception)
        {
            // Si no se puede obtener la tasa, devolver 0
            balanceInUsd = 0;
        }

        return Ok(new { balanceInCop, balanceInUsd });
    }

    // ── Welcome Bonus Endpoints ─────────────────────────────────────────

    // GET: api/Wallet/CheckWelcomeBonus/{username}
    [HttpGet("CheckWelcomeBonus/{username}")]
    public async Task<IActionResult> CheckWelcomeBonus(string username)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == username || u.PhoneNumber == username);
        if (user == null)
        {
            return NotFound(new { message = "Usuario no encontrado" });
        }

        var alreadyClaimed = await context.DepositHistories
            .AnyAsync(d => d.Email == username && d.Token == "welcome_bonus");

        return Ok(new { claimed = alreadyClaimed });
    }

    // POST: api/Wallet/ClaimWelcomeBonus
    [HttpPost("ClaimWelcomeBonus")]
    public async Task<IActionResult> ClaimWelcomeBonus([FromBody] ClaimWelcomeBonusDTO request)
    {
        const float BONUS_AMOUNT = 5000f;

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email || u.PhoneNumber == request.Email);
        if (user == null)
        {
            return NotFound(new { message = "Usuario no encontrado" });
        }

        // Check if already claimed
        var alreadyClaimed = await context.DepositHistories
            .AnyAsync(d => d.Email == request.Email && d.Token == "welcome_bonus");

        if (alreadyClaimed)
        {
            return Conflict(new { message = "Bono ya reclamado" });
        }

        // Find wallet
        var wallet = await context.Wallets.FirstOrDefaultAsync(w => w.Email == request.Email);
        if (wallet == null)
        {
            return NotFound(new { message = "Cartera no encontrada" });
        }

        // Credit bonus
        wallet.Balance += BONUS_AMOUNT;

        context.Entry(wallet).State = EntityState.Modified;

        // Create deposit history record
        var deposit = new DepositHistory
        {
            OrdenId = "None", // Unique ID for this bonus transaction    
            Email = request.Email,
            Amount = BONUS_AMOUNT,
            Token = "welcome_bonus",
            Status = "Éxito"
        };
        await context.DepositHistories.AddAsync(deposit);
        await context.SaveChangesAsync();

        return Ok(new
        {
            message = "Bono reclamado exitosamente",
            amount = BONUS_AMOUNT,
            newBalance = wallet.Balance
        });
    }

    public class ClaimWelcomeBonusDTO
    {
        public string Email { get; set; } = string.Empty;
    }
}