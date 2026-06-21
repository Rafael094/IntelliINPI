namespace IntelliINPI.Domain.Entities;

public sealed class InpiDeadline
{
    public Guid Id { get; set; }
    public Guid IPAssetId { get; set; }
    public IPAsset IPAsset { get; set; } = null!;
    public string Type { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public int? SourceRpiNumber { get; set; }
    public string? SourceDispatchCode { get; set; }
    public DateOnly? BaseDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public string? LegalBasis { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
