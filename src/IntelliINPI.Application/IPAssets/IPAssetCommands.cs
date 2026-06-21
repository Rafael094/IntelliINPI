using FluentValidation;
using IntelliINPI.Application.Abstractions;
using IntelliINPI.Application.Common.Exceptions;
using IntelliINPI.Application.Operational;
using IntelliINPI.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Application.IPAssets;

public sealed record IPAssetDto(
    Guid Id,
    string Type,
    string? InpiProcessNumber,
    string Title,
    string? OwnerName,
    string Status,
    DateOnly? FilingDate,
    DateOnly? GrantDate,
    DateOnly? ExpirationDate,
    DateOnly? InternalDeadline,
    Guid? ClientId,
    string? ClientName,
    Guid? UniversityId,
    string? UniversityName,
    bool IsMonitored,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    bool IsActive,
    string? Source,
    string? Warning);

public sealed record IPAssetRequest(
    string Type,
    string? InpiProcessNumber,
    string Title,
    string? OwnerName,
    string? Status,
    DateOnly? FilingDate,
    DateOnly? GrantDate,
    DateOnly? ExpirationDate,
    DateOnly? InternalDeadline,
    Guid? ClientId,
    Guid? UniversityId,
    bool IsMonitored);

public sealed record ListIPAssetsQuery(string? Type, string? Query) : IRequest<IReadOnlyList<IPAssetDto>>;
public sealed record GetIPAssetQuery(Guid Id) : IRequest<IPAssetDto>;
public sealed record CreateIPAssetCommand(
    string Type,
    string? InpiProcessNumber,
    string Title,
    string? OwnerName,
    string? Status,
    DateOnly? FilingDate,
    DateOnly? GrantDate,
    DateOnly? ExpirationDate,
    DateOnly? InternalDeadline,
    Guid? ClientId,
    Guid? UniversityId,
    bool IsMonitored) : IRequest<IPAssetDto>;

public sealed record UpdateIPAssetCommand(
    Guid Id,
    string Type,
    string? InpiProcessNumber,
    string Title,
    string? OwnerName,
    string? Status,
    DateOnly? FilingDate,
    DateOnly? GrantDate,
    DateOnly? ExpirationDate,
    DateOnly? InternalDeadline,
    Guid? ClientId,
    Guid? UniversityId,
    bool IsMonitored) : IRequest<IPAssetDto>;

public sealed record DeleteIPAssetCommand(Guid Id) : IRequest;

public sealed class IPAssetRequestValidator : AbstractValidator<IPAssetRequest>
{
    public IPAssetRequestValidator()
    {
        RuleFor(x => x.Type).Must(IPAssetHelpers.IsValidType).WithMessage("Tipo de ativo de PI invalido.");
        RuleFor(x => x.InpiProcessNumber).MaximumLength(80);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(240);
        RuleFor(x => x.OwnerName).MaximumLength(240);
        RuleFor(x => x.Status).MaximumLength(80);
    }
}

public sealed class CreateIPAssetCommandValidator : AbstractValidator<CreateIPAssetCommand>
{
    public CreateIPAssetCommandValidator()
    {
        RuleFor(x => x.Type).Must(IPAssetHelpers.IsValidType).WithMessage("Tipo de ativo de PI invalido.");
        RuleFor(x => x.InpiProcessNumber).MaximumLength(80);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(240);
        RuleFor(x => x.OwnerName).MaximumLength(240);
        RuleFor(x => x.Status).MaximumLength(80);
    }
}

public sealed class UpdateIPAssetCommandValidator : AbstractValidator<UpdateIPAssetCommand>
{
    public UpdateIPAssetCommandValidator()
    {
        RuleFor(x => x.Type).Must(IPAssetHelpers.IsValidType).WithMessage("Tipo de ativo de PI invalido.");
        RuleFor(x => x.InpiProcessNumber).MaximumLength(80);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(240);
        RuleFor(x => x.OwnerName).MaximumLength(240);
        RuleFor(x => x.Status).MaximumLength(80);
    }
}

public sealed class ListIPAssetsQueryHandler(IApplicationDbContext dbContext) : IRequestHandler<ListIPAssetsQuery, IReadOnlyList<IPAssetDto>>
{
    public async Task<IReadOnlyList<IPAssetDto>> Handle(ListIPAssetsQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.IPAssets.AsNoTracking().Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            if (!IPAssetHelpers.IsValidType(request.Type))
            {
                return [];
            }

            var type = IPAssetHelpers.NormalizeType(request.Type);
            query = query.Where(x => x.Type == type);
        }

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var term = request.Query.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Title.ToLower().Contains(term)
                || (x.InpiProcessNumber != null && x.InpiProcessNumber.Contains(request.Query))
                || (x.OwnerName != null && x.OwnerName.ToLower().Contains(term)));
        }

        return await query
            .OrderBy(x => x.Title)
            .Select(x => new IPAssetDto(
                x.Id,
                x.Type,
                x.InpiProcessNumber,
                x.Title,
                x.OwnerName,
                x.Status,
                x.FilingDate,
                x.GrantDate,
                x.ExpirationDate,
                x.InternalDeadline,
                x.ClientId,
                x.Client == null ? null : x.Client.Name,
                x.UniversityId,
                x.University == null ? null : x.University.Name,
                x.IsMonitored,
                x.CreatedAtUtc,
                x.UpdatedAtUtc,
                x.IsActive,
                null,
                null))
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetIPAssetQueryHandler(IApplicationDbContext dbContext) : IRequestHandler<GetIPAssetQuery, IPAssetDto>
{
    public async Task<IPAssetDto> Handle(GetIPAssetQuery request, CancellationToken cancellationToken)
    {
        var asset = await dbContext.IPAssets
            .AsNoTracking()
            .Where(x => x.Id == request.Id && x.IsActive)
            .Select(x => new IPAssetDto(
                x.Id,
                x.Type,
                x.InpiProcessNumber,
                x.Title,
                x.OwnerName,
                x.Status,
                x.FilingDate,
                x.GrantDate,
                x.ExpirationDate,
                x.InternalDeadline,
                x.ClientId,
                x.Client == null ? null : x.Client.Name,
                x.UniversityId,
                x.University == null ? null : x.University.Name,
                x.IsMonitored,
                x.CreatedAtUtc,
                x.UpdatedAtUtc,
                x.IsActive,
                null,
                null))
            .SingleOrDefaultAsync(cancellationToken);

        return asset ?? throw new NotFoundException("Ativo de PI nao encontrado.");
    }
}

public sealed class CreateIPAssetCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IInpiSearchService inpiSearchService) : IRequestHandler<CreateIPAssetCommand, IPAssetDto>
{
    public async Task<IPAssetDto> Handle(CreateIPAssetCommand request, CancellationToken cancellationToken)
    {
        await IPAssetHelpers.EnsureReferencesAsync(dbContext, request.ClientId, request.UniversityId, cancellationToken);

        var type = IPAssetHelpers.NormalizeType(request.Type);
        var official = await TryFindOfficialDataAsync(type, request, cancellationToken);
        var now = DateTime.UtcNow;
        var warning = official.Warning;

        var asset = new IPAsset
        {
            Id = Guid.NewGuid(),
            Type = type,
            InpiProcessNumber = official.InpiProcessNumber ?? IPAssetHelpers.TrimToNull(request.InpiProcessNumber),
            Title = official.Title ?? request.Title.Trim(),
            OwnerName = official.OwnerName ?? IPAssetHelpers.TrimToNull(request.OwnerName),
            Status = official.Status ?? IPAssetHelpers.TrimToNull(request.Status) ?? "Draft",
            FilingDate = official.FilingDate ?? request.FilingDate,
            GrantDate = official.GrantDate ?? request.GrantDate,
            ExpirationDate = request.ExpirationDate,
            InternalDeadline = request.InternalDeadline,
            ClientId = request.ClientId,
            UniversityId = request.UniversityId,
            IsMonitored = request.IsMonitored,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            IsActive = true
        };

        dbContext.IPAssets.Add(asset);
        dbContext.AuditLogs.Add(AuditLogHelper.Create(currentUser, "IPPortfolio", "IPAsset", asset.Id, "Created", asset.UniversityId));
        await dbContext.SaveChangesAsync(cancellationToken);

        return IPAssetHelpers.ToDto(asset, null, null, official.Source, warning);
    }

    private async Task<OfficialAssetData> TryFindOfficialDataAsync(string type, CreateIPAssetCommand request, CancellationToken cancellationToken)
    {
        if (type == "Trademark")
        {
            InpiTrademarkResult? trademark = null;
            InpiSearchResultSource? source = null;
            string? warning = null;

            if (!string.IsNullOrWhiteSpace(request.InpiProcessNumber))
            {
                var response = await inpiSearchService.SearchTrademarksAdvancedAsync(
                    new InpiTrademarkSearchRequest(null, false, request.InpiProcessNumber, null, null, null, null, null, null, null, null, 1, 1),
                    cancellationToken);
                trademark = response.Items.FirstOrDefault();
                source = response.Source;
                warning = response.Warning;
            }

            if (trademark is null && !string.IsNullOrWhiteSpace(request.Title))
            {
                var response = await inpiSearchService.SearchTrademarksBasicAsync(
                    new InpiTrademarkSearchRequest(request.Title, true, null, null, null, null, null, null, null, null, null, 1, 1),
                    cancellationToken);
                trademark = response.Items.FirstOrDefault();
                source = response.Source;
                warning = response.Warning;
            }

            if (trademark is not null)
            {
                return new OfficialAssetData(
                    trademark.ProcessNumber,
                    trademark.Name,
                    trademark.Owners.Count == 0 ? null : string.Join(", ", trademark.Owners),
                    trademark.Status,
                    trademark.FilingDate,
                    trademark.RegistrationDate,
                    source?.ToString(),
                    warning);
            }
        }

        if (type == "Patent")
        {
            InpiPatentResult? patent = null;
            InpiSearchResultSource? source = null;
            string? warning = null;

            if (!string.IsNullOrWhiteSpace(request.InpiProcessNumber))
            {
                var response = await inpiSearchService.SearchPatentsAdvancedAsync(
                    new InpiPatentSearchRequest(null, false, request.InpiProcessNumber, null, null, null, null, null, null, null, null, 1, 1),
                    cancellationToken);
                patent = response.Items.FirstOrDefault();
                source = response.Source;
                warning = response.Warning;
            }

            if (patent is null && !string.IsNullOrWhiteSpace(request.Title))
            {
                var response = await inpiSearchService.SearchPatentsBasicAsync(
                    new InpiPatentSearchRequest(request.Title, true, null, null, null, null, null, null, null, null, null, 1, 1),
                    cancellationToken);
                patent = response.Items.FirstOrDefault();
                source = response.Source;
                warning = response.Warning;
            }

            if (patent is not null)
            {
                return new OfficialAssetData(
                    patent.InpiProcessNumber,
                    patent.Title,
                    patent.Applicants.Count == 0 ? null : string.Join(", ", patent.Applicants),
                    patent.Status,
                    patent.FilingDate,
                    patent.GrantDate,
                    source?.ToString(),
                    warning);
            }
        }

        return new OfficialAssetData(
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            "Registro salvo manualmente; nao foi encontrado resultado oficial no INPI ou a consulta online nao esta configurada neste MVP.");
    }
}

public sealed class UpdateIPAssetCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : IRequestHandler<UpdateIPAssetCommand, IPAssetDto>
{
    public async Task<IPAssetDto> Handle(UpdateIPAssetCommand request, CancellationToken cancellationToken)
    {
        await IPAssetHelpers.EnsureReferencesAsync(dbContext, request.ClientId, request.UniversityId, cancellationToken);

        var asset = await dbContext.IPAssets.SingleOrDefaultAsync(x => x.Id == request.Id && x.IsActive, cancellationToken)
            ?? throw new NotFoundException("Ativo de PI nao encontrado.");

        asset.Type = IPAssetHelpers.NormalizeType(request.Type);
        asset.InpiProcessNumber = IPAssetHelpers.TrimToNull(request.InpiProcessNumber);
        asset.Title = request.Title.Trim();
        asset.OwnerName = IPAssetHelpers.TrimToNull(request.OwnerName);
        asset.Status = IPAssetHelpers.TrimToNull(request.Status) ?? "Draft";
        asset.FilingDate = request.FilingDate;
        asset.GrantDate = request.GrantDate;
        asset.ExpirationDate = request.ExpirationDate;
        asset.InternalDeadline = request.InternalDeadline;
        asset.ClientId = request.ClientId;
        asset.UniversityId = request.UniversityId;
        asset.IsMonitored = request.IsMonitored;
        asset.UpdatedAtUtc = DateTime.UtcNow;

        dbContext.AuditLogs.Add(AuditLogHelper.Create(currentUser, "IPPortfolio", "IPAsset", asset.Id, "Updated", asset.UniversityId));
        await dbContext.SaveChangesAsync(cancellationToken);

        return IPAssetHelpers.ToDto(asset, null, null, null, null);
    }
}

public sealed class DeleteIPAssetCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : IRequestHandler<DeleteIPAssetCommand>
{
    public async Task Handle(DeleteIPAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = await dbContext.IPAssets.SingleOrDefaultAsync(x => x.Id == request.Id && x.IsActive, cancellationToken)
            ?? throw new NotFoundException("Ativo de PI nao encontrado.");

        asset.IsActive = false;
        asset.UpdatedAtUtc = DateTime.UtcNow;

        dbContext.AuditLogs.Add(AuditLogHelper.Create(currentUser, "IPPortfolio", "IPAsset", asset.Id, "Deleted", asset.UniversityId));
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

internal static class IPAssetHelpers
{
    private static readonly HashSet<string> ValidTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Trademark",
        "Patent",
        "Software",
        "IndustrialDesign"
    };

    public static bool IsValidType(string? type) => !string.IsNullOrWhiteSpace(type) && ValidTypes.Contains(type.Trim());

    public static string NormalizeType(string type)
    {
        return ValidTypes.Single(x => string.Equals(x, type.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public static string? TrimToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static async Task EnsureReferencesAsync(IApplicationDbContext dbContext, Guid? clientId, Guid? universityId, CancellationToken cancellationToken)
    {
        if (clientId.HasValue)
        {
            var clientExists = await dbContext.Clients.AnyAsync(x => x.Id == clientId.Value && x.IsActive, cancellationToken);
            if (!clientExists)
            {
                throw new NotFoundException("Cliente nao encontrado.");
            }
        }

        if (universityId.HasValue)
        {
            var universityExists = await dbContext.Universities.AnyAsync(x => x.Id == universityId.Value && x.IsActive, cancellationToken);
            if (!universityExists)
            {
                throw new NotFoundException("Universidade nao encontrada.");
            }
        }
    }

    public static IPAssetDto ToDto(IPAsset asset, string? clientName, string? universityName, string? source, string? warning)
    {
        return new IPAssetDto(
            asset.Id,
            asset.Type,
            asset.InpiProcessNumber,
            asset.Title,
            asset.OwnerName,
            asset.Status,
            asset.FilingDate,
            asset.GrantDate,
            asset.ExpirationDate,
            asset.InternalDeadline,
            asset.ClientId,
            clientName,
            asset.UniversityId,
            universityName,
            asset.IsMonitored,
            asset.CreatedAtUtc,
            asset.UpdatedAtUtc,
            asset.IsActive,
            source,
            warning);
    }
}

internal sealed record OfficialAssetData(
    string? InpiProcessNumber,
    string? Title,
    string? OwnerName,
    string? Status,
    DateOnly? FilingDate,
    DateOnly? GrantDate,
    string? Source,
    string? Warning);
