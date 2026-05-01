using System.ComponentModel.DataAnnotations;

namespace Meta_xi.Application;
public class Plan
{
    [Key]
    public int IDPlan { get ; set ; }
    public required string Name { get; set; }
    public required double Price { get; set; }
    public required int MaxQuantity { get; set; }
    public required int DaysActive { get; set; }
    public required double DailyBenefit { get; set; }
    public required double TotalBenefit { get ; set ; }
    
}