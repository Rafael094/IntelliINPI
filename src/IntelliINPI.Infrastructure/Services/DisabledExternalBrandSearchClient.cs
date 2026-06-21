using IntelliINPI.Application.Abstractions;

namespace IntelliINPI.Infrastructure.Services;

public sealed class DisabledExternalBrandSearchClient : IExternalBrandSearchClient
{
    public Task<IReadOnlyList<ExternalBrandSearchResult>> SearchAsync(string query, int maxResults, CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<ExternalBrandSearchResult>>([]);
    }
}
