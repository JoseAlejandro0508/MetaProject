using System.ComponentModel.DataAnnotations;

namespace Meta_xi.Application;

public class DepositHistory
{
    [Key]
    public int Id { get; set; }
    public required string OrdenId { get; set; }
    public required string Email { get; set; }
    public required float Amount { get; set; }
    public required string Token { get; set; }
    public string Status { get; set; } = "Pendiente";
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;
}