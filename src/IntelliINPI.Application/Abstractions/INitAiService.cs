namespace IntelliINPI.Application.Abstractions;

public interface INitAiService
{
    Task<string> GeneratePatentDraftAsync(Guid inventionId, CancellationToken cancellationToken);
    Task<string> GenerateNdaDraftAsync(Guid inventionId, CancellationToken cancellationToken);
    Task<string> AnalyzePriorArtAsync(Guid inventionId, CancellationToken cancellationToken);
}
