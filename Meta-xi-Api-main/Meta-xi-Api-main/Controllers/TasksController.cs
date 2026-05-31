using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Meta_xi.Application;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{



    private readonly DBContext context;
    private readonly TaskService _taskService;
    private readonly ReferService _refservice;

    private readonly List<TaskObject> tasks = new List<TaskObject>
    {
        new TaskObject{ TaskId = 1, friends = 1, time = 2, prize = 5000 },
        new TaskObject{ TaskId = 2, friends = 10, time = 12, prize = 100000 },
        new TaskObject{ TaskId = 3, friends = 5, time = 10, prize = 50000 },
        new TaskObject{ TaskId = 4, friends = 8, time = 24, prize = 90000 },
        new TaskObject{ TaskId = 5, friends = 15, time = 24, prize = 200000 },
        new TaskObject{ TaskId = 6, friends = 20, time = 48, prize = 300000 }
    };
    public TasksController(DBContext dBContext, TaskService taskService,ReferService referService)
    {
        _taskService = taskService;
        context = dBContext;
        _refservice = referService;
    }
    public async Task<int> getCurrentFriends(int idUser, int taskId)
    {
        var registerOfTask = context.TaskRegisters.FirstOrDefault(option => option.UID == idUser && option.TaskId == taskId);
        if (registerOfTask == null)
        {
            return 0;

        }
        var SelectedUser=await context.Users.FindAsync(idUser);
        if (SelectedUser == null)return 0;
        
        var currentFriends = context.Users.Where(u =>u.ReferCode==SelectedUser.Code&&u.CreatedAt > registerOfTask.ActivateAt).Count();
        return currentFriends;
    }

    public class RequestGetTasks
    {
        public string Username { get; set; }
    }
    public class ResponseGetTasks
    {
        public int id { get; set; }
        public int friends{get; set; }
        public int time{get; set; }
        public double prize{get; set; }
        public bool completed{get; set; }
        public int currentRefs{get; set; }
        public int RestTime { get; set; }
    }
    [HttpPost("GetTasks")]
    public async Task<IActionResult> GetTasks([FromBody] RequestGetTasks request)
    {

        var user = await context.Users.FirstOrDefaultAsync(option => option.PhoneNumber == request.Username);
        if (user == null)
        {
            return NotFound(new { message = "Usuario no encontrado" });
        }
        var RegisterOfUser = await context.TaskRegisters.Where(option => option.UID == user.Id).ToListAsync();
        List<ResponseGetTasks> Response_ = new List<ResponseGetTasks>();
        foreach (var task_ in tasks)
        {
            int currentsFriends = await _taskService.GetCurrentFriends(task_.TaskId, user.Id, tasks);
            var selectedTaskRegister = RegisterOfUser.FirstOrDefault(tr => tr.TaskId == task_.TaskId);
            int RestTime = 0;
            if (selectedTaskRegister != null)
            {
                RestTime = task_.time*3600- (int)(DateTime.Now - selectedTaskRegister.ActivateAt).TotalSeconds;
                RestTime = Math.Max(0, RestTime);
  
                
            }
            bool completed = RegisterOfUser.FirstOrDefault(tr => tr.TaskId == task_.TaskId) != null && currentsFriends >= task_.friends;
            if(completed != selectedTaskRegister?.Completed){
                if(selectedTaskRegister != null){
                    selectedTaskRegister.Completed = completed;
                    context.TaskRegisters.Update(selectedTaskRegister);
                    var wallet = await context.Wallets.FirstOrDefaultAsync(option => option.Email == user.Email);
                    if(wallet != null && completed){
                        wallet.Balance += (float)task_.prize;
                        context.Wallets.Update(wallet);
                    }
                    await context.SaveChangesAsync();
                }
            }
            ResponseGetTasks RTObject = new ResponseGetTasks { id = task_.TaskId, friends = task_.friends, time = task_.time, prize = task_.prize, completed = completed, currentRefs = currentsFriends, RestTime = completed?0:RestTime };
            Response_.Add(RTObject);
        }

        return Ok(Response_);
    }
    public class RequestActivateTask
    {
        public int taskId { get; set; }
        public string Username { get; set; }
    }

    [HttpPost("ActivateTask")]
    public async Task<IActionResult> ActivateTask([FromBody] RequestActivateTask request)
    {

        var user = await context.Users.FirstOrDefaultAsync(option => option.PhoneNumber == request.Username);
        if (user == null)
        {
            return NotFound(new { message = "Usuario no encontrado" });
        }
        var RegisterOfUser = await context.TaskRegisters.FirstOrDefaultAsync(option => option.UID == user.Id && option.TaskId == request.taskId);
        if (RegisterOfUser == null)
        {
            TaskRegister newRegister = new TaskRegister { UID = user.Id, TaskId = request.taskId, ActivateAt = DateTime.Now ,Completed=false};
            await context.TaskRegisters.AddAsync(newRegister);
            await context.SaveChangesAsync();
        }
        else
        {
            int currentsFriends = await _taskService.GetCurrentFriends(request.taskId, user.Id, tasks);
            if (RegisterOfUser.Completed)
            {
                return BadRequest(new { message = "Tarea ya completada" });
            }else if(DateTimeOffset.Now.Subtract(RegisterOfUser.ActivateAt).TotalSeconds <= tasks.FirstOrDefault(t => t.TaskId == request.taskId)!.time * 3600)
            {
                return BadRequest(new { message = "Tarea ya activa" });
            }

            else if(currentsFriends>= tasks.FirstOrDefault(t => t.TaskId == request.taskId)!.friends)
            {
                return BadRequest(new { message = "Tarea ya completada" });
                
            }else
            {
                RegisterOfUser.ActivateAt = DateTime.Now;
                context.TaskRegisters.Update(RegisterOfUser);
                await context.SaveChangesAsync();
            }
        }

        return Ok(new {message= "Tarea activada correctamente" });
    }

}