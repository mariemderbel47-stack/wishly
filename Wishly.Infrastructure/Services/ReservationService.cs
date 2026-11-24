using Microsoft.EntityFrameworkCore;
using Wishly.Application.DTOs;
using Wishly.Domain.Entities;
using Wishly.Infrastructure.Persistence;

namespace Wishly.Infrastructure.Services;

public class ReservationService
{
    private readonly WishlyDbContext _context;

    public ReservationService(WishlyDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Succes, string? Erreur, ReservationResponseDto? Reservation)> CreateAsync(CreateReservationDto dto)
    {
        var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == dto.ItemId);
        if (item == null)
        {
            return (false, "Cadeau introuvable.", null);
        }

        if (string.IsNullOrWhiteSpace(dto.EmailReservant))
        {
            return (false, "L'email est requis pour réserver un cadeau.", null);
        }

        var email = dto.EmailReservant.Trim().ToLower();

        // Le propriétaire de la liste ne peut pas réserver ses propres cadeaux
        bool estProprietaire = await _context.Users.AnyAsync(u =>
            u.Email.ToLower() == email &&
            _context.Wishlists.Any(w => w.Id == item.WishlistId && w.UserId == u.Id));
        if (estProprietaire)
        {
            return (false, "Tu ne peux pas réserver un cadeau de ta propre liste.", null);
        }

        bool aDejaReserve = await _context.Reservations
            .Include(r => r.Item)
            .AnyAsync(r => r.Item!.WishlistId == item.WishlistId
                        && r.EmailReservant != null
                        && r.EmailReservant.ToLower() == email);
        if (aDejaReserve)
        {
            return (false, "Tu as déjà réservé un cadeau dans cette wishlist.", null);
        }

        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            ItemId = dto.ItemId,
            NomReservant = dto.NomReservant,
            EmailReservant = email,
            DateReservation = DateTime.UtcNow
        };

        _context.Reservations.Add(reservation);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return (false, "Ce cadeau vient d'être réservé par quelqu'un d'autre.", null);
        }

        return (true, null, new ReservationResponseDto
        {
            Id = reservation.Id,
            ItemId = reservation.ItemId,
            NomReservant = reservation.NomReservant,
            DateReservation = reservation.DateReservation
        });
    }

    // Seule la personne qui a réservé peut annuler
    public async Task<bool> CancelAsync(Guid reservationId, string emailDemandeur)
    {
        var reservation = await _context.Reservations.FindAsync(reservationId);
        if (reservation == null
            || reservation.EmailReservant == null
            || !string.Equals(reservation.EmailReservant, emailDemandeur, StringComparison.OrdinalIgnoreCase))
            return false;

        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync();
        return true;
    }
}