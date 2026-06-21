using IntelliINPI.Application.Nit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliINPI.Api.Controllers;

[ApiController, Authorize, Route("api/nit/institutions")]
public sealed class NitInstitutionsController(IMediator mediator) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> List(CancellationToken ct) => Ok(await mediator.Send(new ListInstitutionsQuery(), ct));
    [HttpGet("{id:guid}")] public async Task<IActionResult> Get(Guid id, CancellationToken ct) => Ok(await mediator.Send(new GetInstitutionQuery(id), ct));
    [HttpPost] public async Task<IActionResult> Create(InstitutionRequest request, CancellationToken ct) { var item = await mediator.Send(new SaveInstitutionCommand(null, request), ct); return CreatedAtAction(nameof(Get), new { id = item.Id }, item); }
    [HttpPut("{id:guid}")] public async Task<IActionResult> Update(Guid id, InstitutionRequest request, CancellationToken ct) => Ok(await mediator.Send(new SaveInstitutionCommand(id, request), ct));
    [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id, CancellationToken ct) { await mediator.Send(new DeleteInstitutionCommand(id), ct); return NoContent(); }
}
