namespace IntelliINPI.Domain.Entities;

public sealed class Company
{
    public Guid Id { get; set; }
    public Guid? UniversityId { get; set; }
    public University? University { get; set; }
    public string LegalName { get; set; } = string.Empty;
    public string? TradeName { get; set; }
    public string? Cnpj { get; set; }
    public string? Segment { get; set; }
    public string? Size { get; set; }
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<TechnologyTransferContract> Contracts { get; set; } = [];
    public ICollection<TechnologyTransferOpportunity> TransferOpportunities { get; set; } = [];
}
