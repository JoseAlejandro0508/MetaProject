using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Meta_xi.Application;

public class TaskService
{

    private readonly DBContext context;
    public TaskService(DBContext dBContext)
    {
        context = dBContext;
    }
    public bool isActiveUser(int UID)
    {

        var selectedUser=context.Users.FirstOrDefault(x => x.Id==UID);
        if (selectedUser==null){
            return false;
        }
        bool HasDeposit=context.Wallets.Any(x => x.Email==selectedUser!.PhoneNumber && x.TotalRecharged>0);
        return HasDeposit;
    }

    public async Task<int> GetCurrentFriends(int TaskId,int UID,List<TaskObject> TaskDB)
    {
        var allUser=await context.Users.ToListAsync();
        var selectedUser=allUser.FirstOrDefault(x => x.Id==UID);
        if (selectedUser==null){
            return 0;

        }
        var selectedTaskRegister=await context.TaskRegisters.FirstOrDefaultAsync(x => x.UID==UID && x.TaskId==TaskId);
        if (selectedTaskRegister==null){
            return 0;
        }
        var selectedTask=TaskDB.FirstOrDefault(x => x.TaskId==TaskId);

        int currentFriends=allUser.Where(x => isActiveUser(x.Id)&&x.ReferCode==selectedUser.Code && x.CreatedAt.Subtract(selectedTaskRegister.ActivateAt).TotalSeconds > 0 &&x.CreatedAt.Subtract(selectedTaskRegister.ActivateAt).TotalSeconds<selectedTask!.time*3600).Count();
        return currentFriends;
    }
}