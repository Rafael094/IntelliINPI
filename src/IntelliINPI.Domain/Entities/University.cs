namespace IntelliINPI.Domain.Entities;

public sealed class University
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TradeName { get; set; }
    public string? Cnpj { get; set; }
    public string Tier { get; set; } = string.Empty;
    public string Type { get; set; } = "Universidade";
    public string? Website { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? ContactName { get; set; }
    public string Status { get; set; } = "Ativa";
    public DateTime CreatedAtUtc { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<NitUserProfile> NitUserProfiles { get; set; } = [];
    public ICollection<Invention> Inventions { get; set; } = [];
    public ICollection<Researcher> Researchers { get; set; } = [];
    public ICollection<Company> Companies { get; set; } = [];
}
