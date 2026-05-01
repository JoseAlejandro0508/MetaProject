using System.ComponentModel.DataAnnotations;

namespace Meta_xi.Application;

public class Wallet
{
    [Key]
    public int IdWallet { get; set; }
    public required string Email { get ; set ; }
    public required float Balance { get ; set ; }


}