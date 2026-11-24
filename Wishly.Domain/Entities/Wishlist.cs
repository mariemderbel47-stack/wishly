namespace Wishly.Domain.Entities;

public class Wishlist
{
    public DateTime? DateOccasion { get; set; }
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string? Occasion { get; set; }
    public string? ThemeVisuel { get; set; }
    public string LienPublicUnique { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public ICollection<Item> Items { get; set; } = new List<Item>();
    public InvitationCard? InvitationCard { get; set; }
}