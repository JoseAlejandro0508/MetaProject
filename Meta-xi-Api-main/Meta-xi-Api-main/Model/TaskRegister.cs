using System.ComponentModel.DataAnnotations;

namespace Meta_xi.Application;

public class TaskRegister
{
    [Key]
    public int Id { get; set; }
    public required int TaskId { get ; set ; }
    public required int UID { get ; set ; }
    
    public required bool Completed{ get ; set ; }=false ;
    public required DateTimeOffset ActivateAt { get; set; }
    public TaskRegister()
    {

        ActivateAt = DateTimeOffset.Now;
    }


}