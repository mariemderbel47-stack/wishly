namespace Wishly.Domain.Entities;

public class Reservation
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public string NomReservant { get; set; } = string.Empty;
    public string? EmailReservant { get; set; }
    public DateTime DateReservation { get; set; } = DateTime.UtcNow;

    public Item? Item { get; set; }
}