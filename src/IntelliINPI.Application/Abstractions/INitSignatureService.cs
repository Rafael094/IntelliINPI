namespace IntelliINPI.Application.Abstractions;

public interface INitSignatureService
{
    Task<string> CreateSignatureRequestAsync(Guid entityId, string documentType, CancellationToken cancellationToken);
    Task<string> GetSignatureStatusAsync(Guid signatureRequestId, CancellationToken cancellationToken);
}
