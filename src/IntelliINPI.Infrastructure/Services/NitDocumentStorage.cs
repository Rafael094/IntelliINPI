using IntelliINPI.Application.Abstractions;
using IntelliINPI.Application.Common.Exceptions;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace IntelliINPI.Infrastructure.Services;

public sealed class NitDocumentStorage(IConfiguration configuration) : INitDocumentStorage
{
    private readonly string root = Path.GetFullPath(configuration["NitDocuments:StoragePath"] ?? Path.Combine("data", "nit", "documents"));

    public async Task<NitStoredDocument> SaveAsync(Stream content, string fileName, bool encrypt, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(root);
        var extension = Path.GetExtension(fileName);
        var storedName = encrypt ? $"{Guid.NewGuid():N}.enc" : $"{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(root, storedName);
        string? ivBase64 = null;
        var encryptionKey = encrypt ? GetEncryptionKey() : null;

        try
        {
            await using var output = File.Create(path);
            if (encryptionKey is not null)
            {
                using var aes = Aes.Create();
                aes.Key = encryptionKey;
                aes.GenerateIV();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                ivBase64 = Convert.ToBase64String(aes.IV);
                await using var crypto = new CryptoStream(output, aes.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: false);
                await content.CopyToAsync(crypto, cancellationToken);
                await crypto.FlushFinalBlockAsync(cancellationToken);
            }
            else
            {
                await content.CopyToAsync(output, cancellationToken);
            }
        }
        catch
        {
            if (File.Exists(path)) File.Delete(path);
            throw;
        }

        return new NitStoredDocument(path, storedName, encrypt, encrypt ? "AES-256-CBC" : null, ivBase64);
    }

    public async Task<Stream> OpenReadAsync(string storagePath, bool isEncrypted, string? encryptionIv, CancellationToken cancellationToken)
    {
        EnsureInsideRoot(storagePath);
        if (!isEncrypted) return File.OpenRead(storagePath);
        if (string.IsNullOrWhiteSpace(encryptionIv)) throw new InvalidOperationException("IV do documento criptografado não encontrado.");

        await using var input = File.OpenRead(storagePath);
        using var aes = Aes.Create();
        aes.Key = GetEncryptionKey();
        aes.IV = Convert.FromBase64String(encryptionIv);
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        await using var crypto = new CryptoStream(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
        var output = new MemoryStream();
        await crypto.CopyToAsync(output, cancellationToken);
        output.Position = 0;
        return output;
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken)
    {
        EnsureInsideRoot(storagePath);
        if (File.Exists(storagePath)) File.Delete(storagePath);
        return Task.CompletedTask;
    }

    private void EnsureInsideRoot(string path)
    {
        var fullPath = Path.GetFullPath(path);
        if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Caminho de documento inválido.");
    }

    private byte[] GetEncryptionKey()
    {
        var configured = configuration["DOCUMENT_ENCRYPTION_KEY"];
        if (string.IsNullOrWhiteSpace(configured))
            throw new ConfigurationAppException("Criptografia indisponível: configure DOCUMENT_ENCRYPTION_KEY e reinicie o backend.");

        try
        {
            var decoded = Convert.FromBase64String(configured);
            if (decoded.Length == 32) return decoded;
        }
        catch (FormatException)
        {
            // A chave também pode ser informada como texto UTF-8 com exatamente 32 bytes.
        }

        var bytes = Encoding.UTF8.GetBytes(configured);
        if (bytes.Length != 32)
            throw new ConfigurationAppException("DOCUMENT_ENCRYPTION_KEY deve possuir exatamente 32 bytes ou ser Base64 de 32 bytes.");
        return bytes;
    }
}
