namespace IntelliINPI.Domain.Entities;

public sealed class Researcher
{
    public Guid Id { get; set; }
    public Guid UniversityId { get; set; }
    public University University { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Cpf { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string? LattesUrl { get; set; }
    public string? Orcid { get; set; }
    public string? Specialties { get; set; }
    public string? TechnologyAreas { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<InventionResearcher> Inventions { get; set; } = [];
}
