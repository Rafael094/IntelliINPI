using IntelliINPI.Application.Nit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliINPI.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/nit/inventions")]
public sealed class NitInventionsController(IMediator mediator) : ControllerBase
{
    [HttpGet("portfolio")]
    public async Task<IActionResult> Portfolio(CancellationToken cancellationToken) => Ok(await mediator.Send(new ListPortfolioInventionsQuery(), cancellationToken));

    [HttpPost("portfolio")]
    public async Task<IActionResult> CreatePortfolio(PortfolioInventionRequest request, CancellationToken cancellationToken) => Ok(await mediator.Send(new SavePortfolioInventionCommand(null, request), cancellationToken));

    [HttpPut("{id:guid}/portfolio")]
    public async Task<IActionResult> UpdatePortfolio(Guid id, PortfolioInventionRequest request, CancellationToken cancellationToken) => Ok(await mediator.Send(new SavePortfolioInventionCommand(id, request), cancellationToken));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InventionDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new ListNitInventionsQuery(), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<InventionDto>> Create(CreateInventionRequest request, CancellationToken cancellationToken)
    {
        var invention = await mediator.Send(
            new CreateNitInventionCommand(
                request.UniversityId,
                request.Title,
                request.Summary,
                request.Inventors,
                request.DepositDate,
                request.Status,
                request.PatentNumber,
                request.InpiProcessNumber),
            cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = invention.Id }, invention);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InventionDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetNitInventionQuery(id), cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<InventionDto>> Update(Guid id, UpdateInventionRequest request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(
            new UpdateNitInventionCommand(
                id,
                request.UniversityId,
                request.Title,
                request.Summary,
                request.Inventors,
                request.DepositDate,
                request.Status,
                request.PatentNumber,
                request.InpiProcessNumber),
            cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteNitInventionCommand(id), cancellationToken);
        return NoContent();
    }
}
