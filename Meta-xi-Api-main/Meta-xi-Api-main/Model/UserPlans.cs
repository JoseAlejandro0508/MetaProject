using System.ComponentModel.DataAnnotations;

namespace Meta_xi.Application;
public class UserPlans
{
    [Key]
    public int IDBuyPlan{ get; set; }
    public required string Username { get; set; }
    public required string NamePlan { get; set; }
    public required DateTime DatePlan { get; set; }
    public required double Percentage { get; set; }
}