using IntelliINPI.Application.Nit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliINPI.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/nit/audit-logs")]
public sealed class NitAuditLogsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NitAuditLogDto>>> List([FromQuery] string? entityName, [FromQuery] string? action, [FromQuery] Guid? userId, [FromQuery] DateTime? startAtUtc, [FromQuery] DateTime? endAtUtc, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new ListNitAuditLogsQuery(entityName, action, userId, startAtUtc, endAtUtc), cancellationToken));
    }
}
