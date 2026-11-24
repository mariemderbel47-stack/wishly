namespace Wishly.Domain.Entities;

public class InvitationCard
{
    public Guid Id { get; set; }
    public Guid WishlistId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? ThemeUtilise { get; set; }
    public DateTime DateGeneration { get; set; } = DateTime.UtcNow;

    public Wishlist? Wishlist { get; set; }
}