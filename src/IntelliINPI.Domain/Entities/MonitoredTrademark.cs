namespace IntelliINPI.Domain.Entities;

public sealed class MonitoredTrademark
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid TrademarkId { get; set; }
    public Trademark Trademark { get; set; } = null!;
    public string ProcessNumber { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastCheckedAtUtc { get; set; }
    public Guid? LastKnownDispatchId { get; set; }
    public TrademarkDispatch? LastKnownDispatch { get; set; }
    public string? LastKnownDispatchCode { get; set; }
    public DateOnly? LastKnownDispatchDate { get; set; }
    public bool HasPendingChanges { get; set; }
    public ICollection<TrademarkMonitoringEvent> Events { get; set; } = [];
}
