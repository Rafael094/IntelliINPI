namespace IntelliINPI.Application.Abstractions;

public interface INitInpiIntegrationService
{
    Task<string> SearchPriorArtAsync(string query, CancellationToken cancellationToken);
    Task<string> SyncPatentStatusAsync(Guid inventionId, CancellationToken cancellationToken);
}
