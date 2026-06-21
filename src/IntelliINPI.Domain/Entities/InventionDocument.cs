namespace IntelliINPI.Domain.Entities;

public sealed class InventionDocument
{
    public Guid Id { get; set; }
    public Guid InventionId { get; set; }
    public Invention Invention { get; set; } = null!;
    public string Type { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileHash { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
