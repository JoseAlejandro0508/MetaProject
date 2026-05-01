using Microsoft.AspNetCore.Identity;

namespace Meta_xi.Application;

public class UserService
{
    private readonly PasswordHasher<string> password;
    public UserService()
    {
        password = new PasswordHasher<string>();
    }
    public string GeneratePassword(string passwordtoscript)
    {
        return password.HashPassword("", passwordtoscript);
    }
    public bool verifyPassword(string passwordtoverify, string HashPassword)
    {
        var passwordVerificationResult = password.VerifyHashedPassword("", HashPassword, passwordtoverify);
        Console.WriteLine(passwordVerificationResult);
        return passwordVerificationResult == PasswordVerificationResult.Success;
    }

}