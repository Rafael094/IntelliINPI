using IntelliINPI.Application.Nit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliINPI.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/nit/contracts")]
public sealed class NitContractsController(IMediator mediator) : ControllerBase
{
    [HttpGet("operational")]
    public async Task<IActionResult> Operational(CancellationToken cancellationToken) => Ok(await mediator.Send(new ListOperationalContractsQuery(), cancellationToken));

    [HttpPost("operational")]
    public async Task<IActionResult> CreateOperational(OperationalContractRequest request, CancellationToken cancellationToken) => Ok(await mediator.Send(new SaveOperationalContractCommand(null, request), cancellationToken));

    [HttpPut("{id:guid}/operational")]
    public async Task<IActionResult> UpdateOperational(Guid id, OperationalContractRequest request, CancellationToken cancellationToken) => Ok(await mediator.Send(new SaveOperationalContractCommand(id, request), cancellationToken));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TechnologyTransferContractDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new ListNitContractsQuery(), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<TechnologyTransferContractDto>> Create(
        CreateTechnologyTransferContractRequest request,
        CancellationToken cancellationToken)
    {
        var contract = await mediator.Send(
            new CreateNitContractCommand(
                request.InventionId,
                request.CompanyName,
                request.Cnpj,
                request.RoyaltyModel,
                request.RoyaltyValue,
                request.MinimumGuarantee,
                request.SignedAt,
                request.Status),
            cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = contract.Id }, contract);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TechnologyTransferContractDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetNitContractQuery(id), cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TechnologyTransferContractDto>> Update(
        Guid id,
        UpdateTechnologyTransferContractRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(
            new UpdateNitContractCommand(
                id,
                request.CompanyName,
                request.Cnpj,
                request.RoyaltyModel,
                request.RoyaltyValue,
                request.MinimumGuarantee,
                request.SignedAt,
                request.Status),
            cancellationToken));
    }
}
