namespace Wishly.Domain.Entities;

public class Organisation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string NomOccasion { get; set; } = string.Empty;
    public DateTime? DateOccasion { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public ICollection<EtapeOrganisation> Etapes { get; set; } = new List<EtapeOrganisation>();
}