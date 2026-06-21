using IntelliINPI.Application.Abstractions;

namespace IntelliINPI.Infrastructure.Services;

public sealed class NitInpiIntegrationServiceStub : INitInpiIntegrationService
{
    private const string Message = "Not implemented in local MVP";

    public Task<string> SearchPriorArtAsync(string query, CancellationToken cancellationToken)
    {
        return Task.FromResult(Message);
    }

    public Task<string> SyncPatentStatusAsync(Guid inventionId, CancellationToken cancellationToken)
    {
        return Task.FromResult(Message);
    }
}
