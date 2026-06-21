using IntelliINPI.Application.Imports;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliINPI.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/import")]
public sealed class ImportsController(IMediator mediator) : ControllerBase
{
    [HttpPost("inpi/trademarks")]
    public async Task<ActionResult<ImportTrademarksResult>> ImportTrademarks(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new ImportTrademarksCommand(), cancellationToken));
    }

    [HttpPost("inpi/rpi/trademarks")]
    public async Task<ActionResult<ImportTrademarksResult>> ImportRpiTrademarks(
        ImportRpiTrademarksRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new ImportRpiTrademarksCommand(request.RpiNumber), cancellationToken));
    }

    [HttpPost("inpi/rpi/history/run")]
    public async Task<ActionResult<RpiHistoryImportStatusDto>> RunRpiHistory(
        RunRpiHistoryImportRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(
            new RunRpiHistoryImportCommand(
                request.StartYear,
                request.StartRpi,
                request.EndRpi,
                request.BatchSize,
                request.DelaySecondsBetweenBatches),
            cancellationToken));
    }

    [HttpGet("inpi/rpi/history/status")]
    public async Task<ActionResult<RpiHistoryImportStatusDto?>> RpiHistoryStatus(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetRpiHistoryImportStatusQuery(), cancellationToken));
    }

    [HttpGet("inpi/rpi/history/errors")]
    public async Task<ActionResult<IReadOnlyList<RpiHistoryImportErrorDto>>> RpiHistoryErrors(
        [FromQuery] Guid? runId,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetRpiHistoryImportErrorsQuery(runId), cancellationToken));
    }

    [HttpPost("inpi/rpi/history/resume")]
    public async Task<ActionResult<RpiHistoryImportStatusDto>> ResumeRpiHistory(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new ResumeRpiHistoryImportCommand(), cancellationToken));
    }

    [HttpPost("inpi/rpi/history/stop")]
    public async Task<ActionResult<RpiHistoryImportStatusDto>> StopRpiHistory(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new StopRpiHistoryImportCommand(), cancellationToken));
    }

    [HttpGet("inpi/status")]
    public async Task<ActionResult<ImportStatusDto>> Status(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetImportStatusQuery(), cancellationToken));
    }
}
