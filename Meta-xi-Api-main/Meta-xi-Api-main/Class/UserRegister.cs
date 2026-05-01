namespace Meta_xi.Application;
public class UserRegister
{
    public required string? Email { get; set; }
    public required string Password { get; set; }
    public required string? PhoneNumber { get; set; }
    public required string? CodeReferrer { get ; set ; }

    public string GeneratedReferCode (){
        int length = 6;
        var random = new Random();
        return new string(Enumerable.Repeat("0123456789", length).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}