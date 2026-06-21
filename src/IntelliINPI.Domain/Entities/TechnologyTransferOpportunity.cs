namespace IntelliINPI.Domain.Entities;

public sealed class TechnologyTransferOpportunity
{
    public Guid Id { get; set; }
    public Guid InventionId { get; set; }
    public Invention Invention { get; set; } = null!;
    public Guid UniversityId { get; set; }
    public University University { get; set; } = null!;
    public Guid? CompanyId { get; set; }
    public Company? Company { get; set; }
    public string Stage { get; set; } = "Nova Tecnologia";
    public string? Notes { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public bool IsActive { get; set; } = true;
}
