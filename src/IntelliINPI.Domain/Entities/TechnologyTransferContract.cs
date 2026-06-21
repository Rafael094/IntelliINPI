namespace IntelliINPI.Domain.Entities;

public sealed class TechnologyTransferContract
{
    public Guid Id { get; set; }
    public Guid InventionId { get; set; }
    public Invention Invention { get; set; } = null!;
    public string CompanyName { get; set; } = string.Empty;
    public string? Number { get; set; }
    public Guid? UniversityId { get; set; }
    public University? University { get; set; }
    public Guid? CompanyId { get; set; }
    public Company? Company { get; set; }
    public string Type { get; set; } = "Licenciamento";
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Term { get; set; }
    public bool AutomaticRenewal { get; set; }
    public decimal? RoyaltyPercentage { get; set; }
    public decimal? FixedValue { get; set; }
    public string? Cnpj { get; set; }
    public string RoyaltyModel { get; set; } = string.Empty;
    public decimal? RoyaltyValue { get; set; }
    public decimal? MinimumGuarantee { get; set; }
    public DateOnly? SignedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<RoyaltyPayment> RoyaltyPayments { get; set; } = [];
}
