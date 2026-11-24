using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Wishly.Application.DTOs;
using Wishly.Domain.Entities;
using Wishly.Infrastructure.Persistence;
using System.Security.Cryptography;

namespace Wishly.Infrastructure.Services;

public class UserService
{
    private readonly WishlyDbContext _context;
    private readonly EmailService _emailService;
    private readonly IConfiguration _config;
    private readonly TokenService _tokenService;

    public UserService(WishlyDbContext context, EmailService emailService, IConfiguration config, TokenService tokenService)
    {
        _context = context;
        _emailService = emailService;
        _config = config;
        _tokenService = tokenService;
    }

    private UserResponseDto ToDto(User user) => new UserResponseDto
    {
        Id = user.Id,
        Nom = user.Nom,
        Email = user.Email,
        PhotoUrl = user.PhotoUrl,
        Token = _tokenService.Creer(user)
    };

    public async Task<UserResponseDto?> RegisterAsync(RegisterUserDto dto)
    {
        var email = (dto.Email ?? "").Trim().ToLowerInvariant();
        bool emailExiste = await _context.Users.AnyAsync(u => u.Email == email);
        if (emailExiste) return null;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Nom = dto.Nom,
            Email = email,
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(dto.MotDePasse),
            DateCreation = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return ToDto(user);
    }

    public async Task<UserResponseDto?> LoginAsync(LoginUserDto dto)
    {
        var email = (dto.Email ?? "").Trim().ToLowerInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return null;
        if (!BCrypt.Net.BCrypt.Verify(dto.MotDePasse, user.MotDePasseHash)) return null;

        return ToDto(user);
    }

    private static string RetourSur(string? retour)
    {
        if (string.IsNullOrWhiteSpace(retour) || !retour.StartsWith("/")
            || retour.StartsWith("//") || retour.Contains('\\'))
        {
            return "/mon-compte";
        }
        return retour;
    }

    // Connexion Google : on valide le jeton Google, puis on envoie l'email "Oui, c'est moi"
    public async Task<(bool Succes, string? Erreur)> LoginWithGoogleAsync(string idToken, string? retour)
    {
        Google.Apis.Auth.GoogleJsonWebSignature.Payload payload;
        try
        {
            var settings = new Google.Apis.Auth.GoogleJsonWebSignature.ValidationSettings();
            var clientId = _config["Google:ClientId"];
            if (!string.IsNullOrWhiteSpace(clientId))
            {
                settings.Audience = new[] { clientId };
            }
            payload = await Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(idToken, settings);
        }
        catch
        {
            return (false, "Connexion Google invalide.");
        }

        if (!payload.EmailVerified)
        {
            return (false, "L'email de ce compte Google n'est pas vérifié.");
        }

        var emailGoogle = payload.Email.Trim().ToLowerInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailGoogle);
        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Nom = payload.Name ?? emailGoogle,
                Email = emailGoogle,
                MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                DateCreation = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        return await EnvoyerLienConnexionAsync(user, retour, "via Google");
    }

    // Connexion par simple adresse email (pour les invités sans compte Google)
    public async Task<(bool Succes, string? Erreur)> ConnexionParEmailAsync(string? email, string? retour)
    {
        email = (email ?? "").Trim().ToLowerInvariant();
        try
        {
            var adresse = new System.Net.Mail.MailAddress(email);
            if (adresse.Address != email || email.Length > 254) throw new FormatException();
        }
        catch
        {
            return (false, "Adresse email invalide.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Nom = email.Split('@')[0],
                Email = email,
                MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                DateCreation = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        return await EnvoyerLienConnexionAsync(user, retour, "avec ton adresse email");
    }

    private async Task<(bool Succes, string? Erreur)> EnvoyerLienConnexionAsync(User user, string? retour, string methode)
    {
        // anti-spam : un seul email par minute et par compte
        bool dejaEnvoye = await _context.ConnexionTokens.AnyAsync(t =>
            t.UserId == user.Id && !t.EstUtilise && t.DateExpiration > DateTime.UtcNow.AddMinutes(14));
        if (dejaEnvoye)
        {
            return (false, "Un email vient d'être envoyé. Patiente une minute avant de réessayer.");
        }

        var token = new ConnexionToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant(),
            DateExpiration = DateTime.UtcNow.AddMinutes(15),
            EstUtilise = false
        };
        _context.ConnexionTokens.Add(token);
        await _context.SaveChangesAsync();

        var baseUrl = (_config["FrontendUrl"] ?? "http://localhost:5173").TrimEnd('/');
        var lien = System.Net.WebUtility.HtmlEncode(
            $"{baseUrl}/confirmer-connexion?token={token.Token}&retour={Uri.EscapeDataString(RetourSur(retour))}");
        var nom = System.Net.WebUtility.HtmlEncode(user.Nom);

        try
        {
            await _emailService.EnvoyerAsync(
                user.Email,
                "Confirme ta connexion à Wishly",
                $@"<p>Bonjour {nom},</p>
                   <p>Une connexion {methode} a été demandée sur Wishly.</p>
                   <p><a href=""{lien}"" style=""display:inline-block;background:#111;color:white;padding:12px 24px;border-radius:24px;text-decoration:none;"">Oui, c'est moi</a></p>
                   <p>Si ce n'était pas toi, ignore cet email.</p>
                   <p>Ce lien expire dans 15 minutes et ne fonctionne qu'une fois.</p>");
        }
        catch
        {
            _context.ConnexionTokens.Remove(token);
            await _context.SaveChangesAsync();
            return (false, "Impossible d'envoyer l'email de confirmation. Réessaie plus tard.");
        }

        return (true, null);
    }

    public async Task<UserResponseDto?> ConfirmerConnexionAsync(string token)
    {
        var connexionToken = await _context.ConnexionTokens.FirstOrDefaultAsync(t => t.Token == token);

        if (connexionToken == null || connexionToken.EstUtilise || connexionToken.DateExpiration < DateTime.UtcNow)
        {
            return null;
        }

        connexionToken.EstUtilise = true;
        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(connexionToken.UserId);
        if (user == null) return null;

        return ToDto(user);
    }

    public async Task<UserResponseDto?> UpdatePhotoAsync(Guid userId, string photoUrl)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;

        user.PhotoUrl = photoUrl;
        await _context.SaveChangesAsync();

        return ToDto(user);
    }

    public async Task<(bool Succes, string? Erreur, UserResponseDto? User)> UpdateProfilAsync(Guid userId, UpdateProfilDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return (false, "Utilisateur introuvable.", null);

        if (!BCrypt.Net.BCrypt.Verify(dto.MotDePasseActuel, user.MotDePasseHash))
        {
            return (false, "Mot de passe incorrect.", null);
        }

        var nouvelEmail = (dto.Email ?? "").Trim().ToLowerInvariant();
        if (nouvelEmail != user.Email)
        {
            bool emailPris = await _context.Users.AnyAsync(u => u.Email == nouvelEmail && u.Id != userId);
            if (emailPris) return (false, "Cet email est déjà utilisé par un autre compte.", null);
        }

        user.Nom = dto.Nom;
        user.Email = nouvelEmail;
        await _context.SaveChangesAsync();

        return (true, null, ToDto(user));
    }

    public async Task<(bool Succes, string? Erreur)> ChangerMotDePasseAsync(Guid userId, ChangerMotDePasseDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return (false, "Utilisateur introuvable.");

        if (!BCrypt.Net.BCrypt.Verify(dto.MotDePasseActuel, user.MotDePasseHash))
        {
            return (false, "Mot de passe actuel incorrect.");
        }

        user.MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(dto.NouveauMotDePasse);
        await _context.SaveChangesAsync();

        return (true, null);
    }
}