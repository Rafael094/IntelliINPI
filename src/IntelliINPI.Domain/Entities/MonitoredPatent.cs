namespace IntelliINPI.Domain.Entities;

public sealed class MonitoredPatent
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid PatentId { get; set; }
    public Patent Patent { get; set; } = null!;
    public string InpiProcessNumber { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastCheckedAtUtc { get; set; }
    public Guid? LastKnownDispatchId { get; set; }
    public PatentDispatch? LastKnownDispatch { get; set; }
    public string? LastKnownDispatchCode { get; set; }
    public DateOnly? LastKnownDispatchDate { get; set; }
    public bool HasPendingChanges { get; set; }
    public ICollection<PatentMonitoringEvent> Events { get; set; } = [];
}
