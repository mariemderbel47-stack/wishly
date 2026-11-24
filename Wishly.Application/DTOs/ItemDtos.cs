namespace Wishly.Application.DTOs;

public class CreateItemDto
{
    public Guid WishlistId { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string? LienExterne { get; set; }
    public string? PhotoUrl { get; set; }
    public decimal? Prix { get; set; }
}

public class ItemResponseDto
{
    public Guid Id { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string? LienExterne { get; set; }
    public string? PhotoUrl { get; set; }
    public decimal? Prix { get; set; }
    public bool EstReserve { get; set; }
}