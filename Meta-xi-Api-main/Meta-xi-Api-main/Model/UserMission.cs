using System.ComponentModel.DataAnnotations;

namespace Meta_xi.Application;

public class UserMission
{
    [Key]
    public int Id { get; set; }
    public required string Email { get; set; }
    public required int MissionId { get; set; }
    public Mission? Mission { get; set; }
    public DateTimeOffset? ClaimedAt { get; set; }
}