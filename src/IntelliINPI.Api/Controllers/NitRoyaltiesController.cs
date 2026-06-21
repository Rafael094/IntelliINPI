using IntelliINPI.Application.Nit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliINPI.Api.Controllers;

[ApiController, Authorize, Route("api/nit/royalties")]
public sealed class NitRoyaltiesController(IMediator mediator) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> List(CancellationToken ct) => Ok(await mediator.Send(new ListRoyaltiesQuery(), ct));
    [HttpGet("summary")] public async Task<IActionResult> Summary(CancellationToken ct) => Ok(await mediator.Send(new GetRoyaltySummaryQuery(), ct));
    [HttpPost] public async Task<IActionResult> Create(RoyaltyRequest request, CancellationToken ct) => Ok(await mediator.Send(new SaveRoyaltyCommand(null, request), ct));
    [HttpPut("{id:guid}")] public async Task<IActionResult> Update(Guid id, RoyaltyRequest request, CancellationToken ct) => Ok(await mediator.Send(new SaveRoyaltyCommand(id, request), ct));
    [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id, CancellationToken ct) { await mediator.Send(new DeleteRoyaltyCommand(id), ct); return NoContent(); }
}
