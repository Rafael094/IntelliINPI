using FluentValidation;
using IntelliINPI.Application.Abstractions;
using IntelliINPI.Application.Common.Exceptions;
using IntelliINPI.Application.Operational;
using IntelliINPI.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Application.Nit;

public sealed record UniversityDto(
    Guid Id,
    string Name,
    string? Cnpj,
    string Tier,
    DateTime CreatedAtUtc,
    bool IsActive);

public sealed record UniversityRequest(string Name, string? Cnpj, string Tier);

public sealed record ListNitUniversitiesQuery : IRequest<IReadOnlyList<UniversityDto>>;
public sealed record GetNitUniversityQuery(Guid Id) : IRequest<UniversityDto>;
public sealed record CreateNitUniversityCommand(string Name, string? Cnpj, string Tier) : IRequest<UniversityDto>;
public sealed record UpdateNitUniversityCommand(Guid Id, string Name, string? Cnpj, string Tier) : IRequest<UniversityDto>;
public sealed record DeleteNitUniversityCommand(Guid Id) : IRequest;

public sealed class UniversityRequestValidator : AbstractValidator<UniversityRequest>
{
    public UniversityRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Cnpj).MaximumLength(20);
        RuleFor(x => x.Tier).NotEmpty().MaximumLength(40);
    }
}

public sealed class CreateNitUniversityCommandValidator : AbstractValidator<CreateNitUniversityCommand>
{
    public CreateNitUniversityCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Cnpj).MaximumLength(20);
        RuleFor(x => x.Tier).NotEmpty().MaximumLength(40);
    }
}

public sealed class UpdateNitUniversityCommandValidator : AbstractValidator<UpdateNitUniversityCommand>
{
    public UpdateNitUniversityCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Cnpj).MaximumLength(20);
        RuleFor(x => x.Tier).NotEmpty().MaximumLength(40);
    }
}

public sealed class ListNitUniversitiesQueryHandler(IApplicationDbContext dbContext) : IRequestHandler<ListNitUniversitiesQuery, IReadOnlyList<UniversityDto>>
{
    public async Task<IReadOnlyList<UniversityDto>> Handle(ListNitUniversitiesQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Universities
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new UniversityDto(x.Id, x.Name, x.Cnpj, x.Tier, x.CreatedAtUtc, x.IsActive))
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetNitUniversityQueryHandler(IApplicationDbContext dbContext) : IRequestHandler<GetNitUniversityQuery, UniversityDto>
{
    public async Task<UniversityDto> Handle(GetNitUniversityQuery request, CancellationToken cancellationToken)
    {
        var university = await dbContext.Universities
            .AsNoTracking()
            .Where(x => x.Id == request.Id && x.IsActive)
            .Select(x => new UniversityDto(x.Id, x.Name, x.Cnpj, x.Tier, x.CreatedAtUtc, x.IsActive))
            .SingleOrDefaultAsync(cancellationToken);

        return university ?? throw new NotFoundException("Universidade nao encontrada.");
    }
}

public sealed class CreateNitUniversityCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : IRequestHandler<CreateNitUniversityCommand, UniversityDto>
{
    public async Task<UniversityDto> Handle(CreateNitUniversityCommand request, CancellationToken cancellationToken)
    {
        var university = new University
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Cnpj = TrimToNull(request.Cnpj),
            Tier = request.Tier.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            IsActive = true
        };

        dbContext.Universities.Add(university);
        dbContext.AuditLogs.Add(AuditLogHelper.Create(currentUser, "NIT", "University", university.Id, "Created", university.Id));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new UniversityDto(university.Id, university.Name, university.Cnpj, university.Tier, university.CreatedAtUtc, university.IsActive);
    }

    private static string? TrimToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class UpdateNitUniversityCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : IRequestHandler<UpdateNitUniversityCommand, UniversityDto>
{
    public async Task<UniversityDto> Handle(UpdateNitUniversityCommand request, CancellationToken cancellationToken)
    {
        var university = await dbContext.Universities.SingleOrDefaultAsync(x => x.Id == request.Id && x.IsActive, cancellationToken)
            ?? throw new NotFoundException("Universidade nao encontrada.");

        university.Name = request.Name.Trim();
        university.Cnpj = TrimToNull(request.Cnpj);
        university.Tier = request.Tier.Trim();
        dbContext.AuditLogs.Add(AuditLogHelper.Create(currentUser, "NIT", "University", university.Id, "Updated", university.Id));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new UniversityDto(university.Id, university.Name, university.Cnpj, university.Tier, university.CreatedAtUtc, university.IsActive);
    }

    private static string? TrimToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class DeleteNitUniversityCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : IRequestHandler<DeleteNitUniversityCommand>
{
    public async Task Handle(DeleteNitUniversityCommand request, CancellationToken cancellationToken)
    {
        var university = await dbContext.Universities.SingleOrDefaultAsync(x => x.Id == request.Id && x.IsActive, cancellationToken)
            ?? throw new NotFoundException("Universidade nao encontrada.");

        university.IsActive = false;
        dbContext.AuditLogs.Add(AuditLogHelper.Create(currentUser, "NIT", "University", university.Id, "Deleted", university.Id));
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
