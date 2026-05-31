using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Meta_xi.Application;

[ApiController]
[Route("api/[controller]")]
public class MisionsUserController : ControllerBase
{
    private readonly DBContext context;

    public MisionsUserController(DBContext dbContext)
    {
        context = dbContext;
    }

    // ── Helper: Get referral count for a user ──────────────────────
    private async Task<int> GetReferralCountAsync(int userId)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return 0;
        return await context.Users.CountAsync(u => u.ReferCode == user.Code);
    }

    // ── Helper: Find user by email or phone ─────────────────────────
    private async Task<(User? user, string? email)> FindUserAsync(string username)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == username || u.PhoneNumber == username);
        if (user == null) return (null, null);
        return (user, user.PhoneNumber);
    }

    // ── GET: api/MisionsUser/GetDates/{username} ────────────────────
    [HttpGet("GetDates/{username}")]
    public async Task<IActionResult> GetDates(string username)
    {
        var (user, email) = await FindUserAsync(username);
        if (user == null) return NotFound(new { message = "Usuario no encontrado" });

        var referralCount = await GetReferralCountAsync(user.Id);

        // Get all missions
        var allMissions = await context.Missions.ToListAsync();

        // Get claimed mission IDs for this user
        var claimedMissionIds = await context.UserMissions
            .Where(um => um.Email == email && um.ClaimedAt != null)
            .Select(um => um.MissionId)
            .ToListAsync();

        var claimedSet = new HashSet<int>(claimedMissionIds);

        // disponibility: sum of Gift for missions where referralCount >= Ref AND not yet claimed
        float disponibility = allMissions
            .Where(m => referralCount >= m.Ref && !claimedSet.Contains(m.Id))
            .Sum(m => m.Gift);

        // quantityMisionsToday: claimed today (materialize first — EF Core+SQLite can't translate DateTimeOffset comparisons)
        var today = DateTimeOffset.UtcNow.Date;
        var userMissionsToday = await context.UserMissions
            .Where(um => um.Email == email && um.ClaimedAt != null)
            .ToListAsync();
        var quantityMisionsToday = userMissionsToday
            .Count(um => um.ClaimedAt!.Value.Date == today);

        // quantityMisions: total claimed ever
        var quantityMisions = await context.UserMissions
            .CountAsync(um => um.Email == email && um.ClaimedAt != null);

        return Ok(new { disponibility, quantityMisionsToday, quantityMisions });
    }

    // ── GET: api/MisionsUser/GetMissions/{username} ─────────────────
    [HttpGet("GetMissions/{username}")]
    public async Task<IActionResult> GetMissions(string username)
    {
        var (user, email) = await FindUserAsync(username);
        if (user == null) return NotFound(new { message = "Usuario no encontrado" });

        var referralCount = await GetReferralCountAsync(user.Id);

        var claimedMissionIds = await context.UserMissions
            .Where(um => um.Email == email && um.ClaimedAt != null)
            .Select(um => um.MissionId)
            .ToListAsync();

        var claimedSet = new HashSet<int>(claimedMissionIds);

        var missions = await context.Missions
            .Where(m => m.Type == MissionType.Normal)
            .ToListAsync();

        var result = missions.Select(m => new
        {
            id = m.Id,
            title = m.Title,
            reward = m.Gift,
            progress = Math.Min(referralCount, m.Ref),
            goal = m.Ref,
            claimed = claimedSet.Contains(m.Id)
        }).ToList();

        return Ok(result);
    }

    // ── POST: api/MisionsUser/LogToClaim ────────────────────────────
    [HttpPost("LogToClaim")]
    public async Task<IActionResult> LogToClaim([FromBody] ClaimMissionRequest request)
    {
        var (user, email) = await FindUserAsync(request.Username);
        if (user == null) return NotFound(new { message = "Usuario no encontrado" });

        var mission = await context.Missions.FirstOrDefaultAsync(m => m.Id == request.IdMission);
        if (mission == null) return NotFound(new { message = "Misión no encontrada" });

        var referralCount = await GetReferralCountAsync(user.Id);
        if (referralCount < mission.Ref)
            return BadRequest(new { message = "No has completado esta misión" });

        // Check if already claimed
        var existingClaim = await context.UserMissions
            .FirstOrDefaultAsync(um => um.Email == email && um.MissionId == mission.Id);
        if (existingClaim != null)
            return BadRequest(new { message = "Esta misión ya fue reclamada" });

        // Find wallet
        var wallet = await context.Wallets.FirstOrDefaultAsync(w => w.Email == email);
        if (wallet == null) return NotFound(new { message = "Cartera no encontrada" });

        // Atomic: update wallet + create UserMission in single SaveChanges
        wallet.Balance += mission.Gift;
        context.Wallets.Update(wallet);

        var userMission = new UserMission
        {
            Email = email!,
            MissionId = mission.Id,
            ClaimedAt = DateTimeOffset.Now
        };
        await context.UserMissions.AddAsync(userMission);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return BadRequest(new { message = "Esta misión ya fue reclamada" });
        }

        return Ok(new { message = "Misión reclamada correctamente" });
    }

    // ── GET: api/MisionsUser/UpdateWallet/{username} ────────────────
    [HttpGet("UpdateWallet/{username}")]
    public async Task<IActionResult> UpdateWallet(string username)
    {
        var (user, email) = await FindUserAsync(username);
        if (user == null) return NotFound(new { message = "Usuario no encontrado" });

        var wallet = await context.Wallets.FirstOrDefaultAsync(w => w.Email == email);
        if (wallet == null) return NotFound(new { message = "Cartera no encontrada" });

        var referralCount = await GetReferralCountAsync(user.Id);
        var allMissions = await context.Missions.ToListAsync();

        // Get already-claimed mission IDs
        var claimedMissionIds = await context.UserMissions
            .Where(um => um.Email == email && um.ClaimedAt != null)
            .Select(um => um.MissionId)
            .ToListAsync();

        var claimedSet = new HashSet<int>(claimedMissionIds);

        // Find eligible missions: referralCount >= Ref AND not yet claimed
        var claimable = allMissions
            .Where(m => referralCount >= m.Ref && !claimedSet.Contains(m.Id))
            .ToList();

        if (!claimable.Any())
            return BadRequest(new { message = "No hay misiones disponibles para reclamar" });

        float totalClaimed = 0;
        int missionsClaimed = 0;

        foreach (var mission in claimable)
        {
            wallet.Balance += mission.Gift;
            totalClaimed += mission.Gift;
            missionsClaimed++;

            var userMission = new UserMission
            {
                Email = email!,
                MissionId = mission.Id,
                ClaimedAt = DateTimeOffset.Now
            };
            await context.UserMissions.AddAsync(userMission);
        }

        context.Wallets.Update(wallet);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return BadRequest(new { message = "Error al reclamar misiones" });
        }

        return Ok(new { message = "Billetera actualizada correctamente", totalClaimed, missionsClaimed });
    }

    // ── Request DTOs ───────────────────────────────────────────────
    public class ClaimMissionRequest
    {
        public int IdMission { get; set; }
        public string Username { get; set; } = string.Empty;
    }
}