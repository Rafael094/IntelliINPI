using IntelliINPI.Application.Nit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliINPI.Api.Controllers;

[ApiController, Authorize, Route("api/nit/transfer-pipeline")]
public sealed class NitTransferPipelineController(IMediator mediator) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> List(CancellationToken ct) => Ok(await mediator.Send(new ListTransferPipelineQuery(), ct));
    [HttpPost] public async Task<IActionResult> Create(TransferOpportunityRequest request, CancellationToken ct) => Ok(await mediator.Send(new SaveTransferOpportunityCommand(null, request), ct));
    [HttpPut("{id:guid}")] public async Task<IActionResult> Update(Guid id, TransferOpportunityRequest request, CancellationToken ct) => Ok(await mediator.Send(new SaveTransferOpportunityCommand(id, request), ct));
    [HttpPatch("{id:guid}/stage")] public async Task<IActionResult> Move(Guid id, MoveTransferStageRequest request, CancellationToken ct) => Ok(await mediator.Send(new MoveTransferOpportunityCommand(id, request.Stage, request.SortOrder), ct));
    [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id, CancellationToken ct) { await mediator.Send(new DeleteTransferOpportunityCommand(id), ct); return NoContent(); }
}

public sealed record MoveTransferStageRequest(string Stage, int SortOrder);
