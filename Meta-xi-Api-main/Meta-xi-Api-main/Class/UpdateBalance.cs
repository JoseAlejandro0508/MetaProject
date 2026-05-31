namespace Meta_xi.Application;
public class UpdateBalance
{
    public required string OrdenId { get; set; }
    public required string Email { get; set; }
    public required float Balance { get; set; }
    public required string Token { get; set; }
}