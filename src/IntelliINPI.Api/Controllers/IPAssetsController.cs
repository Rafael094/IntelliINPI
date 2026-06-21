using IntelliINPI.Application.IPAssets;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliINPI.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/ip-assets")]
public sealed class IPAssetsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<IPAssetDto>>> List([FromQuery] string? type, [FromQuery] string? query, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new ListIPAssetsQuery(type, query), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<IPAssetDto>> Create(IPAssetRequest request, CancellationToken cancellationToken)
    {
        var asset = await mediator.Send(
            new CreateIPAssetCommand(
                request.Type,
                request.InpiProcessNumber,
                request.Title,
                request.OwnerName,
                request.Status,
                request.FilingDate,
                request.GrantDate,
                request.ExpirationDate,
                request.InternalDeadline,
                request.ClientId,
                request.UniversityId,
                request.IsMonitored),
            cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = asset.Id }, asset);
    }

    [HttpPost("register-and-monitor")]
    public async Task<ActionResult<RegisterAndMonitorIPAssetResult>> RegisterAndMonitor(RegisterAndMonitorIPAssetRequest request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new RegisterAndMonitorIPAssetCommand(request.Type, request.Query, request.ClientId, request.UniversityId), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<IPAssetDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetIPAssetQuery(id), cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<IPAssetDto>> Update(Guid id, IPAssetRequest request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(
            new UpdateIPAssetCommand(
                id,
                request.Type,
                request.InpiProcessNumber,
                request.Title,
                request.OwnerName,
                request.Status,
                request.FilingDate,
                request.GrantDate,
                request.ExpirationDate,
                request.InternalDeadline,
                request.ClientId,
                request.UniversityId,
                request.IsMonitored),
            cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteIPAssetCommand(id), cancellationToken);
        return NoContent();
    }
}
