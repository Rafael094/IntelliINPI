using IntelliINPI.Application.Monitoring;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliINPI.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/monitoring")]
public sealed class MonitoringController(IMediator mediator) : ControllerBase
{
    [HttpPost("trademarks")]
    public async Task<ActionResult<Guid>> Add(AddMonitoredTrademarkRequest request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(
            new AddMonitoredTrademarkCommand(request.TrademarkId, request.ProcessNumber, request.Notes),
            cancellationToken));
    }

    [HttpGet("trademarks")]
    public async Task<ActionResult<IReadOnlyList<MonitoredTrademarkDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new ListMonitoredTrademarksQuery(), cancellationToken));
    }

    [HttpDelete("trademarks/{id:guid}")]
    public async Task<IActionResult> Remove(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new RemoveMonitoredTrademarkCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet("events")]
    public async Task<ActionResult<IReadOnlyList<TrademarkMonitoringEventDto>>> ListEvents(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new ListTrademarkMonitoringEventsQuery(false), cancellationToken));
    }

    [HttpGet("events/unread")]
    public async Task<ActionResult<IReadOnlyList<TrademarkMonitoringEventDto>>> ListUnreadEvents(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new ListTrademarkMonitoringEventsQuery(true), cancellationToken));
    }

    [HttpPost("events/{id:guid}/read")]
    public async Task<IActionResult> MarkEventAsRead(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new MarkTrademarkMonitoringEventAsReadCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("check")]
    public async Task<ActionResult<CheckTrademarkMonitoringResult>> Check(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new CheckTrademarkMonitoringCommand(), cancellationToken));
    }

    [HttpPost("patents")]
    public async Task<ActionResult<Guid>> AddPatent(AddMonitoredPatentRequest request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(
            new AddMonitoredPatentCommand(request.PatentId, request.InpiProcessNumber, request.Notes),
            cancellationToken));
    }

    [HttpGet("patents")]
    public async Task<ActionResult<IReadOnlyList<MonitoredPatentDto>>> ListPatents(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new ListMonitoredPatentsQuery(), cancellationToken));
    }

    [HttpDelete("patents/{id:guid}")]
    public async Task<IActionResult> RemovePatent(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new RemoveMonitoredPatentCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("patents/check")]
    public async Task<ActionResult<CheckPatentMonitoringResult>> CheckPatents(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new CheckPatentMonitoringCommand(), cancellationToken));
    }

    [HttpGet("patents/events")]
    public async Task<ActionResult<IReadOnlyList<PatentMonitoringEventDto>>> ListPatentEvents(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new ListPatentMonitoringEventsQuery(), cancellationToken));
    }
}
