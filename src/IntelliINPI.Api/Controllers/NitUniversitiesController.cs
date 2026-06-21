using IntelliINPI.Application.Nit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliINPI.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/nit/universities")]
public sealed class NitUniversitiesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UniversityDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new ListNitUniversitiesQuery(), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<UniversityDto>> Create(UniversityRequest request, CancellationToken cancellationToken)
    {
        var university = await mediator.Send(new CreateNitUniversityCommand(request.Name, request.Cnpj, request.Tier), cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = university.Id }, university);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UniversityDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetNitUniversityQuery(id), cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UniversityDto>> Update(Guid id, UniversityRequest request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new UpdateNitUniversityCommand(id, request.Name, request.Cnpj, request.Tier), cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteNitUniversityCommand(id), cancellationToken);
        return NoContent();
    }
}
