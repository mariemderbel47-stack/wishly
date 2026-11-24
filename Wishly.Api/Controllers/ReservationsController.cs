using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wishly.Application.DTOs;
using Wishly.Infrastructure.Services;

namespace Wishly.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly ReservationService _reservationService;
    private readonly EmailService _emailService;

    public ReservationsController(ReservationService reservationService, EmailService emailService)
    {
        _reservationService = reservationService;
        _emailService = emailService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateReservationDto dto)
    {
        dto.NomReservant = User.GetNom();
        dto.EmailReservant = User.GetEmail();

        var (succes, erreur, reservation) = await _reservationService.CreateAsync(dto);
        if (!succes)
        {
            return Conflict(erreur);
        }

        try
        {
            var nom = System.Net.WebUtility.HtmlEncode(dto.NomReservant);
            await _emailService.EnvoyerAsync(
                dto.EmailReservant,
                "Merci pour ta réservation sur Wishly 💚",
                $@"<p>Bonjour {nom},</p>
                   <p>Ton cadeau a bien été réservé. Merci pour ta générosité !</p>
                   <p>L'équipe Wishly</p>");
        }
        catch
        {
            // la réservation reste valide même si l'email échoue
        }

        return Ok(reservation);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var succes = await _reservationService.CancelAsync(id, User.GetEmail());
        if (!succes)
        {
            return NotFound("Réservation introuvable.");
        }
        return Ok();
    }
}