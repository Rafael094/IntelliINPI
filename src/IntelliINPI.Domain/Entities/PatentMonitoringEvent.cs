namespace IntelliINPI.Domain.Entities;

public sealed class PatentMonitoringEvent
{
    public Guid Id { get; set; }
    public Guid MonitoredPatentId { get; set; }
    public MonitoredPatent MonitoredPatent { get; set; } = null!;
    public Guid PatentId { get; set; }
    public Patent Patent { get; set; } = null!;
    public Guid DispatchId { get; set; }
    public PatentDispatch Dispatch { get; set; } = null!;
    public string InpiProcessNumber { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string? PreviousDispatchCode { get; set; }
    public string? CurrentDispatchCode { get; set; }
    public DateOnly? PreviousDispatchDate { get; set; }
    public DateOnly? CurrentDispatchDate { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public bool IsRead { get; set; }
}
