using IntelliINPI.Application.Nit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliINPI.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/nit/dashboard")]
public sealed class NitDashboardController(IMediator mediator) : ControllerBase
{
    [HttpGet("overview")]
    public async Task<ActionResult<NitDashboardOverviewDto>> Overview(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetNitDashboardOverviewQuery(), cancellationToken));
    }
}
