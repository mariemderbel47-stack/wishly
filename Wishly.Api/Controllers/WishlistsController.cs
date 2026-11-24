using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wishly.Application.DTOs;
using Wishly.Infrastructure.Services;

namespace Wishly.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class WishlistsController : ControllerBase
{
    private readonly WishlistService _wishlistService;

    public WishlistsController(WishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateWishlistDto dto)
    {
        dto.UserId = User.GetUserId(); // on ignore l'id envoyé par le client
        var result = await _wishlistService.CreateAsync(dto);
        if (result == null)
        {
            return BadRequest("Utilisateur introuvable.");
        }
        return Ok(result);
    }

    // Lien partagé : toute personne connectée (email confirmé) peut l'ouvrir
    [HttpGet("{lien}")]
    public async Task<IActionResult> GetByLien(string lien)
    {
        var result = await _wishlistService.GetByLienPublicAsync(lien);
        if (result == null)
        {
            return NotFound("Wishlist introuvable.");
        }
        return Ok(result);
    }

    [HttpGet("id/{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        if (!await _wishlistService.EstProprietaireAsync(id, User.GetUserId()))
        {
            return NotFound("Wishlist introuvable.");
        }
        var result = await _wishlistService.GetByIdAsync(id);
        if (result == null)
        {
            return NotFound("Wishlist introuvable.");
        }
        return Ok(result);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(Guid userId)
    {
        if (userId != User.GetUserId()) return Forbid();
        var result = await _wishlistService.GetByUserIdAsync(userId);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!await _wishlistService.EstProprietaireAsync(id, User.GetUserId()))
        {
            return NotFound("Wishlist introuvable.");
        }
        var succes = await _wishlistService.DeleteAsync(id);
        if (!succes) return NotFound("Wishlist introuvable.");
        return Ok();
    }
}