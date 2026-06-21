namespace IntelliINPI.Domain.Entities;

public sealed class NitDocument
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Outros";
    public Guid UniversityId { get; set; }
    public University University { get; set; } = null!;
    public Guid? InventionId { get; set; }
    public Invention? Invention { get; set; }
    public Guid? ContractId { get; set; }
    public TechnologyTransferContract? Contract { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public long FileSize { get; set; }
    public bool IsEncrypted { get; set; }
    public string? EncryptionAlgorithm { get; set; }
    public string? EncryptionIV { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public Guid UploadedByUserId { get; set; }
    public User UploadedByUser { get; set; } = null!;
    public DateTime UploadedAtUtc { get; set; }
    public bool IsActive { get; set; } = true;
}
