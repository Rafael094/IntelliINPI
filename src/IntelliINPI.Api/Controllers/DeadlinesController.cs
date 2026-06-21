using IntelliINPI.Application.Operational;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliINPI.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/deadlines")]
public sealed class DeadlinesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DeadlineDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new ListDeadlinesQuery(), cancellationToken));
    }

    [HttpGet("operational")]
    public async Task<ActionResult<IReadOnlyList<OperationalDeadlineDto>>> ListOperational(
        [FromQuery] int daysAhead = 365,
        [FromQuery] bool includeManualReview = true,
        CancellationToken cancellationToken = default)
    {
        return Ok(await mediator.Send(new ListOperationalDeadlinesQuery(daysAhead, includeManualReview), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<DeadlineDto>> Create(DeadlineRequest request, CancellationToken cancellationToken)
    {
        var deadline = await mediator.Send(
            new CreateDeadlineCommand(request.Title, request.Description, request.DueDate, request.Status, request.Type, request.ClientId, request.TrademarkId, request.InventionId),
            cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = deadline.Id }, deadline);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DeadlineDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetDeadlineQuery(id), cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DeadlineDto>> Update(Guid id, DeadlineRequest request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(
            new UpdateDeadlineCommand(id, request.Title, request.Description, request.DueDate, request.Status, request.Type, request.ClientId, request.TrademarkId, request.InventionId),
            cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteDeadlineCommand(id), cancellationToken);
        return NoContent();
    }
}
