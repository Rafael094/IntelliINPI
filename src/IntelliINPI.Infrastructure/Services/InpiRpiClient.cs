using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using IntelliINPI.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace IntelliINPI.Infrastructure.Services;

public sealed class InpiRpiClient(HttpClient httpClient, IOptions<InpiRpiOptions> options) : IInpiRpiClient
{
    private const int MaxAttempts = 3;
    private readonly InpiRpiOptions _options = options.Value;

    public Task<InpiRpiDownloadResult> DownloadTrademarkFilesAsync(int? rpiNumber, CancellationToken cancellationToken)
    {
        return DownloadTrademarkFilesAsync(rpiNumber, (_, _) => Task.CompletedTask, cancellationToken);
    }

    public async Task<int> GetLatestTrademarkRpiNumberAsync(CancellationToken cancellationToken)
    {
        var indexHtml = await httpClient.GetStringAsync(_options.BaseUrl, cancellationToken);
        var latest = ExtractTrademarkZipLinks(indexHtml).OrderByDescending(x => x.RpiNumber).FirstOrDefault();

        if (latest is null)
        {
            throw new InvalidOperationException("Nenhum ZIP de marcas foi encontrado na pagina da RPI.");
        }

        return latest.RpiNumber;
    }

    public async Task<InpiRpiDownloadResult> DownloadTrademarkFilesAsync(
        int? rpiNumber,
        Func<string, CancellationToken, Task> logAsync,
        CancellationToken cancellationToken)
    {
        var indexHtml = await httpClient.GetStringAsync(_options.BaseUrl, cancellationToken);
        var links = ExtractTrademarkZipLinks(indexHtml);
        var selected = rpiNumber is null or 0
            ? links.OrderByDescending(x => x.RpiNumber).FirstOrDefault()
            : links.FirstOrDefault(x => x.RpiNumber == rpiNumber.Value);

        if (selected is null)
        {
            if (rpiNumber is null or 0)
            {
                throw new InvalidOperationException("Nenhum ZIP de marcas foi encontrado na pagina da RPI.");
            }

            selected = CreateDirectTrademarkZipLink(rpiNumber.Value);
            await logAsync($"RPI download: RPI {rpiNumber} nao encontrada no indice; tentando URL direta {selected.Url}.", cancellationToken);
        }

        await logAsync($"RPI download: link real selecionado para marcas: {selected.Url}.", cancellationToken);

        var editionDirectory = Path.Combine(_options.RawDirectory, selected.RpiNumber.ToString());
        Directory.CreateDirectory(editionDirectory);

        var zipPath = Path.Combine(editionDirectory, selected.FileName);
        if (IsValidZipFile(zipPath))
        {
            await logAsync($"RPI download: ZIP local valido encontrado em {zipPath}. Download ignorado.", cancellationToken);
        }
        else
        {
            await DownloadZipWithRetryAsync(selected.Url, zipPath, logAsync, cancellationToken);
        }

        var files = new List<InpiRpiFile>
        {
            new(selected.FileName, zipPath, selected.Url.ToString())
        };

        using var archive = ZipFile.OpenRead(zipPath);
        foreach (var entry in archive.Entries.Where(IsSupportedEntry))
        {
            var destinationPath = Path.Combine(editionDirectory, Path.GetFileName(entry.FullName));
            entry.ExtractToFile(destinationPath, overwrite: true);
            files.Add(new InpiRpiFile(entry.FullName, destinationPath, selected.Url.ToString()));
        }

        return new InpiRpiDownloadResult(selected.RpiNumber, files);
    }

    private async Task DownloadZipWithRetryAsync(
        Uri url,
        string zipPath,
        Func<string, CancellationToken, Task> logAsync,
        CancellationToken cancellationToken)
    {
        Exception? lastException = null;

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            var tempPath = $"{zipPath}.tmp";
            TryDelete(tempPath);

            try
            {
                await LogRemoteMetadataAsync(url, attempt, logAsync, cancellationToken);
                await logAsync($"RPI download tentativa {attempt}/{MaxAttempts}: iniciando GET {url}.", cancellationToken);

                using var request = CreateRequest(HttpMethod.Get, url);
                using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                await LogResponseAsync($"RPI download tentativa {attempt}/{MaxAttempts}: GET", response, logAsync, cancellationToken);
                response.EnsureSuccessStatusCode();

                await using (var source = await response.Content.ReadAsStreamAsync(cancellationToken))
                await using (var destination = File.Create(tempPath))
                {
                    await source.CopyToAsync(destination, cancellationToken);
                }

                var downloadedSize = new FileInfo(tempPath).Length;
                await logAsync($"RPI download tentativa {attempt}/{MaxAttempts}: tamanho baixado={downloadedSize} bytes; arquivo temporario={tempPath}.", cancellationToken);
                ValidateZipFile(tempPath);

                TryDelete(zipPath);
                File.Move(tempPath, zipPath);
                await logAsync($"RPI download concluido: arquivo ZIP valido salvo em {zipPath}.", cancellationToken);
                return;
            }
            catch (Exception exception) when (attempt < MaxAttempts)
            {
                lastException = exception;
                await logAsync($"RPI download tentativa {attempt}/{MaxAttempts}: erro={exception.Message}. Nova tentativa sera feita.", cancellationToken);
                TryDelete(tempPath);
                await Task.Delay(TimeSpan.FromSeconds(attempt * 5), cancellationToken);
            }
            catch (Exception exception)
            {
                lastException = exception;
                await logAsync($"RPI download tentativa {attempt}/{MaxAttempts}: erro final={exception.Message}.", cancellationToken);
                TryDelete(tempPath);
            }
        }

        throw new InvalidOperationException($"Falha ao baixar ZIP da RPI apos {MaxAttempts} tentativas: {lastException?.Message}", lastException);
    }

    private async Task LogRemoteMetadataAsync(
        Uri url,
        int attempt,
        Func<string, CancellationToken, Task> logAsync,
        CancellationToken cancellationToken)
    {
        try
        {
            using var request = CreateRequest(HttpMethod.Head, url);
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            await LogResponseAsync($"RPI download tentativa {attempt}/{MaxAttempts}: HEAD", response, logAsync, cancellationToken);
        }
        catch (Exception exception)
        {
            await logAsync($"RPI download tentativa {attempt}/{MaxAttempts}: HEAD falhou para {url}; erro={exception.Message}.", cancellationToken);
        }
    }

    private static Task LogResponseAsync(
        string prefix,
        HttpResponseMessage response,
        Func<string, CancellationToken, Task> logAsync,
        CancellationToken cancellationToken)
    {
        var finalUrl = response.RequestMessage?.RequestUri?.ToString() ?? "n/a";
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "n/a";
        var contentLength = response.Content.Headers.ContentLength?.ToString() ?? "n/a";
        return logAsync($"{prefix}: status={(int)response.StatusCode}; content-type={contentType}; content-length={contentLength}; redirect-final={finalUrl}.", cancellationToken);
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, Uri url)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0 Safari/537.36 IntelliINPI/1.0");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/zip"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));
        return request;
    }

    private static void ValidateZipFile(string path)
    {
        if (!IsValidZipFile(path))
        {
            throw new InvalidOperationException("Arquivo baixado nao possui assinatura ZIP valida ou nao contem entradas.");
        }
    }

    private static bool IsValidZipFile(string path)
    {
        var fileInfo = new FileInfo(path);
        if (!fileInfo.Exists || fileInfo.Length <= 0)
        {
            return false;
        }

        Span<byte> signature = stackalloc byte[2];
        using var stream = File.OpenRead(path);
        var read = stream.Read(signature);
        if (read < 2 || signature[0] != 0x50 || signature[1] != 0x4B)
        {
            return false;
        }

        using var archive = ZipFile.OpenRead(path);
        return archive.Entries.Count > 0;
    }

    private IReadOnlyList<RpiZipLink> ExtractTrademarkZipLinks(string html)
    {
        var matches = Regex.Matches(html, "href=[\"'](?<href>[^\"']*/txt/RM(?<rpi>\\d+)\\.zip)[\"']", RegexOptions.IgnoreCase);
        var links = new List<RpiZipLink>();

        foreach (Match match in matches)
        {
            var href = WebUtility.HtmlDecode(match.Groups["href"].Value);
            var url = new Uri(new Uri(_options.BaseUrl), href);
            var fileName = Path.GetFileName(url.LocalPath);

            if (int.TryParse(match.Groups["rpi"].Value, out var rpiNumber))
            {
                links.Add(new RpiZipLink(rpiNumber, url, fileName));
            }
        }

        return links;
    }

    private RpiZipLink CreateDirectTrademarkZipLink(int rpiNumber)
    {
        var fileName = $"RM{rpiNumber}.zip";
        var url = new Uri(new Uri(_options.BaseUrl), $"/txt/{fileName}");
        return new RpiZipLink(rpiNumber, url, fileName);
    }

    private static bool IsSupportedEntry(ZipArchiveEntry entry)
    {
        var extension = Path.GetExtension(entry.FullName);
        return string.Equals(extension, ".xml", StringComparison.OrdinalIgnoreCase)
            || string.Equals(extension, ".txt", StringComparison.OrdinalIgnoreCase);
    }

    private static void TryDelete(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private sealed record RpiZipLink(int RpiNumber, Uri Url, string FileName);
}
