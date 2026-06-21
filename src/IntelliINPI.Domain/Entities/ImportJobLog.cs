namespace IntelliINPI.Domain.Entities;

public sealed class ImportJobLog
{
    public Guid Id { get; set; }
    public Guid ImportJobId { get; set; }
    public ImportJob ImportJob { get; set; } = null!;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
