using System.ComponentModel.DataAnnotations;

namespace Meta_xi.Application;

public class WithdrawalHistory
{
    [Key]
    public int Id { get; set; }
    public required string Email { get; set; }
    public required float Amount { get; set; }
    public required float Fee { get; set; }
    public required float NetAmount { get; set; }
    public required string Token { get; set; }
    public required string AccountNumber { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;
}