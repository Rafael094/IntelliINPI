using IntelliINPI.Application.Abstractions;

namespace IntelliINPI.Infrastructure.Services;

public sealed class NitSignatureServiceStub : INitSignatureService
{
    private const string Message = "Not implemented in local MVP";

    public Task<string> CreateSignatureRequestAsync(Guid entityId, string documentType, CancellationToken cancellationToken)
    {
        return Task.FromResult(Message);
    }

    public Task<string> GetSignatureStatusAsync(Guid signatureRequestId, CancellationToken cancellationToken)
    {
        return Task.FromResult(Message);
    }
}
