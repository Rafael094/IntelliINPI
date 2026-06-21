using IntelliINPI.Application.Nit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliINPI.Api.Controllers;

[ApiController, Authorize, Route("api/nit/companies")]
public sealed class NitCompaniesController(IMediator mediator) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> List([FromQuery] string? search, CancellationToken ct) => Ok(await mediator.Send(new ListCompaniesQuery(search), ct));
    [HttpGet("{id:guid}")] public async Task<IActionResult> Get(Guid id, CancellationToken ct) => Ok(await mediator.Send(new GetCompanyQuery(id), ct));
    [HttpPost] public async Task<IActionResult> Create(CompanyRequest request, CancellationToken ct) { var item = await mediator.Send(new SaveCompanyCommand(null, request), ct); return CreatedAtAction(nameof(Get), new { id = item.Id }, item); }
    [HttpPut("{id:guid}")] public async Task<IActionResult> Update(Guid id, CompanyRequest request, CancellationToken ct) => Ok(await mediator.Send(new SaveCompanyCommand(id, request), ct));
    [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id, CancellationToken ct) { await mediator.Send(new DeleteCompanyCommand(id), ct); return NoContent(); }
}
