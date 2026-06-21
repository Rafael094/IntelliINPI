using FluentValidation;
using IntelliINPI.Application.Abstractions;
using IntelliINPI.Application.Common.Exceptions;
using IntelliINPI.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Application.Nit;

public sealed record InventionDto(
    Guid Id,
    Guid UniversityId,
    string UniversityName,
    string Title,
    string Summary,
    string Inventors,
    DateOnly? DepositDate,
    string Status,
    string? PatentNumber,
    string? InpiProcessNumber,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    bool IsActive);

public sealed record CreateInventionRequest(
    Guid? UniversityId,
    string Title,
    string Summary,
    string Inventors,
    DateOnly? DepositDate,
    string? Status,
    string? PatentNumber,
    string? InpiProcessNumber);

public sealed record UpdateInventionRequest(
    Guid? UniversityId,
    string Title,
    string Summary,
    string Inventors,
    DateOnly? DepositDate,
    string Status,
    string? PatentNumber,
    string? InpiProcessNumber);

public sealed record ListNitInventionsQuery : IRequest<IReadOnlyList<InventionDto>>;
public sealed record GetNitInventionQuery(Guid Id) : IRequest<InventionDto>;
public sealed record CreateNitInventionCommand(
    Guid? UniversityId,
    string Title,
    string Summary,
    string Inventors,
    DateOnly? DepositDate,
    string? Status,
    string? PatentNumber,
    string? InpiProcessNumber) : IRequest<InventionDto>;

public sealed record UpdateNitInventionCommand(
    Guid Id,
    Guid? UniversityId,
    string Title,
    string Summary,
    string Inventors,
    DateOnly? DepositDate,
    string Status,
    string? PatentNumber,
    string? InpiProcessNumber) : IRequest<InventionDto>;

public sealed record DeleteNitInventionCommand(Guid Id) : IRequest;

public static class NitInventionStatuses
{
    public static readonly string[] Allowed =
    [
        "Draft",
        "SubmittedToNit",
        "UnderReview",
        "FiledAtInpi",
        "Granted",
        "Licensed",
        "Archived"
    ];

    public static bool IsAllowed(string? status)
    {
        return Allowed.Contains(status, StringComparer.OrdinalIgnoreCase);
    }

    public static string Normalize(string? status)
    {
        return Allowed.FirstOrDefault(x => string.Equals(x, status, StringComparison.OrdinalIgnoreCase)) ?? "Draft";
    }
}

public sealed class CreateNitInventionCommandValidator : AbstractValidator<CreateNitInventionCommand>
{
    public CreateNitInventionCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(240);
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Inventors).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Status)
            .Must(status => string.IsNullOrWhiteSpace(status) || NitInventionStatuses.IsAllowed(status))
            .WithMessage("Status invalido para invencao.");
        RuleFor(x => x.PatentNumber).MaximumLength(80);
        RuleFor(x => x.InpiProcessNumber).MaximumLength(80);
    }
}

public sealed class UpdateNitInventionCommandValidator : AbstractValidator<UpdateNitInventionCommand>
{
    public UpdateNitInventionCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(240);
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Inventors).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Status).Must(NitInventionStatuses.IsAllowed).WithMessage("Status invalido para invencao.");
        RuleFor(x => x.PatentNumber).MaximumLength(80);
        RuleFor(x => x.InpiProcessNumber).MaximumLength(80);
    }
}

public sealed class ListNitInventionsQueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<ListNitInventionsQuery, IReadOnlyList<InventionDto>>
{
    public async Task<IReadOnlyList<InventionDto>> Handle(ListNitInventionsQuery request, CancellationToken cancellationToken)
    {
        var access = await NitAccessContext.LoadAsync(dbContext, currentUser.UserId, cancellationToken);
        var query = dbContext.Inventions.AsNoTracking().Where(x => x.IsActive);

        if (!access.IsGlobalAdmin)
        {
            query = query.Where(x => access.UniversityIds.Contains(x.UniversityId));
        }

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new InventionDto(
                x.Id,
                x.UniversityId,
                x.University.Name,
                x.Title,
                x.Summary,
                x.Inventors,
                x.DepositDate,
                x.Status,
                x.PatentNumber,
                x.InpiProcessNumber,
                x.CreatedAtUtc,
                x.UpdatedAtUtc,
                x.IsActive))
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetNitInventionQueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<GetNitInventionQuery, InventionDto>
{
    public async Task<InventionDto> Handle(GetNitInventionQuery request, CancellationToken cancellationToken)
    {
        var access = await NitAccessContext.LoadAsync(dbContext, currentUser.UserId, cancellationToken);
        var query = dbContext.Inventions.AsNoTracking().Where(x => x.Id == request.Id && x.IsActive);

        if (!access.IsGlobalAdmin)
        {
            query = query.Where(x => access.UniversityIds.Contains(x.UniversityId));
        }

        var invention = await query
            .Select(x => new InventionDto(
                x.Id,
                x.UniversityId,
                x.University.Name,
                x.Title,
                x.Summary,
                x.Inventors,
                x.DepositDate,
                x.Status,
                x.PatentNumber,
                x.InpiProcessNumber,
                x.CreatedAtUtc,
                x.UpdatedAtUtc,
                x.IsActive))
            .SingleOrDefaultAsync(cancellationToken);

        return invention ?? throw new NotFoundException("Invencao nao encontrada.");
    }
}

public sealed class CreateNitInventionCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<CreateNitInventionCommand, InventionDto>
{
    public async Task<InventionDto> Handle(CreateNitInventionCommand request, CancellationToken cancellationToken)
    {
        var access = await NitAccessContext.LoadAsync(dbContext, currentUser.UserId, cancellationToken);
        var universityId = NitInventionHelpers.ResolveUniversityId(access, request.UniversityId);
        await EnsureUniversityExistsAsync(universityId, cancellationToken);

        var now = DateTime.UtcNow;
        var invention = new Invention
        {
            Id = Guid.NewGuid(),
            UniversityId = universityId,
            Title = request.Title.Trim(),
            Summary = request.Summary.Trim(),
            Inventors = request.Inventors.Trim(),
            DepositDate = request.DepositDate,
            Status = NitInventionStatuses.Normalize(request.Status),
            PatentNumber = NitInventionHelpers.TrimToNull(request.PatentNumber),
            InpiProcessNumber = NitInventionHelpers.TrimToNull(request.InpiProcessNumber),
            CreatedAtUtc = now,
            IsActive = true
        };

        dbContext.Inventions.Add(invention);
        AddAuditLog(invention.Id, universityId, "Created");
        await dbContext.SaveChangesAsync(cancellationToken);

        return await LoadDtoAsync(invention.Id, cancellationToken);
    }

    private async Task EnsureUniversityExistsAsync(Guid universityId, CancellationToken cancellationToken)
    {
        var exists = await dbContext.Universities.AnyAsync(x => x.Id == universityId && x.IsActive, cancellationToken);
        if (!exists)
        {
            throw new NotFoundException("Universidade nao encontrada.");
        }
    }

    private async Task<InventionDto> LoadDtoAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Inventions
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new InventionDto(
                x.Id,
                x.UniversityId,
                x.University.Name,
                x.Title,
                x.Summary,
                x.Inventors,
                x.DepositDate,
                x.Status,
                x.PatentNumber,
                x.InpiProcessNumber,
                x.CreatedAtUtc,
                x.UpdatedAtUtc,
                x.IsActive))
            .SingleAsync(cancellationToken);
    }

    private void AddAuditLog(Guid inventionId, Guid universityId, string action)
    {
        dbContext.AuditLogs.Add(NitAuditLogFactory.Create(currentUser, universityId, "Invention", inventionId, action));
    }
}

public sealed class UpdateNitInventionCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<UpdateNitInventionCommand, InventionDto>
{
    public async Task<InventionDto> Handle(UpdateNitInventionCommand request, CancellationToken cancellationToken)
    {
        var access = await NitAccessContext.LoadAsync(dbContext, currentUser.UserId, cancellationToken);
        var invention = await dbContext.Inventions.SingleOrDefaultAsync(x => x.Id == request.Id && x.IsActive, cancellationToken)
            ?? throw new NotFoundException("Invencao nao encontrada.");

        if (!access.CanAccess(invention.UniversityId))
        {
            throw new NotFoundException("Invencao nao encontrada.");
        }

        if (request.UniversityId.HasValue && request.UniversityId.Value != invention.UniversityId)
        {
            if (!access.IsGlobalAdmin)
            {
                throw new UnauthorizedAppException("Somente admin global pode alterar universidade da invencao.");
            }

            var universityExists = await dbContext.Universities.AnyAsync(x => x.Id == request.UniversityId.Value && x.IsActive, cancellationToken);
            if (!universityExists)
            {
                throw new NotFoundException("Universidade nao encontrada.");
            }

            invention.UniversityId = request.UniversityId.Value;
        }

        invention.Title = request.Title.Trim();
        invention.Summary = request.Summary.Trim();
        invention.Inventors = request.Inventors.Trim();
        invention.DepositDate = request.DepositDate;
        invention.Status = NitInventionStatuses.Normalize(request.Status);
        invention.PatentNumber = NitInventionHelpers.TrimToNull(request.PatentNumber);
        invention.InpiProcessNumber = NitInventionHelpers.TrimToNull(request.InpiProcessNumber);
        invention.UpdatedAtUtc = DateTime.UtcNow;

        dbContext.AuditLogs.Add(NitAuditLogFactory.Create(currentUser, invention.UniversityId, "Invention", invention.Id, "Updated"));
        await dbContext.SaveChangesAsync(cancellationToken);

        return await dbContext.Inventions
            .AsNoTracking()
            .Where(x => x.Id == invention.Id)
            .Select(x => new InventionDto(
                x.Id,
                x.UniversityId,
                x.University.Name,
                x.Title,
                x.Summary,
                x.Inventors,
                x.DepositDate,
                x.Status,
                x.PatentNumber,
                x.InpiProcessNumber,
                x.CreatedAtUtc,
                x.UpdatedAtUtc,
                x.IsActive))
            .SingleAsync(cancellationToken);
    }
}

public sealed class DeleteNitInventionCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<DeleteNitInventionCommand>
{
    public async Task Handle(DeleteNitInventionCommand request, CancellationToken cancellationToken)
    {
        var access = await NitAccessContext.LoadAsync(dbContext, currentUser.UserId, cancellationToken);
        var invention = await dbContext.Inventions.SingleOrDefaultAsync(x => x.Id == request.Id && x.IsActive, cancellationToken)
            ?? throw new NotFoundException("Invencao nao encontrada.");

        if (!access.CanAccess(invention.UniversityId))
        {
            throw new NotFoundException("Invencao nao encontrada.");
        }

        invention.IsActive = false;
        invention.UpdatedAtUtc = DateTime.UtcNow;
        dbContext.AuditLogs.Add(NitAuditLogFactory.Create(currentUser, invention.UniversityId, "Invention", invention.Id, "Deleted"));
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

internal sealed record NitAccessContext(bool IsGlobalAdmin, IReadOnlyList<Guid> UniversityIds)
{
    public bool CanAccess(Guid universityId)
    {
        return IsGlobalAdmin || UniversityIds.Contains(universityId);
    }

    public static async Task<NitAccessContext> LoadAsync(IApplicationDbContext dbContext, Guid userId, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new UnauthorizedAppException("Usuario autenticado nao encontrado.");

        if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return new NitAccessContext(true, []);
        }

        var universityIds = await dbContext.NitUserProfiles
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.UniversityId)
            .ToListAsync(cancellationToken);

        return new NitAccessContext(false, universityIds);
    }
}

internal static class NitAuditLogFactory
{
    public static AuditLog Create(ICurrentUser currentUser, Guid universityId, string entityName, Guid entityId, string action)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = currentUser.UserId,
            UniversityId = universityId,
            Module = "NIT",
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            IpAddress = currentUser.IpAddress,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}

internal static class NitInventionHelpers
{
    public static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public static Guid ResolveUniversityId(NitAccessContext access, Guid? requestedUniversityId)
    {
        if (access.IsGlobalAdmin)
        {
            return requestedUniversityId ?? throw new ValidationException("UniversityId e obrigatorio para admin global.");
        }

        if (requestedUniversityId.HasValue)
        {
            if (!access.UniversityIds.Contains(requestedUniversityId.Value))
            {
                throw new UnauthorizedAppException("Usuario nao possui acesso a universidade informada.");
            }

            return requestedUniversityId.Value;
        }

        if (access.UniversityIds.Count == 1)
        {
            return access.UniversityIds[0];
        }

        throw new UnauthorizedAppException("Usuario nao possui perfil NIT unico para definir a universidade.");
    }
}
