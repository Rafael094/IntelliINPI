using FluentValidation;
using IntelliINPI.Application.Abstractions;
using IntelliINPI.Application.Common.Exceptions;
using IntelliINPI.Application.Operational;
using IntelliINPI.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Application.IPAssets;

public sealed record InpiDeadlineDto(
    Guid Id,
    Guid IPAssetId,
    string IPAssetType,
    string IPAssetTitle,
    string? InpiProcessNumber,
    string Type,
    string Source,
    int? SourceRpiNumber,
    string? SourceDispatchCode,
    DateOnly? BaseDate,
    DateOnly? DueDate,
    string? LegalBasis,
    string Status,
    bool IsInternal,
    string? Notes,
    DateTime CreatedAtUtc);

public sealed record InpiDeadlineRequest(
    Guid IPAssetId,
    string Type,
    string Source,
    int? SourceRpiNumber,
    string? SourceDispatchCode,
    DateOnly? BaseDate,
    DateOnly? DueDate,
    string? LegalBasis,
    string? Status,
    bool IsInternal,
    string? Notes);

public sealed record ListInpiDeadlinesQuery(bool? IsInternal, int DaysAhead = 90) : IRequest<IReadOnlyList<InpiDeadlineDto>>;
public sealed record GetInpiDeadlineQuery(Guid Id) : IRequest<InpiDeadlineDto>;
public sealed record CreateInpiDeadlineCommand(
    Guid IPAssetId,
    string Type,
    string Source,
    int? SourceRpiNumber,
    string? SourceDispatchCode,
    DateOnly? BaseDate,
    DateOnly? DueDate,
    string? LegalBasis,
    string? Status,
    bool IsInternal,
    string? Notes) : IRequest<InpiDeadlineDto>;
public sealed record UpdateInpiDeadlineCommand(
    Guid Id,
    Guid IPAssetId,
    string Type,
    string Source,
    int? SourceRpiNumber,
    string? SourceDispatchCode,
    DateOnly? BaseDate,
    DateOnly? DueDate,
    string? LegalBasis,
    string? Status,
    bool IsInternal,
    string? Notes) : IRequest<InpiDeadlineDto>;
public sealed record DeleteInpiDeadlineCommand(Guid Id) : IRequest;

public static class InpiDeadlineTypes
{
    public static readonly string[] Allowed =
    [
        "TrademarkOpposition",
        "TrademarkManifestation",
        "TrademarkAppeal",
        "TrademarkRenewal",
        "PatentAnnuity",
        "PatentOfficeActionResponse",
        "PatentAppeal",
        "InternalDeadline",
        "Other"
    ];

    public static bool IsAllowed(string? type) => !string.IsNullOrWhiteSpace(type) && Allowed.Contains(type.Trim(), StringComparer.OrdinalIgnoreCase);
    public static string Normalize(string type) => Allowed.First(x => string.Equals(x, type.Trim(), StringComparison.OrdinalIgnoreCase));
}

public sealed class InpiDeadlineRequestValidator : AbstractValidator<InpiDeadlineRequest>
{
    public InpiDeadlineRequestValidator()
    {
        RuleFor(x => x.IPAssetId).NotEmpty();
        RuleFor(x => x.Type).Must(InpiDeadlineTypes.IsAllowed).WithMessage("Tipo de prazo INPI invalido.");
        RuleFor(x => x.Source).NotEmpty().MaximumLength(80);
        RuleFor(x => x.SourceDispatchCode).MaximumLength(40);
        RuleFor(x => x.LegalBasis).MaximumLength(1000);
        RuleFor(x => x.Status).MaximumLength(80);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

public sealed class CreateInpiDeadlineCommandValidator : AbstractValidator<CreateInpiDeadlineCommand>
{
    public CreateInpiDeadlineCommandValidator()
    {
        RuleFor(x => x.IPAssetId).NotEmpty();
        RuleFor(x => x.Type).Must(InpiDeadlineTypes.IsAllowed).WithMessage("Tipo de prazo INPI invalido.");
        RuleFor(x => x.Source).NotEmpty().MaximumLength(80);
        RuleFor(x => x.SourceDispatchCode).MaximumLength(40);
        RuleFor(x => x.LegalBasis).MaximumLength(1000);
        RuleFor(x => x.Status).MaximumLength(80);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

public sealed class UpdateInpiDeadlineCommandValidator : AbstractValidator<UpdateInpiDeadlineCommand>
{
    public UpdateInpiDeadlineCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.IPAssetId).NotEmpty();
        RuleFor(x => x.Type).Must(InpiDeadlineTypes.IsAllowed).WithMessage("Tipo de prazo INPI invalido.");
        RuleFor(x => x.Source).NotEmpty().MaximumLength(80);
        RuleFor(x => x.SourceDispatchCode).MaximumLength(40);
        RuleFor(x => x.LegalBasis).MaximumLength(1000);
        RuleFor(x => x.Status).MaximumLength(80);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

public sealed class ListInpiDeadlinesQueryHandler(IApplicationDbContext dbContext) : IRequestHandler<ListInpiDeadlinesQuery, IReadOnlyList<InpiDeadlineDto>>
{
    public async Task<IReadOnlyList<InpiDeadlineDto>> Handle(ListInpiDeadlinesQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var limit = today.AddDays(Math.Clamp(request.DaysAhead, 1, 365));
        var query = dbContext.InpiDeadlines.AsNoTracking().Where(x => x.DueDate == null || x.DueDate <= limit);

        if (request.IsInternal.HasValue)
        {
            query = query.Where(x => x.IsInternal == request.IsInternal.Value);
        }

        return await Project(query)
            .OrderBy(x => x.DueDate == null)
            .ThenBy(x => x.DueDate)
            .ThenBy(x => x.IPAssetTitle)
            .ToListAsync(cancellationToken);
    }

    internal static IQueryable<InpiDeadlineDto> Project(IQueryable<InpiDeadline> query)
    {
        return query.Select(x => new InpiDeadlineDto(
            x.Id,
            x.IPAssetId,
            x.IPAsset.Type,
            x.IPAsset.Title,
            x.IPAsset.InpiProcessNumber,
            x.Type,
            x.Source,
            x.SourceRpiNumber,
            x.SourceDispatchCode,
            x.BaseDate,
            x.DueDate,
            x.LegalBasis,
            x.Status,
            x.IsInternal,
            x.Notes,
            x.CreatedAtUtc));
    }
}

public sealed class GetInpiDeadlineQueryHandler(IApplicationDbContext dbContext) : IRequestHandler<GetInpiDeadlineQuery, InpiDeadlineDto>
{
    public async Task<InpiDeadlineDto> Handle(GetInpiDeadlineQuery request, CancellationToken cancellationToken)
    {
        var deadline = await ListInpiDeadlinesQueryHandler.Project(dbContext.InpiDeadlines.AsNoTracking().Where(x => x.Id == request.Id))
            .SingleOrDefaultAsync(cancellationToken);

        return deadline ?? throw new NotFoundException("Prazo de PI nao encontrado.");
    }
}

public sealed class CreateInpiDeadlineCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : IRequestHandler<CreateInpiDeadlineCommand, InpiDeadlineDto>
{
    public async Task<InpiDeadlineDto> Handle(CreateInpiDeadlineCommand request, CancellationToken cancellationToken)
    {
        await EnsureAssetExistsAsync(request.IPAssetId, cancellationToken);

        var normalized = InpiDeadlineCalculator.Normalize(request.Type, request.BaseDate, request.DueDate, request.Status, request.IsInternal, request.Notes, request.LegalBasis);
        var deadline = new InpiDeadline
        {
            Id = Guid.NewGuid(),
            IPAssetId = request.IPAssetId,
            Type = InpiDeadlineTypes.Normalize(request.Type),
            Source = request.Source.Trim(),
            SourceRpiNumber = request.SourceRpiNumber,
            SourceDispatchCode = TrimToNull(request.SourceDispatchCode),
            BaseDate = request.BaseDate,
            DueDate = normalized.DueDate,
            LegalBasis = normalized.LegalBasis,
            Status = normalized.Status,
            IsInternal = request.IsInternal,
            Notes = normalized.Notes,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.InpiDeadlines.Add(deadline);
        dbContext.AuditLogs.Add(AuditLogHelper.Create(currentUser, "IPPortfolio", "InpiDeadline", deadline.Id, "Created"));
        await dbContext.SaveChangesAsync(cancellationToken);

        return await LoadDtoAsync(deadline.Id, cancellationToken);
    }

    private async Task EnsureAssetExistsAsync(Guid ipAssetId, CancellationToken cancellationToken)
    {
        if (!await dbContext.IPAssets.AnyAsync(x => x.Id == ipAssetId && x.IsActive, cancellationToken))
        {
            throw new NotFoundException("Ativo de PI nao encontrado.");
        }
    }

    private async Task<InpiDeadlineDto> LoadDtoAsync(Guid id, CancellationToken cancellationToken)
    {
        return await ListInpiDeadlinesQueryHandler.Project(dbContext.InpiDeadlines.AsNoTracking().Where(x => x.Id == id)).SingleAsync(cancellationToken);
    }

    private static string? TrimToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class UpdateInpiDeadlineCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : IRequestHandler<UpdateInpiDeadlineCommand, InpiDeadlineDto>
{
    public async Task<InpiDeadlineDto> Handle(UpdateInpiDeadlineCommand request, CancellationToken cancellationToken)
    {
        var deadline = await dbContext.InpiDeadlines.SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Prazo de PI nao encontrado.");

        if (!await dbContext.IPAssets.AnyAsync(x => x.Id == request.IPAssetId && x.IsActive, cancellationToken))
        {
            throw new NotFoundException("Ativo de PI nao encontrado.");
        }

        var normalized = InpiDeadlineCalculator.Normalize(request.Type, request.BaseDate, request.DueDate, request.Status, request.IsInternal, request.Notes, request.LegalBasis);
        deadline.IPAssetId = request.IPAssetId;
        deadline.Type = InpiDeadlineTypes.Normalize(request.Type);
        deadline.Source = request.Source.Trim();
        deadline.SourceRpiNumber = request.SourceRpiNumber;
        deadline.SourceDispatchCode = TrimToNull(request.SourceDispatchCode);
        deadline.BaseDate = request.BaseDate;
        deadline.DueDate = normalized.DueDate;
        deadline.LegalBasis = normalized.LegalBasis;
        deadline.Status = normalized.Status;
        deadline.IsInternal = request.IsInternal;
        deadline.Notes = normalized.Notes;

        dbContext.AuditLogs.Add(AuditLogHelper.Create(currentUser, "IPPortfolio", "InpiDeadline", deadline.Id, "Updated"));
        await dbContext.SaveChangesAsync(cancellationToken);

        return await ListInpiDeadlinesQueryHandler.Project(dbContext.InpiDeadlines.AsNoTracking().Where(x => x.Id == deadline.Id)).SingleAsync(cancellationToken);
    }

    private static string? TrimToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class DeleteInpiDeadlineCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : IRequestHandler<DeleteInpiDeadlineCommand>
{
    public async Task Handle(DeleteInpiDeadlineCommand request, CancellationToken cancellationToken)
    {
        var deadline = await dbContext.InpiDeadlines.SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Prazo de PI nao encontrado.");

        dbContext.InpiDeadlines.Remove(deadline);
        dbContext.AuditLogs.Add(AuditLogHelper.Create(currentUser, "IPPortfolio", "InpiDeadline", deadline.Id, "Deleted"));
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

internal static class InpiDeadlineCalculator
{
    public static (DateOnly? DueDate, string Status, string? Notes, string? LegalBasis) Normalize(
        string type,
        DateOnly? baseDate,
        DateOnly? requestedDueDate,
        string? status,
        bool isInternal,
        string? notes,
        string? legalBasis)
    {
        if (isInternal)
        {
            return (requestedDueDate, TrimToNull(status) ?? "Pendente", TrimToNull(notes), TrimToNull(legalBasis));
        }

        if (requestedDueDate.HasValue)
        {
            return (requestedDueDate, TrimToNull(status) ?? "Pendente", TrimToNull(notes), TrimToNull(legalBasis));
        }

        if (!baseDate.HasValue)
        {
            return (null, "RevisaoManualNecessaria", AppendManualReview(notes, "Sem data-base oficial para calculo do prazo."), TrimToNull(legalBasis));
        }

        var normalizedType = InpiDeadlineTypes.Normalize(type);
        return normalizedType switch
        {
            "TrademarkRenewal" => (baseDate.Value.AddYears(10), TrimToNull(status) ?? "Pendente", TrimToNull(notes), TrimToNull(legalBasis) ?? "Prazo configuravel: renovacao decenal de marca."),
            "PatentAnnuity" => (baseDate.Value.AddYears(1), TrimToNull(status) ?? "Pendente", TrimToNull(notes), TrimToNull(legalBasis) ?? "Prazo configuravel: controle anual de patente."),
            _ => (null, "RevisaoManualNecessaria", AppendManualReview(notes, "Regra automatica nao implementada para este tipo; revisar manualmente."), TrimToNull(legalBasis))
        };
    }

    private static string? TrimToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string AppendManualReview(string? notes, string message)
    {
        return string.IsNullOrWhiteSpace(notes) ? message : $"{notes.Trim()} {message}";
    }
}
