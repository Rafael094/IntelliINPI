using System.Net.Http.Json;
using System.Text.Json;
using IntelliINPI.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace IntelliINPI.Infrastructure.Services;

public sealed class ExaBrandSearchClient(HttpClient httpClient, IOptions<ExternalSearchOptions> options)
    : IExternalBrandSearchClient
{
    private readonly ExternalSearchOptions options = options.Value;

    public async Task<IReadOnlyList<ExternalBrandSearchResult>> SearchAsync(string query, int maxResults, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            return [];
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "search");
        request.Headers.Add("x-api-key", options.ApiKey);
        request.Content = JsonContent.Create(new
        {
            query,
            numResults = Math.Clamp(maxResults, 1, 10),
            type = "auto"
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        if (!document.RootElement.TryGetProperty("results", out var resultsElement)
            || resultsElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var results = new List<ExternalBrandSearchResult>();
        foreach (var item in resultsElement.EnumerateArray())
        {
            var url = ReadString(item, "url");
            if (string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            var title = ReadString(item, "title");
            var text = ReadString(item, "text");
            var score = ReadDecimal(item, "score");

            results.Add(new ExternalBrandSearchResult(
                "Exa",
                query,
                string.IsNullOrWhiteSpace(title) ? url : title,
                url,
                text,
                score));
        }

        return results;
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : property.ToString();
    }

    private static decimal? ReadDecimal(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.Number && property.TryGetDecimal(out var value)
            ? value
            : null;
    }
}
