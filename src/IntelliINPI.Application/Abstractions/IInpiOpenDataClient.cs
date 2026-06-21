namespace IntelliINPI.Application.Abstractions;

public interface IInpiOpenDataClient
{
    Task<IReadOnlyList<InpiOpenDataFile>> DownloadTrademarkFilesAsync(CancellationToken cancellationToken);
}

public sealed record InpiOpenDataFile(string Name, string Path, string SourceUrl);
