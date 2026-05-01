using System.ComponentModel.DataAnnotations;

namespace Meta_xi.Application;
public class ReferLevel3
{
    [Key]
    public int IDReferLevel1 { get ; set ; }
    public required string UniqueCodeReferrer { get ; set ; }
    public required string UniqueCodeReFerred { get ; set ; }
    public required User? User { get ; set ; }
    public required int IDUserReferrer{ get ; set ; }
    
}