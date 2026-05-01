
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Meta_xi.Application;
[ApiController]
[Route("api/[controller]")]
public class PlansController : ControllerBase{
    private readonly DBContext context;

    public PlansController(DBContext dBContext){
        context = dBContext;
    }
    //Endpoint para colocar algun plan nuevo
    [HttpPost("PlanRegister")]
    public async Task<IActionResult> PlanRegister(PlanRegister planRegister){
        Plan plan = new Plan{
            Name = planRegister.Name,
            Price = planRegister.Price,
            MaxQuantity = planRegister.MaxQuantity,
            DailyBenefit = planRegister.DailyBenefit,
            DaysActive = planRegister.DaysActive,
            TotalBenefit = planRegister.TotalBenefit
        };
        await context.Plans.AddAsync(plan);
        await context.SaveChangesAsync();
        return Ok("Plan registrado correctamente");
    }
    //Endpoint para obtener todos los planes disponibles en la aplicación
    [HttpGet("Plans/{username}")]
    public async Task<IActionResult> Plans(string username){
        var user = await context.Users.FirstOrDefaultAsync(option => option.Email == username || option.PhoneNumber == username);
        if(user == null){
            return NotFound("El usuario no existe en la aplicacion");
        }
        var userPlans = await context.UserPlans.Where(option => option.Username == username).ToListAsync();
        if(!userPlans.Any()){
            var plans = await context.Plans.ToListAsync();
            return Ok(plans);
        }
        var oldplans = await context.Plans.ToListAsync();
        foreach(var i in userPlans){
            foreach(var j in oldplans){
                if(i.NamePlan == j.Name){
                    j.MaxQuantity = j.MaxQuantity - 1;
                }
            }
        }
        return Ok(oldplans);
    }
}