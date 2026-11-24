using Microsoft.EntityFrameworkCore;
using Wishly.Application.DTOs;
using Wishly.Domain.Entities;
using Wishly.Infrastructure.Persistence;

namespace Wishly.Infrastructure.Services;

public class OrganisationService
{
    private readonly WishlyDbContext _context;

    public OrganisationService(WishlyDbContext context)
    {
        _context = context;
    }

    public async Task<OrganisationResponseDto> CreateAsync(CreateOrganisationDto dto)
    {
        var organisation = new Organisation
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            NomOccasion = dto.NomOccasion,
            DateOccasion = dto.DateOccasion,
            DateCreation = DateTime.UtcNow
        };

        _context.Organisations.Add(organisation);
        await _context.SaveChangesAsync();

        return MapToDto(organisation);
    }

    public async Task<List<OrganisationResponseDto>> GetByUserIdAsync(Guid userId)
    {
        var organisations = await _context.Organisations
            .Where(o => o.UserId == userId)
            .Include(o => o.Etapes)
            .ToListAsync();

        return organisations.Select(MapToDto).ToList();
    }

    public async Task<EtapeResponseDto?> AddEtapeAsync(CreateEtapeDto dto)
    {
        bool orgExiste = await _context.Organisations.AnyAsync(o => o.Id == dto.OrganisationId);
        if (!orgExiste) return null;

        var etape = new EtapeOrganisation
        {
            Id = Guid.NewGuid(),
            OrganisationId = dto.OrganisationId,
            Texte = dto.Texte,
            EstTerminee = false
        };

        _context.EtapesOrganisation.Add(etape);
        await _context.SaveChangesAsync();

        return new EtapeResponseDto { Id = etape.Id, Texte = etape.Texte, EstTerminee = etape.EstTerminee };
    }

    public async Task<bool> ToggleEtapeAsync(Guid etapeId)
    {
        var etape = await _context.EtapesOrganisation.FindAsync(etapeId);
        if (etape == null) return false;

        etape.EstTerminee = !etape.EstTerminee;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteOrganisationAsync(Guid id)
    {
        var organisation = await _context.Organisations.FindAsync(id);
        if (organisation == null) return false;

        _context.Organisations.Remove(organisation);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteEtapeAsync(Guid id)
    {
        var etape = await _context.EtapesOrganisation.FindAsync(id);
        if (etape == null) return false;

        _context.EtapesOrganisation.Remove(etape);
        await _context.SaveChangesAsync();
        return true;
    }

    private static OrganisationResponseDto MapToDto(Organisation o) => new()
    {
        Id = o.Id,
        NomOccasion = o.NomOccasion,
        DateOccasion = o.DateOccasion,
        Etapes = o.Etapes.Select(e => new EtapeResponseDto { Id = e.Id, Texte = e.Texte, EstTerminee = e.EstTerminee }).ToList()
    };
}