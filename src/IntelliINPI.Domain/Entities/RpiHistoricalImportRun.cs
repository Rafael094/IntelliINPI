namespace IntelliINPI.Domain.Entities;

public sealed class RpiHistoricalImportRun
{
    public Guid Id { get; set; }
    public int StartRpi { get; set; }
    public int EndRpi { get; set; }
    public int CurrentRpi { get; set; }
    public int BatchSize { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAtUtc { get; set; }
    public DateTime? FinishedAtUtc { get; set; }
    public int TotalRpis { get; set; }
    public int SuccessfulRpis { get; set; }
    public int FailedRpis { get; set; }
    public int SkippedRpis { get; set; }
    public int TotalDispatchesImported { get; set; }
    public int DuplicateDispatches { get; set; }
    public string? LastErrorSummary { get; set; }
    public string? ErrorMessage { get; set; }
}
