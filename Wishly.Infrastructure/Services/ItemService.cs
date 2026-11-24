using Microsoft.EntityFrameworkCore;
using Wishly.Application.DTOs;
using Wishly.Domain.Entities;
using Wishly.Infrastructure.Persistence;

namespace Wishly.Infrastructure.Services;

public class ItemService
{
    private readonly WishlyDbContext _context;

    public ItemService(WishlyDbContext context)
    {
        _context = context;
    }

    public async Task<ItemResponseDto?> CreateAsync(CreateItemDto dto)
    {
        bool wishlistExiste = await _context.Wishlists.AnyAsync(w => w.Id == dto.WishlistId);
        if (!wishlistExiste)
        {
            return null;
        }

        var item = new Item
        {
            Id = Guid.NewGuid(),
            WishlistId = dto.WishlistId,
            Titre = dto.Titre,
            LienExterne = dto.LienExterne,
            PhotoUrl = dto.PhotoUrl,
            Prix = dto.Prix,
            DateAjout = DateTime.UtcNow
        };

        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        return MapToDto(item, estReserve: false);
    }

    public async Task<List<ItemResponseDto>> GetByWishlistIdAsync(Guid wishlistId)
    {
        var items = await _context.Items
            .Where(i => i.WishlistId == wishlistId)
            .Include(i => i.Reservation)
            .ToListAsync();

        return items.Select(i => MapToDto(i, estReserve: i.Reservation != null)).ToList();
    }

    private static ItemResponseDto MapToDto(Item i, bool estReserve) => new()
    {
        Id = i.Id,
        Titre = i.Titre,
        LienExterne = i.LienExterne,
        PhotoUrl = i.PhotoUrl,
        Prix = i.Prix,
        EstReserve = estReserve
    };
    public async Task<bool> DeleteAsync(Guid id)
    {
        var item = await _context.Items.FindAsync(id);
        if (item == null) return false;

        _context.Items.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<bool> EstProprietaireAsync(Guid itemId, Guid userId)
    {
        return await _context.Items.AnyAsync(i => i.Id == itemId &&
            _context.Wishlists.Any(w => w.Id == i.WishlistId && w.UserId == userId));
    }
}