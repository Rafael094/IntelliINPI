using IntelliINPI.Application.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliINPI.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public sealed class DashboardController(IMediator mediator) : ControllerBase
{
    [HttpGet("operational")]
    public async Task<ActionResult<OperationalDashboardDto>> Operational(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetOperationalDashboardQuery(), cancellationToken));
    }
}
