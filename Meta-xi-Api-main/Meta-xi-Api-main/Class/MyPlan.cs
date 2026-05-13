namespace Meta_xi.Application;
public class MyPlan
{
    public required string Name { get ; set ; }
    public required double Percentage { get ; set ; }
    public required int DaysRemaining { get ; set ; }
    public required double HourBenefit { get ; set ; }
    public required double DailyBenefit { get ; set ; }
    public required double TotalBenefit { get ; set ; }
    public required int IdPlan { get ; set ; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public double? DailyProfitPercentage { get; set; }
}