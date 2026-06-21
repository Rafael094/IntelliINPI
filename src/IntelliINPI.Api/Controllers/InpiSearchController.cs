using IntelliINPI.Application.Abstractions;
using IntelliINPI.Application.InpiSearch;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliINPI.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/inpi/search")]
public sealed class InpiSearchController(IMediator mediator) : ControllerBase
{
    [HttpGet("trademarks/basic")]
    public async Task<ActionResult<InpiSearchResponse<InpiTrademarkResult>>> SearchTrademarksBasic(
        [FromQuery] string? query = null,
        [FromQuery] string? niceClass = null,
        [FromQuery] bool exact = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await mediator.Send(new SearchInpiTrademarksBasicQuery(query, niceClass, exact, page, pageSize), cancellationToken));
    }

    [HttpGet("trademarks/advanced")]
    public async Task<ActionResult<InpiSearchResponse<InpiTrademarkResult>>> SearchTrademarksAdvanced(
        [FromQuery] string? processNumber = null,
        [FromQuery] string? trademarkName = null,
        [FromQuery] bool exact = true,
        [FromQuery] string? owner = null,
        [FromQuery] string? niceClass = null,
        [FromQuery] string? status = null,
        [FromQuery] string? dispatchCode = null,
        [FromQuery] DateOnly? startDate = null,
        [FromQuery] DateOnly? endDate = null,
        [FromQuery] bool liveOnly = false,
        [FromQuery] string? presentation = null,
        [FromQuery] string? nature = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await mediator.Send(
            new SearchInpiTrademarksAdvancedQuery(processNumber, trademarkName, exact, owner, niceClass, status, dispatchCode, startDate, endDate, liveOnly, presentation, nature, page, pageSize),
            cancellationToken));
    }

    [HttpGet("trademarks/boolean")]
    public async Task<ActionResult<InpiSearchResponse<InpiTrademarkResult>>> SearchTrademarksBoolean(
        [FromQuery] string? expression,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await mediator.Send(new SearchInpiTrademarksBooleanQuery(expression, page, pageSize), cancellationToken));
    }

    [HttpGet("patents/basic")]
    public async Task<ActionResult<InpiSearchResponse<InpiPatentResult>>> SearchPatentsBasic(
        [FromQuery] string? query,
        [FromQuery] bool exact = false,
        [FromQuery] string? processNumber = null,
        [FromQuery] string? gruNumber = null,
        [FromQuery] string? protocolNumber = null,
        [FromQuery] string? searchMode = null,
        [FromQuery] string? searchField = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await mediator.Send(new SearchInpiPatentsBasicQuery(query, exact, page, pageSize, processNumber, gruNumber, protocolNumber, searchMode, searchField), cancellationToken));
    }

    [HttpGet("patents/advanced")]
    public async Task<ActionResult<InpiSearchResponse<InpiPatentResult>>> SearchPatentsAdvanced(
        [FromQuery] string? processNumber,
        [FromQuery] string? title,
        [FromQuery] string? applicant,
        [FromQuery] string? inventor,
        [FromQuery] string? ipcClass,
        [FromQuery] string? status,
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        [FromQuery] string? priorityNumber = null,
        [FromQuery] string? pctNumber = null,
        [FromQuery] DateOnly? priorityStartDate = null,
        [FromQuery] DateOnly? priorityEndDate = null,
        [FromQuery] DateOnly? pctDepositStartDate = null,
        [FromQuery] DateOnly? pctDepositEndDate = null,
        [FromQuery] DateOnly? pctPublicationStartDate = null,
        [FromQuery] DateOnly? pctPublicationEndDate = null,
        [FromQuery(Name = "abstract")] string? abstractText = null,
        [FromQuery] string? ipcKeyword = null,
        [FromQuery] string? applicantDocument = null,
        [FromQuery] bool grantedOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await mediator.Send(
            new SearchInpiPatentsAdvancedQuery(processNumber, title, applicant, inventor, ipcClass, status, startDate, endDate, page, pageSize,
                priorityNumber, pctNumber, priorityStartDate, priorityEndDate, pctDepositStartDate, pctDepositEndDate,
                pctPublicationStartDate, pctPublicationEndDate, abstractText, ipcKeyword, applicantDocument, grantedOnly),
            cancellationToken));
    }

    [HttpGet("patents/boolean")]
    public async Task<ActionResult<InpiSearchResponse<InpiPatentResult>>> SearchPatentsBoolean(
        [FromQuery] string? expression,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await mediator.Send(new SearchInpiPatentsBooleanQuery(expression, page, pageSize), cancellationToken));
    }
}
