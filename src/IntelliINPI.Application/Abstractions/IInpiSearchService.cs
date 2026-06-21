namespace IntelliINPI.Application.Abstractions;

public enum InpiSearchResultSource
{
    OnlineInpi,
    LocalDatabase,
    OnlineFailedLocalFallback
}

public sealed record InpiTrademarkSearchRequest(
    string? Query,
    bool Exact,
    string? ProcessNumber,
    string? TrademarkName,
    string? Owner,
    string? NiceClass,
    string? Status,
    string? DispatchCode,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Expression,
    int Page,
    int PageSize,
    bool LiveOnly = false,
    string? Presentation = null,
    string? Nature = null);

public sealed record InpiPatentSearchRequest(
    string? Query,
    bool Exact,
    string? ProcessNumber,
    string? Title,
    string? Applicant,
    string? Inventor,
    string? IpcClass,
    string? Status,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Expression,
    int Page,
    int PageSize,
    string? GruNumber = null,
    string? ProtocolNumber = null,
    string? SearchMode = null,
    string? SearchField = null,
    string? PriorityNumber = null,
    string? PctNumber = null,
    DateOnly? PriorityStartDate = null,
    DateOnly? PriorityEndDate = null,
    DateOnly? PctDepositStartDate = null,
    DateOnly? PctDepositEndDate = null,
    DateOnly? PctPublicationStartDate = null,
    DateOnly? PctPublicationEndDate = null,
    string? Abstract = null,
    string? IpcKeyword = null,
    string? ApplicantDocument = null,
    bool GrantedOnly = false);

public sealed record InpiSearchResponse<T>(
    InpiSearchResultSource Source,
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    string? Warning);

public sealed record InpiTrademarkResult(
    Guid? LocalId,
    string ProcessNumber,
    string Name,
    string? Status,
    IReadOnlyList<string> NiceClasses,
    IReadOnlyList<string> Owners,
    DateOnly? FilingDate,
    DateOnly? RegistrationDate,
    DateOnly? LastDispatchDate,
    string? InpiDetailUrl = null);

public sealed record InpiPatentResult(
    Guid? LocalId,
    string? InpiProcessNumber,
    string Title,
    string? Abstract,
    IReadOnlyList<string> Applicants,
    IReadOnlyList<string> Inventors,
    string? IpcClass,
    DateOnly? FilingDate,
    DateOnly? PublicationDate,
    DateOnly? GrantDate,
    string? Status);

public interface IInpiSearchService
{
    Task<InpiSearchResponse<InpiTrademarkResult>> SearchTrademarksBasicAsync(InpiTrademarkSearchRequest request, CancellationToken cancellationToken);
    Task<InpiSearchResponse<InpiTrademarkResult>> SearchTrademarksAdvancedAsync(InpiTrademarkSearchRequest request, CancellationToken cancellationToken);
    Task<InpiSearchResponse<InpiTrademarkResult>> SearchTrademarksBooleanAsync(InpiTrademarkSearchRequest request, CancellationToken cancellationToken);
    Task<InpiSearchResponse<InpiPatentResult>> SearchPatentsBasicAsync(InpiPatentSearchRequest request, CancellationToken cancellationToken);
    Task<InpiSearchResponse<InpiPatentResult>> SearchPatentsAdvancedAsync(InpiPatentSearchRequest request, CancellationToken cancellationToken);
    Task<InpiSearchResponse<InpiPatentResult>> SearchPatentsBooleanAsync(InpiPatentSearchRequest request, CancellationToken cancellationToken);
    Task SyncTrademarkDetailAsync(string processNumber, CancellationToken cancellationToken);
    Task<InpiTrademarkResult?> GetTrademarkDetailAsync(string processNumber, CancellationToken cancellationToken);
    Task<InpiPatentResult?> GetPatentDetailAsync(string processNumber, CancellationToken cancellationToken);
}
