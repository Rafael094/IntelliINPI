namespace IntelliINPI.Domain.Entities;

public sealed class RpiImportCheckpoint
{
    public Guid Id { get; set; }
    public int RpiNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAtUtc { get; set; }
    public DateTime? FinishedAtUtc { get; set; }
    public int DispatchesImported { get; set; }
    public int FailedRows { get; set; }
    public int DuplicateDispatches { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ZipPath { get; set; }
    public int XmlOrTxtFilesCount { get; set; }
}
