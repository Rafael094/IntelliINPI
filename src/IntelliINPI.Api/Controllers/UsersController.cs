using IntelliINPI.Application.Users;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IntelliINPI.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController(IMediator mediator) : ControllerBase
{
    [HttpPost("admin")]
    public async Task<ActionResult<CreateAdminUserResponse>> CreateAdmin(CreateAdminUserRequest request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new CreateAdminUserCommand(request.Email, request.Password), cancellationToken));
    }
}
