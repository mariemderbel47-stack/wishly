namespace Wishly.Domain.Entities;

public class EtapeOrganisation
{
    public Guid Id { get; set; }
    public Guid OrganisationId { get; set; }
    public string Texte { get; set; } = string.Empty;
    public bool EstTerminee { get; set; } = false;

    public Organisation? Organisation { get; set; }
}