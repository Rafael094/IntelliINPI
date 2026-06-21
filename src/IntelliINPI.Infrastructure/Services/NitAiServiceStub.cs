using IntelliINPI.Application.Abstractions;

namespace IntelliINPI.Infrastructure.Services;

public sealed class NitAiServiceStub : INitAiService
{
    private const string Message = "Not implemented in local MVP";

    public Task<string> GeneratePatentDraftAsync(Guid inventionId, CancellationToken cancellationToken)
    {
        return Task.FromResult(Message);
    }

    public Task<string> GenerateNdaDraftAsync(Guid inventionId, CancellationToken cancellationToken)
    {
        return Task.FromResult(Message);
    }

    public Task<string> AnalyzePriorArtAsync(Guid inventionId, CancellationToken cancellationToken)
    {
        return Task.FromResult(Message);
    }
}
