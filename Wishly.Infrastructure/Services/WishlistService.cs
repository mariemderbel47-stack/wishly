using Microsoft.EntityFrameworkCore;
using Wishly.Application.DTOs;
using Wishly.Domain.Entities;
using Wishly.Infrastructure.Persistence;

namespace Wishly.Infrastructure.Services;

public class WishlistService
{
    private readonly WishlyDbContext _context;

    public WishlistService(WishlyDbContext context)
    {
        _context = context;
    }

    public async Task<WishlistResponseDto?> CreateAsync(CreateWishlistDto dto)
    {
        bool userExiste = await _context.Users.AnyAsync(u => u.Id == dto.UserId);
        if (!userExiste)
        {
            return null;
        }

        var wishlist = new Wishlist
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            Nom = dto.Nom,
            Occasion = dto.Occasion,
            ThemeVisuel = dto.ThemeVisuel,
            LienPublicUnique = Guid.NewGuid().ToString("N"),
            DateCreation = DateTime.UtcNow
        };

        _context.Wishlists.Add(wishlist);
        await _context.SaveChangesAsync();

        return MapToDto(wishlist);
    }

    public async Task<WishlistResponseDto?> GetByLienPublicAsync(string lien)
    {
        var wishlist = await _context.Wishlists
            .FirstOrDefaultAsync(w => w.LienPublicUnique == lien);

        return wishlist == null ? null : MapToDto(wishlist);
    }

    private static WishlistResponseDto MapToDto(Wishlist w) => new()
    {
        Id = w.Id,
        Nom = w.Nom,
        Occasion = w.Occasion,
        ThemeVisuel = w.ThemeVisuel,
        LienPublicUnique = w.LienPublicUnique,
        DateCreation = w.DateCreation
    };
    public async Task<WishlistResponseDto?> GetByIdAsync(Guid id)
    {
        var wishlist = await _context.Wishlists.FindAsync(id);
        return wishlist == null ? null : MapToDto(wishlist);
    }
    public async Task<List<WishlistResponseDto>> GetByUserIdAsync(Guid userId)
    {
        var wishlists = await _context.Wishlists
            .Where(w => w.UserId == userId)
            .ToListAsync();

        return wishlists.Select(MapToDto).ToList();
    }
    public async Task<bool> DeleteAsync(Guid id)
    {
        var wishlist = await _context.Wishlists.FindAsync(id);
        if (wishlist == null) return false;

        _context.Wishlists.Remove(wishlist);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<bool> EstProprietaireAsync(Guid wishlistId, Guid userId)
    {
        return await _context.Wishlists.AnyAsync(w => w.Id == wishlistId && w.UserId == userId);
    }
}