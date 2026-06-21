using System.Net;
using System.Text.RegularExpressions;
using IntelliINPI.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace IntelliINPI.Infrastructure.Services;

public sealed class InpiOpenDataClient(HttpClient httpClient, IOptions<InpiOpenDataOptions> options) : IInpiOpenDataClient
{
    private readonly InpiOpenDataOptions _options = options.Value;

    public async Task<IReadOnlyList<InpiOpenDataFile>> DownloadTrademarkFilesAsync(CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_options.RawDirectory);

        var indexHtml = await httpClient.GetStringAsync(_options.BaseUrl, cancellationToken);
        var links = ExtractCsvLinks(indexHtml)
            .Where(link => _options.TrademarkFileNames.Any(name => link.FileName.Contains(name, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (links.Count == 0)
        {
            throw new InvalidOperationException("Nenhum CSV de marcas foi encontrado na página de Dados Abertos do INPI.");
        }

        var downloadedFiles = new List<InpiOpenDataFile>();
        foreach (var link in links)
        {
            var destinationPath = Path.Combine(_options.RawDirectory, link.FileName);
            await using var source = await httpClient.GetStreamAsync(link.Url, cancellationToken);
            await using var destination = File.Create(destinationPath);
            await source.CopyToAsync(destination, cancellationToken);
            downloadedFiles.Add(new InpiOpenDataFile(link.FileName, destinationPath, link.Url.ToString()));
        }

        return downloadedFiles;
    }

    private IReadOnlyList<CsvLink> ExtractCsvLinks(string html)
    {
        var matches = Regex.Matches(html, "href=[\"'](?<href>[^\"']+\\.csv[^\"']*)[\"']", RegexOptions.IgnoreCase);
        var links = new List<CsvLink>();

        foreach (Match match in matches)
        {
            var href = WebUtility.HtmlDecode(match.Groups["href"].Value);
            var url = new Uri(new Uri(_options.BaseUrl), href);
            var fileName = Path.GetFileName(url.LocalPath);

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                links.Add(new CsvLink(url, fileName));
            }
        }

        return links;
    }

    private sealed record CsvLink(Uri Url, string FileName);
}
