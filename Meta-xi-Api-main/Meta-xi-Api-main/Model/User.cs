using System.ComponentModel.DataAnnotations;

namespace Meta_xi.Application;
public class User
{
    [Key]
    public int Id { get; set; }
    public required string? Email { get; set; }
    public required string? PhoneNumber { get; set; }
    public required string Password { get; set; }
    public required string Token { get ; set ;}
    public required string? Code { get; set; }
    public required ICollection<ReferLevel1>? referLevel1s { get ; set ; }
    public required ICollection<ReferLevel2>? referLevel2s { get ; set ; }
    public required ICollection<ReferLevel3>? referLevel3s { get ; set ; }
    public required Wallet? Wallet { get ; set ; }
}