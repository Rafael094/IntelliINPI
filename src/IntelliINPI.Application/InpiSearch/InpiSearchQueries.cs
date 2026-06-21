using IntelliINPI.Application.Abstractions;
using MediatR;

namespace IntelliINPI.Application.InpiSearch;

public sealed record SearchInpiTrademarksBasicQuery(string? Query, string? NiceClass, bool Exact, int Page, int PageSize)
    : IRequest<InpiSearchResponse<InpiTrademarkResult>>;

public sealed record SearchInpiTrademarksAdvancedQuery(
    string? ProcessNumber,
    string? TrademarkName,
    bool Exact,
    string? Owner,
    string? NiceClass,
    string? Status,
    string? DispatchCode,
    DateOnly? StartDate,
    DateOnly? EndDate,
    bool LiveOnly,
    string? Presentation,
    string? Nature,
    int Page,
    int PageSize) : IRequest<InpiSearchResponse<InpiTrademarkResult>>;

public sealed record SearchInpiTrademarksBooleanQuery(string? Expression, int Page, int PageSize)
    : IRequest<InpiSearchResponse<InpiTrademarkResult>>;

public sealed record SearchInpiPatentsBasicQuery(string? Query, bool Exact, int Page, int PageSize, string? ProcessNumber = null,
    string? GruNumber = null, string? ProtocolNumber = null, string? SearchMode = null, string? SearchField = null)
    : IRequest<InpiSearchResponse<InpiPatentResult>>;

public sealed record SearchInpiPatentsAdvancedQuery(
    string? ProcessNumber,
    string? Title,
    string? Applicant,
    string? Inventor,
    string? IpcClass,
    string? Status,
    DateOnly? StartDate,
    DateOnly? EndDate,
    int Page,
    int PageSize,
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
    bool GrantedOnly = false) : IRequest<InpiSearchResponse<InpiPatentResult>>;

public sealed record SearchInpiPatentsBooleanQuery(string? Expression, int Page, int PageSize)
    : IRequest<InpiSearchResponse<InpiPatentResult>>;

public sealed class SearchInpiTrademarksBasicQueryHandler(IInpiSearchService inpiSearchService)
    : IRequestHandler<SearchInpiTrademarksBasicQuery, InpiSearchResponse<InpiTrademarkResult>>
{
    public Task<InpiSearchResponse<InpiTrademarkResult>> Handle(SearchInpiTrademarksBasicQuery request, CancellationToken cancellationToken)
    {
        return inpiSearchService.SearchTrademarksBasicAsync(
            new InpiTrademarkSearchRequest(request.Query, request.Exact, null, null, null, request.NiceClass, null, null, null, null, null, request.Page, request.PageSize),
            cancellationToken);
    }
}

public sealed class SearchInpiTrademarksAdvancedQueryHandler(IInpiSearchService inpiSearchService)
    : IRequestHandler<SearchInpiTrademarksAdvancedQuery, InpiSearchResponse<InpiTrademarkResult>>
{
    public Task<InpiSearchResponse<InpiTrademarkResult>> Handle(SearchInpiTrademarksAdvancedQuery request, CancellationToken cancellationToken)
    {
        return inpiSearchService.SearchTrademarksAdvancedAsync(
            new InpiTrademarkSearchRequest(
                null,
                request.Exact,
                request.ProcessNumber,
                request.TrademarkName,
                request.Owner,
                request.NiceClass,
                request.Status,
                request.DispatchCode,
                request.StartDate,
                request.EndDate,
                null,
                request.Page,
                request.PageSize,
                request.LiveOnly,
                request.Presentation,
                request.Nature),
            cancellationToken);
    }
}

public sealed class SearchInpiTrademarksBooleanQueryHandler(IInpiSearchService inpiSearchService)
    : IRequestHandler<SearchInpiTrademarksBooleanQuery, InpiSearchResponse<InpiTrademarkResult>>
{
    public Task<InpiSearchResponse<InpiTrademarkResult>> Handle(SearchInpiTrademarksBooleanQuery request, CancellationToken cancellationToken)
    {
        return inpiSearchService.SearchTrademarksBooleanAsync(
            new InpiTrademarkSearchRequest(null, false, null, null, null, null, null, null, null, null, request.Expression, request.Page, request.PageSize),
            cancellationToken);
    }
}

public sealed class SearchInpiPatentsBasicQueryHandler(IInpiSearchService inpiSearchService)
    : IRequestHandler<SearchInpiPatentsBasicQuery, InpiSearchResponse<InpiPatentResult>>
{
    public Task<InpiSearchResponse<InpiPatentResult>> Handle(SearchInpiPatentsBasicQuery request, CancellationToken cancellationToken)
    {
        return inpiSearchService.SearchPatentsBasicAsync(
            new InpiPatentSearchRequest(request.Query, request.Exact, request.ProcessNumber, null, null, null, null, null, null, null, null, request.Page, request.PageSize,
                request.GruNumber, request.ProtocolNumber, request.SearchMode, request.SearchField),
            cancellationToken);
    }
}

public sealed class SearchInpiPatentsAdvancedQueryHandler(IInpiSearchService inpiSearchService)
    : IRequestHandler<SearchInpiPatentsAdvancedQuery, InpiSearchResponse<InpiPatentResult>>
{
    public Task<InpiSearchResponse<InpiPatentResult>> Handle(SearchInpiPatentsAdvancedQuery request, CancellationToken cancellationToken)
    {
        return inpiSearchService.SearchPatentsAdvancedAsync(
            new InpiPatentSearchRequest(
                null,
                false,
                request.ProcessNumber,
                request.Title,
                request.Applicant,
                request.Inventor,
                request.IpcClass,
                request.Status,
                request.StartDate,
                request.EndDate,
                null,
                request.Page,
                request.PageSize,
                PriorityNumber: request.PriorityNumber,
                PctNumber: request.PctNumber,
                PriorityStartDate: request.PriorityStartDate,
                PriorityEndDate: request.PriorityEndDate,
                PctDepositStartDate: request.PctDepositStartDate,
                PctDepositEndDate: request.PctDepositEndDate,
                PctPublicationStartDate: request.PctPublicationStartDate,
                PctPublicationEndDate: request.PctPublicationEndDate,
                Abstract: request.Abstract,
                IpcKeyword: request.IpcKeyword,
                ApplicantDocument: request.ApplicantDocument,
                GrantedOnly: request.GrantedOnly),
            cancellationToken);
    }
}

public sealed class SearchInpiPatentsBooleanQueryHandler(IInpiSearchService inpiSearchService)
    : IRequestHandler<SearchInpiPatentsBooleanQuery, InpiSearchResponse<InpiPatentResult>>
{
    public Task<InpiSearchResponse<InpiPatentResult>> Handle(SearchInpiPatentsBooleanQuery request, CancellationToken cancellationToken)
    {
        return inpiSearchService.SearchPatentsBooleanAsync(
            new InpiPatentSearchRequest(null, false, null, null, null, null, null, null, null, null, request.Expression, request.Page, request.PageSize),
            cancellationToken);
    }
}
