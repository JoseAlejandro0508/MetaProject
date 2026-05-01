using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Meta_xi.Application;
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly DBContext context;
    private readonly UserService userService;
    private readonly IGeneratedJwt generatedJwt;
    public readonly IRegisteredToReferLevel registeredToReferLevel;
    public UserController(DBContext dBContex, UserService service, IGeneratedJwt IgeneratedJwt, IRegisteredToReferLevel registeredToRefer)
    {
        context = dBContex;
        userService = service;
        generatedJwt = IgeneratedJwt;
        registeredToReferLevel = registeredToRefer;
    }
    //Endpoint para registrar un usuario
[HttpPost("UserRegister")]
public async Task<IActionResult> UserRegister(UserRegister userRegister)
{
    string code;
    bool isUnique;
    do
    {
        code = userRegister.GeneratedReferCode();
        isUnique = !await context.Users.AnyAsync(option => option.Code == code);
    } while (!isUnique);

    if (userRegister.Email != null)
    {
        var user = await context.Users.FirstOrDefaultAsync(option => option.Email == userRegister.Email);
        if (user != null)
        {
            return BadRequest(new { message = "Ese correo ya ha sido usado " });
        }

        string emailPattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-]+$";
        Match emailMatch = Regex.Match(userRegister.Email, emailPattern);
        if (!emailMatch.Success)
        {
            return NotFound(new { message = "Por favor entre un correo valido" });
        }

        if (userRegister.Password.Length < 6)
        {
            return BadRequest(new { message = "La contraseña debe tener al menos 6 caracteres" });
        }

        User userToRegister = new User
        {
            Email = userRegister.Email,
            Password = userService.GeneratePassword(userRegister.Password),
            PhoneNumber = null,
            Token = generatedJwt.GeneratedToken(userRegister.Email, userRegister.Password),
            Code = code,
            referLevel1s = null,
            referLevel2s = null,
            referLevel3s = null,
            Wallet = null
        };
        await context.Users.AddAsync(userToRegister);
        await context.SaveChangesAsync();

        var wallet = await context.Wallets.FirstOrDefaultAsync(option => option.Email == userRegister.Email);
        if (wallet != null)
        {
            return NotFound(new { message = "El usuario ya tiene una cartera" });
        }

        Wallet wallet1 = new Wallet
        {
            Email = userRegister.Email,
            Balance = 0,
        };
        await context.Wallets.AddAsync(wallet1);
        await context.SaveChangesAsync();
    }
    else
    {
        var phoneNumber = await context.Users.FirstOrDefaultAsync(option => option.PhoneNumber == userRegister.PhoneNumber);
        if (phoneNumber != null)
        {
            return BadRequest(new { message = "Ese número de telefono ya ha sido usado" });
        }

        PhoneNumberValidator phoneNumberValidator = new PhoneNumberValidator();
        if (userRegister.PhoneNumber != null)
        {
            if (phoneNumberValidator.IsValidPhoneNumber(userRegister.PhoneNumber))
            {
                var phoneIsRegistered = await context.Users.FirstOrDefaultAsync(option => option.PhoneNumber == userRegister.PhoneNumber);
                if (phoneIsRegistered != null)
                {
                    return NotFound(new { message = "Ese número de telefono ya ha sido usado" });
                }

                if (userRegister.Password.Length < 6)
                {
                    return BadRequest(new { message = "La contraseña debe tener al menos 6 caracteres" });
                }
            }

            User user = new User
            {
                Email = null,
                Password = userService.GeneratePassword(userRegister.Password),
                PhoneNumber = userRegister.PhoneNumber,
                Token = generatedJwt.GeneratedToken(userRegister.PhoneNumber, userRegister.Password),
                Code = code,
                referLevel1s = null,
                referLevel2s = null,
                referLevel3s = null,
                Wallet = null
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var wallet = await context.Wallets.FirstOrDefaultAsync(option => option.Email == userRegister.Email);
            if (wallet != null)
            {
                return NotFound(new { message = "El usuario ya tiene una cartera" });
            }

            Wallet wallet1 = new Wallet
            {
                Email = userRegister.PhoneNumber,
                Balance = 0
            };
            await context.Wallets.AddAsync(wallet1);
            await context.SaveChangesAsync();
        }
    }

    if (userRegister.CodeReferrer != null)
    {
        var father = await context.ReferLevel1s.FirstOrDefaultAsync(option => option.UniqueCodeReFerred == userRegister.CodeReferrer);
        if (father != null)
        {
            var grandfather = await context.ReferLevel1s.FirstOrDefaultAsync(option => option.UniqueCodeReFerred == father.UniqueCodeReferrer);
            if (grandfather != null)
            {
                await registeredToReferLevel.VerifyToReferLevel3(grandfather.UniqueCodeReferrer, code);
            }
            await registeredToReferLevel.VerifyToReferLevel2(father.UniqueCodeReferrer, code);
        }
        await registeredToReferLevel.VerifyToReferLevel1(userRegister.CodeReferrer, code);
    }
    
    return Ok(new { message = "Usuario registrado correctamente" });
}

    //Endpoint para loguear un usuario 
    [HttpPost("Login")]
    public async Task<IActionResult> Login(UserLogin userLogin)
    {
        if (userLogin.Email != null)
        {
            var user = await context.Users.FirstOrDefaultAsync(option => option.Email == userLogin.Email);
            if (user == null)
            {
                return NotFound("Usuario no encontrado");
            }
            Console.WriteLine(user.Password);
            if (!userService.verifyPassword(userLogin.Password, user.Password))
            {
                return BadRequest("Contraseña incorrecta");
            }
            user.Token = generatedJwt.GeneratedToken(userLogin.Email, userLogin.Password);
            context.Entry(user).State = EntityState.Modified;
            await context.SaveChangesAsync();
            return Ok(user.Token);
        }
        var username = await context.Users.FirstOrDefaultAsync(option => option.PhoneNumber == userLogin.PhoneNumber);
        if (username == null)
        {
            return NotFound("Usuario no encontrado");
        }
        if (!userService.verifyPassword(userLogin.Password, username.Password))
        {
            return BadRequest("Contraseña incorrecta");
        }
        if (userLogin.PhoneNumber != null)
        {
            username.Token = generatedJwt.GeneratedToken(userLogin.PhoneNumber, userLogin.Password);
        }
        context.Entry(username).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return Ok(username.Token);
    }
    //Obtener el link para el código de referido
    [HttpGet("GetLink/{username}")]
    public async Task<IActionResult> GetLink(string username){
        var user = await context.Users.FirstOrDefaultAsync(option => option.Email == username || option.PhoneNumber == username);
        if(user == null){
            return NotFound(new { message = "Usuario no encontrado" });
        }
        string link = $"http://localhost:4200/login?code={user.Code}";
        return Ok(new {link});
    }
    //Endpoint para actualizar contraseña
    [HttpPatch("UpdatePassword")]
    public async Task<IActionResult> UpdatePassword(UpdatePassword updatePassword)
    {
        var user = await context.Users.FirstOrDefaultAsync(option => option.Email == updatePassword.Username || option.PhoneNumber == updatePassword.Username);
        if (user == null)
        {
            return NotFound(new { message = "Usuario no encontrado" });
        }
        if (!userService.verifyPassword(updatePassword.OldPassword, user.Password))
        {
            return BadRequest(new { message = "Contraseña anterior incorrecta" });
        }
        string pattern2 = @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$";
        Match match1 = Regex.Match(updatePassword.NewPassword, pattern2);
        if (!match1.Success)
        {
            if (!Regex.IsMatch(updatePassword.NewPassword, "(?=.*[A-Z])"))
            {
                return BadRequest(new { message = "La contraseña debe tener al menos una mayúscula" });
            }
            else if (!Regex.IsMatch(updatePassword.NewPassword, "(?=.*[a-z])"))
            {
                return BadRequest(new { message = "La contraseña debe tener al menos una minúscula" });
            }
            else if (!Regex.IsMatch(updatePassword.NewPassword, "(?=.*\\d)"))
            {
                return BadRequest(new { message = "La contraseña debe tener al menos un número" });
            }
            else if (!Regex.IsMatch(updatePassword.NewPassword, "(?=.*[^\\da-zA-Z])"))
            {
                return BadRequest(new { message = "La contraseña debe tener al menos un carácter especial" });
            }
            else
            {
                return BadRequest(new { message = "La contraseña debe tener al menos 6 caracteres" });
            }
        }
        user.Password = userService.GeneratePassword(updatePassword.NewPassword);
        context.Entry(user).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return Ok(new { message = "Contraseña actualizada correctamente" });
    }
}
