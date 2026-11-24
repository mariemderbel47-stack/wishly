namespace Wishly.Application.DTOs;

public class RegisterUserDto
{
    public string Nom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MotDePasse { get; set; } = string.Empty;
}

public class LoginUserDto
{
    public string Email { get; set; } = string.Empty;
    public string MotDePasse { get; set; } = string.Empty;
}

public class UserResponseDto
{
    public Guid Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string? Token { get; set; }
}

public class PhotoDto
{
    public string PhotoUrl { get; set; } = string.Empty;
}

public class UpdateProfilDto
{
    public string Nom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MotDePasseActuel { get; set; } = string.Empty;
}

public class ChangerMotDePasseDto
{
    public string MotDePasseActuel { get; set; } = string.Empty;
    public string NouveauMotDePasse { get; set; } = string.Empty;
}

public class ConfirmerConnexionDto
{
    public string Token { get; set; } = string.Empty;
}