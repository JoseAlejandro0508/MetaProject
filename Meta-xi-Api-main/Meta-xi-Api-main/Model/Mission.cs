using System.ComponentModel.DataAnnotations;

namespace Meta_xi.Application;

public class Mission
{
    [Key]
    public int Id { get; set; }
    public required string Title { get; set; }
    public required MissionType Type { get; set; }
    public required int Ref { get; set; }
    public required float Gift { get; set; }
    public string? ImageUrl { get; set; }
}

public enum MissionType
{
    Normal = 0,
    Premium = 1
}