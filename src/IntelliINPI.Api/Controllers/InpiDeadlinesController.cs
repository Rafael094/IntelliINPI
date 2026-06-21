using IntelliINPI.Application.IPAssets;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliINPI.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/inpi-deadlines")]
public sealed class InpiDeadlinesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InpiDeadlineDto>>> List([FromQuery] bool? isInternal, [FromQuery] int daysAhead = 90, CancellationToken cancellationToken = default)
    {
        return Ok(await mediator.Send(new ListInpiDeadlinesQuery(isInternal, daysAhead), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<InpiDeadlineDto>> Create(InpiDeadlineRequest request, CancellationToken cancellationToken)
    {
        var deadline = await mediator.Send(
            new CreateInpiDeadlineCommand(
                request.IPAssetId,
                request.Type,
                request.Source,
                request.SourceRpiNumber,
                request.SourceDispatchCode,
                request.BaseDate,
                request.DueDate,
                request.LegalBasis,
                request.Status,
                request.IsInternal,
                request.Notes),
            cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = deadline.Id }, deadline);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InpiDeadlineDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetInpiDeadlineQuery(id), cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<InpiDeadlineDto>> Update(Guid id, InpiDeadlineRequest request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(
            new UpdateInpiDeadlineCommand(
                id,
                request.IPAssetId,
                request.Type,
                request.Source,
                request.SourceRpiNumber,
                request.SourceDispatchCode,
                request.BaseDate,
                request.DueDate,
                request.LegalBasis,
                request.Status,
                request.IsInternal,
                request.Notes),
            cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteInpiDeadlineCommand(id), cancellationToken);
        return NoContent();
    }
}
