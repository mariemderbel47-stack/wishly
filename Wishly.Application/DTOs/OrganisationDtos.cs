namespace Wishly.Application.DTOs;

public class CreateOrganisationDto
{
    public Guid UserId { get; set; }
    public string NomOccasion { get; set; } = string.Empty;
    public DateTime? DateOccasion { get; set; }
}

public class CreateEtapeDto
{
    public Guid OrganisationId { get; set; }
    public string Texte { get; set; } = string.Empty;
}

public class EtapeResponseDto
{
    public Guid Id { get; set; }
    public string Texte { get; set; } = string.Empty;
    public bool EstTerminee { get; set; }
}

public class OrganisationResponseDto
{
    public Guid Id { get; set; }
    public string NomOccasion { get; set; } = string.Empty;
    public DateTime? DateOccasion { get; set; }
    public List<EtapeResponseDto> Etapes { get; set; } = new();
}