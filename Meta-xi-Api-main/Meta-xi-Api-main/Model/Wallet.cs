using System.ComponentModel.DataAnnotations;

namespace Meta_xi.Application;

public class Wallet
{
    [Key]
    public int IdWallet { get; set; }
    public required string Email { get ; set ; }
    public required float Balance { get ; set ; }
    public float TotalRecharged { get; set; } = 0;
    public float TotalWithdrawn { get; set; } = 0;

}