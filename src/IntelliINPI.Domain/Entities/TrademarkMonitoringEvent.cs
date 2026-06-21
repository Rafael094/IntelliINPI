namespace IntelliINPI.Domain.Entities;

public sealed class TrademarkMonitoringEvent
{
    public Guid Id { get; set; }
    public Guid MonitoredTrademarkId { get; set; }
    public MonitoredTrademark MonitoredTrademark { get; set; } = null!;
    public Guid TrademarkId { get; set; }
    public Trademark Trademark { get; set; } = null!;
    public Guid DispatchId { get; set; }
    public TrademarkDispatch Dispatch { get; set; } = null!;
    public string ProcessNumber { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string? PreviousDispatchCode { get; set; }
    public string? CurrentDispatchCode { get; set; }
    public DateOnly? PreviousDispatchDate { get; set; }
    public DateOnly? CurrentDispatchDate { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public bool IsRead { get; set; }
}
