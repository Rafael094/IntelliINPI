using FluentValidation;
using IntelliINPI.Application.Abstractions;
using IntelliINPI.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Application.Auth;

public sealed record LoginRequest(string Email, string? Password = null, string? Senha = null)
{
    public string EffectivePassword => Password ?? Senha ?? string.Empty;
}
public sealed record LoginResponse(string AccessToken, UserDto User);
public sealed record UserDto(Guid Id, string Email, string Role);
public sealed record LoginCommand(string Email, string Password) : IRequest<LoginResponse>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class LoginCommandHandler(
    IApplicationDbContext dbContext,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : IRequestHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.ToLowerInvariant();
        var user = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Email == email, cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAppException("Credenciais inválidas.");
        }

        return new LoginResponse(jwtTokenService.CreateToken(user), new UserDto(user.Id, user.Email, user.Role));
    }
}
