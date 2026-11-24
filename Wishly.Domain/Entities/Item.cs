namespace Wishly.Domain.Entities;

public class Item
{
    public Guid Id { get; set; }
    public Guid WishlistId { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string? LienExterne { get; set; }
    public string? PhotoUrl { get; set; }
    public decimal? Prix { get; set; }
    public DateTime DateAjout { get; set; } = DateTime.UtcNow;

    public Wishlist? Wishlist { get; set; }
    public Reservation? Reservation { get; set; }
}