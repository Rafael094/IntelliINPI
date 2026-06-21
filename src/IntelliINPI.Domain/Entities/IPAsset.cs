namespace IntelliINPI.Domain.Entities;

public sealed class IPAsset
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? InpiProcessNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? OwnerName { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateOnly? FilingDate { get; set; }
    public DateOnly? GrantDate { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public DateOnly? InternalDeadline { get; set; }
    public Guid? ClientId { get; set; }
    public Client? Client { get; set; }
    public Guid? UniversityId { get; set; }
    public University? University { get; set; }
    public bool IsMonitored { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsActive { get; set; } = true;
}
