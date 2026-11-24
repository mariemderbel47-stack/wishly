using Microsoft.AspNetCore.Mvc;

namespace Wishly.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadsController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public UploadsController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpPost("image")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Aucun fichier reçu.");
        }

        var extensionsAutorisees = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!extensionsAutorisees.Contains(extension))
        {
            return BadRequest("Format de fichier non supporté.");
        }

        var racineWeb = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var dossierUploads = Path.Combine(racineWeb, "uploads");
        Directory.CreateDirectory(dossierUploads);

        var nomFichier = $"{Guid.NewGuid()}{extension}";
        var cheminComplet = Path.Combine(dossierUploads, nomFichier);

        using (var stream = new FileStream(cheminComplet, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var url = $"{Request.Scheme}://{Request.Host}/uploads/{nomFichier}";
        return Ok(new { url });
    }
}