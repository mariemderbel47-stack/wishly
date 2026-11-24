namespace Wishly.Domain.Entities;

public class User
{
    public string? PhotoUrl { get; set; }
    public Guid Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MotDePasseHash { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    public ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}