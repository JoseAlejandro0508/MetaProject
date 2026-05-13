using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Meta_xi.Application;

[ApiController]
[Route("api/[controller]")]
public class TrendUserController : ControllerBase
{
    private readonly DBContext context;

    public TrendUserController(DBContext dbContext)
    {
        context = dbContext;
    }

    // ── Helper: Get referral count for a user ──────────────────────
    private async Task<int> GetReferralCountAsync(int userId)
    {
        return await context.ReferLevel1s
            .CountAsync(r => r.IDUserReferrer == userId);
    }

    // ── Helper: Find user by email or phone ─────────────────────────
    private async Task<(User? user, string? email)> FindUserAsync(string username)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == username || u.PhoneNumber == username);
        if (user == null) return (null, null);
        return (user, user.Email);
    }

    // ── GET: api/TrendUser/GetTendency/{username} ──────────────────
    [HttpGet("GetTendency/{username}")]
    public async Task<IActionResult> GetTendency(string username)
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
            .Where(m => m.Type == MissionType.Premium)
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

    // ── POST: api/TrendUser/LogToClaim ─────────────────────────────
    [HttpPost("LogToClaim")]
    public async Task<IActionResult> LogToClaim([FromBody] ClaimMissionRequest request)
    {
        var (user, email) = await FindUserAsync(request.Username);
        if (user == null) return NotFound(new { message = "Usuario no encontrado" });

        var mission = await context.Missions.FirstOrDefaultAsync(m => m.Id == request.IdMission);
        if (mission == null) return NotFound(new { message = "Misión no encontrada" });

        // Validate: must be Premium type
        if (mission.Type != MissionType.Premium)
            return BadRequest(new { message = "Esta misión no es de tipo Premium" });

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

        // Atomic: update wallet + create UserMission
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

    // ── GET: api/TrendUser/GetCompletedMissions/{username} ────────
    [HttpGet("GetCompletedMissions/{username}")]
    public async Task<IActionResult> GetCompletedMissions(string username)
    {
        var (user, email) = await FindUserAsync(username);
        if (user == null) return NotFound(new { message = "Usuario no encontrado" });

        var completedMissions = await context.UserMissions
            .Where(um => um.Email == email && um.ClaimedAt != null)
            .Join(
                context.Missions,
                um => um.MissionId,
                m => m.Id,
                (um, m) => new { id = m.Id, title = m.Title, reward = m.Gift }
            )
            .ToListAsync();

        return Ok(completedMissions);
    }

    // ── Request DTOs ───────────────────────────────────────────────
    public class ClaimMissionRequest
    {
        public int IdMission { get; set; }
        public string Username { get; set; } = string.Empty;
    }
}