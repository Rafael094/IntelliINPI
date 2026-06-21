namespace IntelliINPI.Application.Abstractions;

public interface INitDocumentStorage
{
    Task<NitStoredDocument> SaveAsync(Stream content, string fileName, bool encrypt, CancellationToken cancellationToken);
    Task<Stream> OpenReadAsync(string storagePath, bool isEncrypted, string? encryptionIv, CancellationToken cancellationToken);
    Task DeleteAsync(string storagePath, CancellationToken cancellationToken);
}

public sealed record NitStoredDocument(
    string StoragePath,
    string StoredFileName,
    bool IsEncrypted,
    string? EncryptionAlgorithm,
    string? EncryptionIV);
