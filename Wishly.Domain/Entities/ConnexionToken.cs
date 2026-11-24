namespace Wishly.Domain.Entities;

public class ConnexionToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime DateExpiration { get; set; }
    public bool EstUtilise { get; set; } = false;

    public User? User { get; set; }
}