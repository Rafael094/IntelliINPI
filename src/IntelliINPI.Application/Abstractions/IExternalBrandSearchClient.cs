namespace IntelliINPI.Application.Abstractions;

public interface IExternalBrandSearchClient
{
    Task<IReadOnlyList<ExternalBrandSearchResult>> SearchAsync(string query, int maxResults, CancellationToken cancellationToken);
}

public sealed record ExternalBrandSearchResult(
    string Source,
    string Query,
    string Title,
    string Url,
    string? Snippet,
    decimal? Score);
