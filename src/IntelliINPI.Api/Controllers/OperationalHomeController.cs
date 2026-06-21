using IntelliINPI.Application.Operational;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliINPI.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/operational-home")]
public sealed class OperationalHomeController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<OperationalHomeDto>> Get(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetOperationalHomeQuery(), cancellationToken));
    }
}
