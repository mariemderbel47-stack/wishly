namespace Wishly.Application.DTOs;

public class CreateWishlistDto
{
    public Guid UserId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string? Occasion { get; set; }
    public string? ThemeVisuel { get; set; }
    public DateTime? DateOccasion { get; set; }
}

public class WishlistResponseDto
{
    public Guid Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string? Occasion { get; set; }
    public string? ThemeVisuel { get; set; }
    public DateTime? DateOccasion { get; set; }
    public string LienPublicUnique { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; }
}