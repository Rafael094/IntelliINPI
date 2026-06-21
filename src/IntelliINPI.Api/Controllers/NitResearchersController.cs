using IntelliINPI.Application.Nit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliINPI.Api.Controllers;

[ApiController, Authorize, Route("api/nit/researchers")]
public sealed class NitResearchersController(IMediator mediator) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> List([FromQuery] string? search, [FromQuery] Guid? institutionId, [FromQuery] string? technologyArea, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default) => Ok(await mediator.Send(new ListResearchersQuery(search, institutionId, technologyArea, page, pageSize), ct));
    [HttpGet("{id:guid}")] public async Task<IActionResult> Get(Guid id, CancellationToken ct) => Ok(await mediator.Send(new GetResearcherQuery(id), ct));
    [HttpPost] public async Task<IActionResult> Create(ResearcherRequest request, CancellationToken ct) { var item = await mediator.Send(new SaveResearcherCommand(null, request), ct); return CreatedAtAction(nameof(Get), new { id = item.Id }, item); }
    [HttpPut("{id:guid}")] public async Task<IActionResult> Update(Guid id, ResearcherRequest request, CancellationToken ct) => Ok(await mediator.Send(new SaveResearcherCommand(id, request), ct));
    [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id, CancellationToken ct) { await mediator.Send(new DeleteResearcherCommand(id), ct); return NoContent(); }
}
