using IntelliINPI.Application.Trademarks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using IntelliINPI.Infrastructure.Persistence;

namespace IntelliINPI.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/trademarks")]
public sealed class TrademarksController(IMediator mediator, ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TrademarkDto>>> Search([FromQuery] string? term, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new SearchTrademarksQuery(term), cancellationToken));
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<TrademarkSearchItemDto>>> SearchLocal(
        [FromQuery] string? query,
        [FromQuery] string? niceClass,
        [FromQuery] string? status,
        [FromQuery] string? owner,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await mediator.Send(
            new SearchLocalTrademarksQuery(query, niceClass, status, owner, page, pageSize),
            cancellationToken));
    }

    [HttpGet("{processNumber}/detail")]
    public async Task<ActionResult<TrademarkDetailDto>> GetDetail(
        string processNumber,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTrademarkDetailQuery(processNumber), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("{processNumber}/logo")]
    public async Task<IActionResult> GetLogo(string processNumber, CancellationToken cancellationToken)
    {
        var logo = await dbContext.Trademarks
            .AsNoTracking()
            .Where(x => x.ProcessNumber == processNumber && x.LogoPath != null)
            .Select(x => new { x.LogoPath, x.LogoContentType })
            .FirstOrDefaultAsync(cancellationToken);

        if (logo?.LogoPath is null || !System.IO.File.Exists(logo.LogoPath))
        {
            return NotFound();
        }

        var contentType = string.IsNullOrWhiteSpace(logo.LogoContentType) ? "image/jpeg" : logo.LogoContentType;
        return PhysicalFile(Path.GetFullPath(logo.LogoPath), contentType);
    }

    [HttpPost("availability-analysis")]
    public async Task<ActionResult<TrademarkAvailabilityAnalysisDto>> AnalyzeAvailability(
        AnalyzeTrademarkAvailabilityRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(
            new AnalyzeTrademarkAvailabilityQuery(request.ProposedName, request.ActivityDescription),
            cancellationToken));
    }
}
