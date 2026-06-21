using IntelliINPI.Application.Imports;
using IntelliINPI.Domain.Entities;

namespace IntelliINPI.Application.Abstractions;

public interface ITrademarkImporter
{
    ImportSource Source { get; }
    Task<ImportTrademarksResult> ImportAsync(CancellationToken cancellationToken);
}

public interface IInpiOpenDataTrademarkImporter : ITrademarkImporter;

public interface IInpiRpiTrademarkImporter : ITrademarkImporter
{
    Task<ImportTrademarksResult> ImportAsync(int? rpiNumber, CancellationToken cancellationToken);
}

public interface IInpiRpiClient
{
    Task<int> GetLatestTrademarkRpiNumberAsync(CancellationToken cancellationToken);
    Task<InpiRpiDownloadResult> DownloadTrademarkFilesAsync(int? rpiNumber, CancellationToken cancellationToken);
    Task<InpiRpiDownloadResult> DownloadTrademarkFilesAsync(
        int? rpiNumber,
        Func<string, CancellationToken, Task> logAsync,
        CancellationToken cancellationToken);
}

public interface IInpiRpiHistorySettings
{
    string RawDirectory { get; }
    int? GetStartRpiForYear(int year);
}

public sealed record InpiRpiDownloadResult(int RpiNumber, IReadOnlyList<InpiRpiFile> Files);

public sealed record InpiRpiFile(string Name, string Path, string SourceUrl);

public interface IInpiSearchFallbackClient
{
    Task<IReadOnlyList<SearchFallbackTrademarkResult>> SearchByProcessNumberAsync(string processNumber, CancellationToken cancellationToken);
    Task<IReadOnlyList<SearchFallbackTrademarkResult>> SearchByNameAsync(string name, CancellationToken cancellationToken);
}

public sealed record SearchFallbackTrademarkResult(
    string ProcessNumber,
    string Name,
    string? Status,
    string SourceUrl);
