using Microsoft.AspNetCore.Mvc;
using Wishly.Application.DTOs;
using Wishly.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;


namespace Wishly.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrganisationsController : ControllerBase
{
    private readonly OrganisationService _service;

    public OrganisationsController(OrganisationService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateOrganisationDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return Ok(result);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(Guid userId)
    {
        var result = await _service.GetByUserIdAsync(userId);
        return Ok(result);
    }

    [HttpPost("etapes")]
    public async Task<IActionResult> AddEtape(CreateEtapeDto dto)
    {
        var result = await _service.AddEtapeAsync(dto);
        if (result == null) return NotFound("Organisation introuvable.");
        return Ok(result);
    }

    [HttpPut("etapes/{id}/toggle")]
    public async Task<IActionResult> ToggleEtape(Guid id)
    {
        var succes = await _service.ToggleEtapeAsync(id);
        if (!succes) return NotFound();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var succes = await _service.DeleteOrganisationAsync(id);
        if (!succes) return NotFound();
        return Ok();
    }

    [HttpDelete("etapes/{id}")]
    public async Task<IActionResult> DeleteEtape(Guid id)
    {
        var succes = await _service.DeleteEtapeAsync(id);
        if (!succes) return NotFound();
        return Ok();
    }
}