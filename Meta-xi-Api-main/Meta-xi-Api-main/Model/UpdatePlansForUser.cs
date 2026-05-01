using System.ComponentModel.DataAnnotations;

namespace Meta_xi.Application;
public class UpdatePlansForUser{
    [Key]
    public int IDUpdatePlansForUser { get; set; }
    public required string Username { get; set; }
    public required double AcumulatedBenefitperHour { get; set; }
    public required double AcumulatedTotalBenefit { get; set; }
}