
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
            TotalBenefit = planRegister.TotalBenefit,
            Description = planRegister.Description,
            DailyProfitPercentage = planRegister.DailyProfitPercentage,
            ImageUrl = planRegister.ImageUrl
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
    //Endpoint para obtener todos los planes sin filtro de usuario
    [HttpGet]
    public async Task<IActionResult> GetAllPlans()
    {
        var plans = await context.Plans.ToListAsync();
        return Ok(plans);
    }
    //Endpoint para actualizar un plan existente
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePlan(int id, [FromBody] PlanRegister planRegister)
    {
        var plan = await context.Plans.FirstOrDefaultAsync(p => p.IDPlan == id);
        if (plan == null)
        {
            return NotFound(new { message = "Plan no encontrado" });
        }
        plan.Name = planRegister.Name;
        plan.Price = planRegister.Price;
        plan.MaxQuantity = planRegister.MaxQuantity;
        plan.DaysActive = planRegister.DaysActive;
        plan.DailyBenefit = planRegister.DailyBenefit;
        plan.TotalBenefit = planRegister.TotalBenefit;
        plan.Description = planRegister.Description;
        plan.DailyProfitPercentage = planRegister.DailyProfitPercentage;
        plan.ImageUrl = planRegister.ImageUrl;
        context.Entry(plan).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return Ok(plan);
    }
    //Endpoint para eliminar un plan
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlan(int id)
    {
        var plan = await context.Plans.FirstOrDefaultAsync(p => p.IDPlan == id);
        if (plan == null)
        {
            return NotFound(new { message = "Plan no encontrado" });
        }
        context.Plans.Remove(plan);
        await context.SaveChangesAsync();
        return Ok(new { message = "Plan eliminado correctamente" });
    }
}