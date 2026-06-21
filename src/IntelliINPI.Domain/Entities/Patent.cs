namespace IntelliINPI.Domain.Entities;

public sealed class Patent
{
    public Guid Id { get; set; }
    public string InpiProcessNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Abstract { get; set; }
    public string? Applicants { get; set; }
    public string? Inventors { get; set; }
    public string? IpcClass { get; set; }
    public DateOnly? FilingDate { get; set; }
    public DateOnly? PublicationDate { get; set; }
    public DateOnly? GrantDate { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<PatentDispatch> Dispatches { get; set; } = [];
    public ICollection<MonitoredPatent> Monitors { get; set; } = [];
}
