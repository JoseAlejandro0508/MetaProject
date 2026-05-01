using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Meta_xi.Application;
public class GeneratedJwt : IGeneratedJwt
{
    private readonly IConfiguration configuration;
    public GeneratedJwt(IConfiguration configuration2)
    {
        configuration = configuration2;
    }
    public string GeneratedToken(string Email, string Password)
    {
        var claims = new[] {
            new Claim("email" ,Email),
            new Claim("password" , Password)
        };
        var Key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("super secret key for new project with the 512 bytes patreon , thanks for user my app")
        );
        var creds = new SigningCredentials(Key, SecurityAlgorithms.HmacSha512Signature);
        var Securitytoken = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(1),
            signingCredentials: creds

        );
        string token = new JwtSecurityTokenHandler().WriteToken(Securitytoken);
        return token;
    }
    public bool VerifyToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("super secret key for new project with the 512 bytes patreon , thanks for user my app")),
            ValidateIssuer = false,
            ValidateAudience = false
        };

        SecurityToken validatedToken;
        var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);


        if (validatedToken.ValidTo < DateTime.UtcNow)
        {
            return true;
        }

        return false;
    }
}