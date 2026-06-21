namespace IntelliINPI.Domain.Entities;

public sealed class Invention
{
    public Guid Id { get; set; }
    public Guid UniversityId { get; set; }
    public University University { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? ExecutiveSummary { get; set; }
    public string? TechnicalDescription { get; set; }
    public string? TechnologyArea { get; set; }
    public int? Trl { get; set; }
    public string? CommercialPotential { get; set; }
    public string? TargetMarket { get; set; }
    public string? ProtectionStatus { get; set; }
    public DateOnly? CreationDate { get; set; }
    public string? Responsible { get; set; }
    public string Inventors { get; set; } = string.Empty;
    public DateOnly? DepositDate { get; set; }
    public string Status { get; set; } = "Draft";
    public string? PatentNumber { get; set; }
    public string? InpiProcessNumber { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<InventionDocument> Documents { get; set; } = [];
    public ICollection<TechnologyTransferContract> TechnologyTransferContracts { get; set; } = [];
    public ICollection<InventionResearcher> Researchers { get; set; } = [];
    public ICollection<TechnologyTransferOpportunity> TransferOpportunities { get; set; } = [];
}
