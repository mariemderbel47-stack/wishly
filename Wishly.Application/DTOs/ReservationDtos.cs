namespace Wishly.Application.DTOs;

public class CreateReservationDto
{
    public Guid ItemId { get; set; }
    public string NomReservant { get; set; } = string.Empty;
    public string? EmailReservant { get; set; }
}

public class ReservationResponseDto
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public string NomReservant { get; set; } = string.Empty;
    public DateTime DateReservation { get; set; }
}