namespace IntelliINPI.Domain.Entities;

public sealed class ImportJob
{
    public Guid Id { get; set; }
    public string Source { get; set; } = string.Empty;
    public ImportSource SourceType { get; set; } = ImportSource.OpenDataCsv;
    public string Status { get; set; } = "Pending";
    public DateTime StartedAtUtc { get; set; }
    public DateTime? FinishedAtUtc { get; set; }
    public ICollection<ImportJobLog> Logs { get; set; } = [];
}
