using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wishly.Application.DTOs;
using Wishly.Infrastructure.Services;

namespace Wishly.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly ItemService _itemService;
    private readonly WishlistService _wishlistService;
    private readonly LinkPreviewService _linkPreviewService;

    public ItemsController(ItemService itemService, WishlistService wishlistService, LinkPreviewService linkPreviewService)
    {
        _itemService = itemService;
        _wishlistService = wishlistService;
        _linkPreviewService = linkPreviewService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateItemDto dto)
    {
        if (!await _wishlistService.EstProprietaireAsync(dto.WishlistId, User.GetUserId()))
        {
            return BadRequest("Wishlist introuvable.");
        }
        var result = await _itemService.CreateAsync(dto);
        if (result == null)
        {
            return BadRequest("Wishlist introuvable.");
        }
        return Ok(result);
    }

    [HttpGet("wishlist/{wishlistId}")]
    public async Task<IActionResult> GetByWishlist(Guid wishlistId)
    {
        var items = await _itemService.GetByWishlistIdAsync(wishlistId);
        return Ok(items);
    }

    [HttpGet("preview")]
    public async Task<IActionResult> GetPreview([FromQuery] string url)
    {
        if (string.IsNullOrWhiteSpace(url)
            || !Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return BadRequest("URL invalide.");
        }

        var preview = await _linkPreviewService.GetPreviewAsync(uri.ToString());
        return Ok(preview);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!await _itemService.EstProprietaireAsync(id, User.GetUserId()))
        {
            return NotFound("Cadeau introuvable.");
        }
        var succes = await _itemService.DeleteAsync(id);
        if (!succes) return NotFound("Cadeau introuvable.");
        return Ok();
    }
}