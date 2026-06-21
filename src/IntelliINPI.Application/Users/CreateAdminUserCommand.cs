using FluentValidation;
using IntelliINPI.Application.Abstractions;
using IntelliINPI.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Application.Users;

public sealed record CreateAdminUserRequest(string Email, string Password);
public sealed record CreateAdminUserResponse(Guid Id, string Email);
public sealed record CreateAdminUserCommand(string Email, string Password) : IRequest<CreateAdminUserResponse>;

public sealed class CreateAdminUserCommandValidator : AbstractValidator<CreateAdminUserCommand>
{
    public CreateAdminUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).MinimumLength(8);
    }
}

public sealed class CreateAdminUserCommandHandler(IApplicationDbContext dbContext, IPasswordHasher passwordHasher)
    : IRequestHandler<CreateAdminUserCommand, CreateAdminUserResponse>
{
    public async Task<CreateAdminUserResponse> Handle(CreateAdminUserCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.ToLowerInvariant();
        var existingUser = await dbContext.Users.SingleOrDefaultAsync(x => x.Email == email, cancellationToken);

        if (existingUser is not null)
        {
            return new CreateAdminUserResponse(existingUser.Id, existingUser.Email);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password),
            Role = "Admin",
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new CreateAdminUserResponse(user.Id, user.Email);
    }
}
