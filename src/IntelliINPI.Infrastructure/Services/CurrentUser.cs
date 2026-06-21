using System.Security.Claims;
using IntelliINPI.Application.Abstractions;
using IntelliINPI.Application.Common.Exceptions;
using Microsoft.AspNetCore.Http;

namespace IntelliINPI.Infrastructure.Services;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public string? IpAddress => httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public Guid UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? httpContextAccessor.HttpContext?.User.FindFirstValue("sub");

            return Guid.TryParse(value, out var userId)
                ? userId
                : throw new UnauthorizedAppException("Usuário autenticado não identificado.");
        }
    }
}
