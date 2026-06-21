using FluentValidation;
using IntelliINPI.Application.Abstractions;
using IntelliINPI.Application.Common.Exceptions;
using IntelliINPI.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IntelliINPI.Application.Nit;

public sealed record TechnologyTransferContractDto(
    Guid Id,
    Guid InventionId,
    string InventionTitle,
    Guid UniversityId,
    string CompanyName,
    string? Cnpj,
    string RoyaltyModel,
    decimal? RoyaltyValue,
    decimal? MinimumGuarantee,
    DateOnly? SignedAt,
    string Status,
    DateTime CreatedAtUtc);

public sealed record CreateTechnologyTransferContractRequest(
    Guid InventionId,
    string CompanyName,
    string? Cnpj,
    string RoyaltyModel,
    decimal? RoyaltyValue,
    decimal? MinimumGuarantee,
    DateOnly? SignedAt,
    string Status);

public sealed record UpdateTechnologyTransferContractRequest(
    string CompanyName,
    string? Cnpj,
    string RoyaltyModel,
    decimal? RoyaltyValue,
    decimal? MinimumGuarantee,
    DateOnly? SignedAt,
    string Status);

public sealed record ListNitContractsQuery : IRequest<IReadOnlyList<TechnologyTransferContractDto>>;
public sealed record GetNitContractQuery(Guid Id) : IRequest<TechnologyTransferContractDto>;
public sealed record CreateNitContractCommand(
    Guid InventionId,
    string CompanyName,
    string? Cnpj,
    string RoyaltyModel,
    decimal? RoyaltyValue,
    decimal? MinimumGuarantee,
    DateOnly? SignedAt,
    string Status) : IRequest<TechnologyTransferContractDto>;

public sealed record UpdateNitContractCommand(
    Guid Id,
    string CompanyName,
    string? Cnpj,
    string RoyaltyModel,
    decimal? RoyaltyValue,
    decimal? MinimumGuarantee,
    DateOnly? SignedAt,
    string Status) : IRequest<TechnologyTransferContractDto>;

public static class NitRoyaltyModels
{
    public static readonly string[] Allowed =
    [
        "FixedPercentage",
        "MinimumGuarantee",
        "EquityParticipation",
        "Hybrid"
    ];

    public static bool IsAllowed(string? model)
    {
        return Allowed.Contains(model, StringComparer.OrdinalIgnoreCase);
    }

    public static string Normalize(string model)
    {
        return Allowed.First(x => string.Equals(x, model, StringComparison.OrdinalIgnoreCase));
    }
}

public sealed class CreateNitContractCommandValidator : AbstractValidator<CreateNitContractCommand>
{
    public CreateNitContractCommandValidator()
    {
        RuleFor(x => x.InventionId).NotEmpty();
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Cnpj).MaximumLength(20);
        RuleFor(x => x.RoyaltyModel).NotEmpty().Must(NitRoyaltyModels.IsAllowed).WithMessage("RoyaltyModel invalido.");
        RuleFor(x => x.RoyaltyValue).GreaterThanOrEqualTo(0).When(x => x.RoyaltyValue.HasValue);
        RuleFor(x => x.MinimumGuarantee).GreaterThanOrEqualTo(0).When(x => x.MinimumGuarantee.HasValue);
        RuleFor(x => x.Status).NotEmpty().MaximumLength(40);
    }
}

public sealed class UpdateNitContractCommandValidator : AbstractValidator<UpdateNitContractCommand>
{
    public UpdateNitContractCommandValidator()
    {
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Cnpj).MaximumLength(20);
        RuleFor(x => x.RoyaltyModel).NotEmpty().Must(NitRoyaltyModels.IsAllowed).WithMessage("RoyaltyModel invalido.");
        RuleFor(x => x.RoyaltyValue).GreaterThanOrEqualTo(0).When(x => x.RoyaltyValue.HasValue);
        RuleFor(x => x.MinimumGuarantee).GreaterThanOrEqualTo(0).When(x => x.MinimumGuarantee.HasValue);
        RuleFor(x => x.Status).NotEmpty().MaximumLength(40);
    }
}

public sealed class ListNitContractsQueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<ListNitContractsQuery, IReadOnlyList<TechnologyTransferContractDto>>
{
    public async Task<IReadOnlyList<TechnologyTransferContractDto>> Handle(ListNitContractsQuery request, CancellationToken cancellationToken)
    {
        var access = await NitAccessContext.LoadAsync(dbContext, currentUser.UserId, cancellationToken);
        var query = dbContext.TechnologyTransferContracts.AsNoTracking()
            .Where(x => x.Invention.IsActive);

        if (!access.IsGlobalAdmin)
        {
            query = query.Where(x => access.UniversityIds.Contains(x.Invention.UniversityId));
        }

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(NitContractProjection.Dto)
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetNitContractQueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<GetNitContractQuery, TechnologyTransferContractDto>
{
    public async Task<TechnologyTransferContractDto> Handle(GetNitContractQuery request, CancellationToken cancellationToken)
    {
        var access = await NitAccessContext.LoadAsync(dbContext, currentUser.UserId, cancellationToken);
        var query = dbContext.TechnologyTransferContracts.AsNoTracking().Where(x => x.Id == request.Id && x.Invention.IsActive);

        if (!access.IsGlobalAdmin)
        {
            query = query.Where(x => access.UniversityIds.Contains(x.Invention.UniversityId));
        }

        var contract = await query.Select(NitContractProjection.Dto).SingleOrDefaultAsync(cancellationToken);
        return contract ?? throw new NotFoundException("Contrato nao encontrado.");
    }
}

public sealed class CreateNitContractCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<CreateNitContractCommand, TechnologyTransferContractDto>
{
    public async Task<TechnologyTransferContractDto> Handle(CreateNitContractCommand request, CancellationToken cancellationToken)
    {
        var access = await NitAccessContext.LoadAsync(dbContext, currentUser.UserId, cancellationToken);
        var invention = await dbContext.Inventions.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == request.InventionId && x.IsActive, cancellationToken)
            ?? throw new NotFoundException("Invencao nao encontrada.");

        if (!access.CanAccess(invention.UniversityId))
        {
            throw new NotFoundException("Invencao nao encontrada.");
        }

        var contract = new TechnologyTransferContract
        {
            Id = Guid.NewGuid(),
            InventionId = invention.Id,
            CompanyName = request.CompanyName.Trim(),
            Cnpj = NitInventionHelpers.TrimToNull(request.Cnpj),
            RoyaltyModel = NitRoyaltyModels.Normalize(request.RoyaltyModel),
            RoyaltyValue = request.RoyaltyValue,
            MinimumGuarantee = request.MinimumGuarantee,
            SignedAt = request.SignedAt,
            Status = request.Status.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.TechnologyTransferContracts.Add(contract);
        dbContext.AuditLogs.Add(NitAuditLogFactory.Create(currentUser, invention.UniversityId, "TechnologyTransferContract", contract.Id, "Created"));
        await dbContext.SaveChangesAsync(cancellationToken);

        return await LoadDtoAsync(contract.Id, cancellationToken);
    }

    private async Task<TechnologyTransferContractDto> LoadDtoAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.TechnologyTransferContracts
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(NitContractProjection.Dto)
            .SingleAsync(cancellationToken);
    }
}

public sealed class UpdateNitContractCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<UpdateNitContractCommand, TechnologyTransferContractDto>
{
    public async Task<TechnologyTransferContractDto> Handle(UpdateNitContractCommand request, CancellationToken cancellationToken)
    {
        var access = await NitAccessContext.LoadAsync(dbContext, currentUser.UserId, cancellationToken);
        var contract = await dbContext.TechnologyTransferContracts
            .Include(x => x.Invention)
            .SingleOrDefaultAsync(x => x.Id == request.Id && x.Invention.IsActive, cancellationToken)
            ?? throw new NotFoundException("Contrato nao encontrado.");

        if (!access.CanAccess(contract.Invention.UniversityId))
        {
            throw new NotFoundException("Contrato nao encontrado.");
        }

        contract.CompanyName = request.CompanyName.Trim();
        contract.Cnpj = NitInventionHelpers.TrimToNull(request.Cnpj);
        contract.RoyaltyModel = NitRoyaltyModels.Normalize(request.RoyaltyModel);
        contract.RoyaltyValue = request.RoyaltyValue;
        contract.MinimumGuarantee = request.MinimumGuarantee;
        contract.SignedAt = request.SignedAt;
        contract.Status = request.Status.Trim();

        dbContext.AuditLogs.Add(NitAuditLogFactory.Create(currentUser, contract.Invention.UniversityId, "TechnologyTransferContract", contract.Id, "Updated"));
        await dbContext.SaveChangesAsync(cancellationToken);

        return await dbContext.TechnologyTransferContracts
            .AsNoTracking()
            .Where(x => x.Id == contract.Id)
            .Select(NitContractProjection.Dto)
            .SingleAsync(cancellationToken);
    }
}

internal static class NitContractProjection
{
    public static readonly Expression<Func<TechnologyTransferContract, TechnologyTransferContractDto>> Dto = contract =>
        new TechnologyTransferContractDto(
            contract.Id,
            contract.InventionId,
            contract.Invention.Title,
            contract.Invention.UniversityId,
            contract.CompanyName,
            contract.Cnpj,
            contract.RoyaltyModel,
            contract.RoyaltyValue,
            contract.MinimumGuarantee,
            contract.SignedAt,
            contract.Status,
            contract.CreatedAtUtc);
}
