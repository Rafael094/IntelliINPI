using IntelliINPI.Application.Operational;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliINPI.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/clients")]
public sealed class ClientsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClientDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new ListClientsQuery(), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<ClientDto>> Create(ClientRequest request, CancellationToken cancellationToken)
    {
        var client = await mediator.Send(new CreateClientCommand(request.Name, request.DocumentNumber, request.Email, request.Phone, request.Notes), cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = client.Id }, client);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClientDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetClientQuery(id), cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ClientDto>> Update(Guid id, ClientRequest request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new UpdateClientCommand(id, request.Name, request.DocumentNumber, request.Email, request.Phone, request.Notes), cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteClientCommand(id), cancellationToken);
        return NoContent();
    }
}
