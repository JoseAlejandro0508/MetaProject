using Microsoft.AspNetCore.Mvc;

namespace Meta_xi.Application;
[ApiController]
[Route("api/[controller]")]
public class  ReferController : ControllerBase
{
    private readonly DBContext context;
    public ReferController(DBContext dbcontext){
        context = dbcontext;
    }
    [HttpPost("GetReferred/{id}")]
    public async Task<IActionResult> GetRefered(int id){
        var user = await context.Users.FindAsync(id);
        if(user == null){
            return NotFound("Usuario no encontrado");
        }
        var referslevel1 = context.ReferLevel1s.Where(option => option.IDUserReferrer == id).ToList();
        var refersLevel2 = context.ReferLevel2s.Where(option => option.IDUserReferrer == id).ToList();
        var refersLevel3 = context.ReferLevel3s.Where(option => option.IDUserReferrer == id).ToList();
        QuantityRefersLevelsBack quantityRefersLevelsBack = new QuantityRefersLevelsBack{
            QuantityRefersLevel1 = referslevel1.Count,
            QuantityRefersLevel2 = refersLevel2.Count,
            QuantityRefersLevel3 = refersLevel3.Count
        };
        return Ok(quantityRefersLevelsBack);
    }
}