using Meta.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Meta_xi.Application;
[ApiController]
[Route("api/[controller]")]
public class WalletController : ControllerBase
{
    private readonly DBContext context;
    public WalletController(DBContext _context)
    {
        context = _context;
    }
    [HttpPost("UpdateBalance")]
    public async Task<IActionResult> UpdateBalance(UpdateBalance updateBalance){
        GetMoneyValues getMoneyValues = new GetMoneyValues();
        var wallet = await context.Wallets.FirstOrDefaultAsync(option => option.Email == updateBalance.Email);
        if(wallet == null){
            return NotFound(new { message = "No existe ninguna cartera con ese correo" });
        }
        string token = updateBalance.Token.ToLower();
        switch (token){
            case "nequi":
                wallet.Balance = updateBalance.Balance;
                context.Entry(wallet).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return Ok(new { message = "Balance actualizada correctamente" });
            case "trx":
                decimal balance = await getMoneyValues.GetMoneyValueAsync("trx");
                float value = (float)balance;
                Console.WriteLine(value);
                decimal usdToCop = await getMoneyValues.GetMoneyValueAsync("cop");
                float usd = (float)usdToCop;
                Console.WriteLine(usd);
                wallet.Balance = wallet.Balance + (value * updateBalance.Balance * usd);
                context.Entry(wallet).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return Ok(new { message = "Balance actualizado correctamente" });    
                case "usdt_trc20":
                    decimal balance2 = await getMoneyValues.GetMoneyValueAsync("tether");
                    float value2 = (float)balance2;
                    decimal usdToCop2 = await getMoneyValues.GetMoneyValueAsync("cop");
                    float usd2 = (float)usdToCop2;
                    wallet.Balance = wallet.Balance + (value2 * updateBalance.Balance * usd2);
                    context.Entry(wallet).State = EntityState.Modified;
                    await context.SaveChangesAsync();
                    return Ok(new { message = "Balance actualizado correctamente" });
                case "paypal":
                    decimal balance3 = await getMoneyValues.GetMoneyValueAsync("cop");
                    float value3 = (float)balance3;
                    wallet.Balance = wallet.Balance + (value3 * updateBalance.Balance);
                    context.Entry(wallet).State = EntityState.Modified;
                    await context.SaveChangesAsync();
                    return Ok(new { message = "Balance actualizado correctamente" });
                    default:
                        return NotFound(new { message = "Token no soportado" });
        } 
    }
    [HttpGet("GetBalance/{username}")]
    public async Task<IActionResult> GetBalance(string username){
        var wallet = await context.Wallets.FirstOrDefaultAsync(option => option.Email == username);
        if(wallet == null){
            return NotFound(new { message = "El usuario no posee ninguna cartera"});
        }
        return Ok(wallet.Balance);
    }
}