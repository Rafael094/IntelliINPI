using FluentValidation;
using IntelliINPI.Application.Abstractions;
using IntelliINPI.Application.Common.Exceptions;
using IntelliINPI.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Application.Operational;

public sealed record ClientDto(
    Guid Id,
    string Name,
    string? DocumentNumber,
    string? Email,
    string? Phone,
    string? Notes,
    DateTime CreatedAtUtc,
    bool IsActive);

public sealed record ClientRequest(
    string Name,
    string? DocumentNumber,
    string? Email,
    string? Phone,
    string? Notes);

public sealed record ListClientsQuery : IRequest<IReadOnlyList<ClientDto>>;
public sealed record GetClientQuery(Guid Id) : IRequest<ClientDto>;
public sealed record CreateClientCommand(string Name, string? DocumentNumber, string? Email, string? Phone, string? Notes) : IRequest<ClientDto>;
public sealed record UpdateClientCommand(Guid Id, string Name, string? DocumentNumber, string? Email, string? Phone, string? Notes) : IRequest<ClientDto>;
public sealed record DeleteClientCommand(Guid Id) : IRequest;

public sealed class ClientRequestValidator : AbstractValidator<ClientRequest>
{
    public ClientRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DocumentNumber).MaximumLength(40);
        RuleFor(x => x.Email).MaximumLength(160).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(40);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

public sealed class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DocumentNumber).MaximumLength(40);
        RuleFor(x => x.Email).MaximumLength(160).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(40);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

public sealed class UpdateClientCommandValidator : AbstractValidator<UpdateClientCommand>
{
    public UpdateClientCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DocumentNumber).MaximumLength(40);
        RuleFor(x => x.Email).MaximumLength(160).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(40);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

public sealed class ListClientsQueryHandler(IApplicationDbContext dbContext) : IRequestHandler<ListClientsQuery, IReadOnlyList<ClientDto>>
{
    public async Task<IReadOnlyList<ClientDto>> Handle(ListClientsQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Clients
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new ClientDto(x.Id, x.Name, x.DocumentNumber, x.Email, x.Phone, x.Notes, x.CreatedAtUtc, x.IsActive))
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetClientQueryHandler(IApplicationDbContext dbContext) : IRequestHandler<GetClientQuery, ClientDto>
{
    public async Task<ClientDto> Handle(GetClientQuery request, CancellationToken cancellationToken)
    {
        var client = await dbContext.Clients
            .AsNoTracking()
            .Where(x => x.Id == request.Id && x.IsActive)
            .Select(x => new ClientDto(x.Id, x.Name, x.DocumentNumber, x.Email, x.Phone, x.Notes, x.CreatedAtUtc, x.IsActive))
            .SingleOrDefaultAsync(cancellationToken);

        return client ?? throw new NotFoundException("Cliente nao encontrado.");
    }
}

public sealed class CreateClientCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : IRequestHandler<CreateClientCommand, ClientDto>
{
    public async Task<ClientDto> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            DocumentNumber = TrimToNull(request.DocumentNumber),
            Email = TrimToNull(request.Email),
            Phone = TrimToNull(request.Phone),
            Notes = TrimToNull(request.Notes),
            CreatedAtUtc = DateTime.UtcNow,
            IsActive = true
        };

        dbContext.Clients.Add(client);
        dbContext.AuditLogs.Add(AuditLogHelper.Create(currentUser, "Operational", "Client", client.Id, "Created"));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ClientDto(client.Id, client.Name, client.DocumentNumber, client.Email, client.Phone, client.Notes, client.CreatedAtUtc, client.IsActive);
    }

    private static string? TrimToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class UpdateClientCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : IRequestHandler<UpdateClientCommand, ClientDto>
{
    public async Task<ClientDto> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        var client = await dbContext.Clients.SingleOrDefaultAsync(x => x.Id == request.Id && x.IsActive, cancellationToken)
            ?? throw new NotFoundException("Cliente nao encontrado.");

        client.Name = request.Name.Trim();
        client.DocumentNumber = TrimToNull(request.DocumentNumber);
        client.Email = TrimToNull(request.Email);
        client.Phone = TrimToNull(request.Phone);
        client.Notes = TrimToNull(request.Notes);
        dbContext.AuditLogs.Add(AuditLogHelper.Create(currentUser, "Operational", "Client", client.Id, "Updated"));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ClientDto(client.Id, client.Name, client.DocumentNumber, client.Email, client.Phone, client.Notes, client.CreatedAtUtc, client.IsActive);
    }

    private static string? TrimToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class DeleteClientCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : IRequestHandler<DeleteClientCommand>
{
    public async Task Handle(DeleteClientCommand request, CancellationToken cancellationToken)
    {
        var client = await dbContext.Clients.SingleOrDefaultAsync(x => x.Id == request.Id && x.IsActive, cancellationToken)
            ?? throw new NotFoundException("Cliente nao encontrado.");

        client.IsActive = false;
        dbContext.AuditLogs.Add(AuditLogHelper.Create(currentUser, "Operational", "Client", client.Id, "Deleted"));
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
