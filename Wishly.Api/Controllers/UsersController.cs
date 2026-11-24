using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Wishly.Application.DTOs;
using Wishly.Infrastructure.Services;

namespace Wishly.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    [EnableRateLimiting("auth")]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserDto dto)
    {
        var result = await _userService.RegisterAsync(dto);
        if (result == null) return BadRequest("Cet email est déjà utilisé.");
        return Ok(result);
    }

    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserDto dto)
    {
        var result = await _userService.LoginAsync(dto);
        if (result == null) return Unauthorized("Email ou mot de passe incorrect.");
        return Ok(result);
    }

    [EnableRateLimiting("auth")]
    [HttpPost("google")]
    public async Task<IActionResult> LoginGoogle(GoogleAuthDto dto)
    {
        var (succes, erreur) = await _userService.LoginWithGoogleAsync(dto.IdToken, dto.Retour);
        if (!succes) return BadRequest(erreur);
        return Ok(new { message = "Un email de confirmation a été envoyé." });
    }

    [EnableRateLimiting("auth")]
    [HttpPost("email")]
    public async Task<IActionResult> LoginEmail(EmailAuthDto dto)
    {
        var (succes, erreur) = await _userService.ConnexionParEmailAsync(dto.Email, dto.Retour);
        if (!succes) return BadRequest(erreur);
        return Ok(new { message = "Un email de confirmation a été envoyé." });
    }

    [EnableRateLimiting("auth")]
    [HttpPost("confirmer-connexion")]
    public async Task<IActionResult> ConfirmerConnexion(ConfirmerConnexionDto dto)
    {
        var result = await _userService.ConfirmerConnexionAsync(dto.Token);
        if (result == null) return BadRequest("Lien invalide ou expiré.");
        return Ok(result);
    }

    [Authorize]
    [HttpPut("{id}/photo")]
    public async Task<IActionResult> UpdatePhoto(Guid id, [FromBody] PhotoDto dto)
    {
        if (User.GetUserId() != id) return Forbid();
        var result = await _userService.UpdatePhotoAsync(id, dto.PhotoUrl);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [Authorize]
    [HttpPut("{id}/profil")]
    public async Task<IActionResult> UpdateProfil(Guid id, UpdateProfilDto dto)
    {
        if (User.GetUserId() != id) return Forbid();
        var (succes, erreur, user) = await _userService.UpdateProfilAsync(id, dto);
        if (!succes) return BadRequest(erreur);
        return Ok(user);
    }

    [Authorize]
    [HttpPut("{id}/mot-de-passe")]
    public async Task<IActionResult> ChangerMotDePasse(Guid id, ChangerMotDePasseDto dto)
    {
        if (User.GetUserId() != id) return Forbid();
        var (succes, erreur) = await _userService.ChangerMotDePasseAsync(id, dto);
        if (!succes) return BadRequest(erreur);
        return Ok();
    }
}