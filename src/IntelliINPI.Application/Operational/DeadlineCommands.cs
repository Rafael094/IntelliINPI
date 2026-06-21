using FluentValidation;
using IntelliINPI.Application.Abstractions;
using IntelliINPI.Application.Common.Exceptions;
using IntelliINPI.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Application.Operational;

public sealed record DeadlineDto(
    Guid Id,
    string Title,
    string? Description,
    DateOnly DueDate,
    string Status,
    string Type,
    Guid? ClientId,
    string? ClientName,
    Guid? TrademarkId,
    string? TrademarkName,
    string? TrademarkProcessNumber,
    Guid? InventionId,
    string? InventionTitle,
    DateTime CreatedAtUtc,
    bool IsActive);

public sealed record OperationalDeadlineDto(
    string Id,
    string Source,
    string Scope,
    string Type,
    string Title,
    string? Description,
    DateOnly? DueDate,
    int? DaysUntilDue,
    string Status,
    string StatusLabel,
    Guid? TrademarkId,
    string? TrademarkName,
    string? TrademarkProcessNumber,
    Guid? ClientId,
    string? ClientName,
    Guid? InventionId,
    string? InventionTitle,
    Guid? IPAssetId,
    string? IPAssetTitle,
    bool RequiresManualReview);

public sealed record DeadlineRequest(
    string Title,
    string? Description,
    DateOnly DueDate,
    string Status,
    string Type,
    Guid? ClientId,
    Guid? TrademarkId,
    Guid? InventionId);

public sealed record ListDeadlinesQuery : IRequest<IReadOnlyList<DeadlineDto>>;
public sealed record ListOperationalDeadlinesQuery(int DaysAhead = 365, bool IncludeManualReview = true) : IRequest<IReadOnlyList<OperationalDeadlineDto>>;
public sealed record GetDeadlineQuery(Guid Id) : IRequest<DeadlineDto>;
public sealed record CreateDeadlineCommand(string Title, string? Description, DateOnly DueDate, string Status, string Type, Guid? ClientId, Guid? TrademarkId, Guid? InventionId) : IRequest<DeadlineDto>;
public sealed record UpdateDeadlineCommand(Guid Id, string Title, string? Description, DateOnly DueDate, string Status, string Type, Guid? ClientId, Guid? TrademarkId, Guid? InventionId) : IRequest<DeadlineDto>;
public sealed record DeleteDeadlineCommand(Guid Id) : IRequest;

public static class DeadlineTypes
{
    public static readonly string[] Allowed =
    [
        "INPIRequirement",
        "Opposition",
        "Appeal",
        "Annuity",
        "Renewal",
        "ContractExpiration",
        "Other"
    ];

    public static bool IsAllowed(string type) => Allowed.Contains(type, StringComparer.OrdinalIgnoreCase);
    public static string Normalize(string type) => Allowed.First(x => string.Equals(x, type, StringComparison.OrdinalIgnoreCase));
}

public sealed class DeadlineRequestValidator : AbstractValidator<DeadlineRequest>
{
    public DeadlineRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Status).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Type).NotEmpty().Must(DeadlineTypes.IsAllowed).WithMessage("Tipo de prazo invalido.");
    }
}

public sealed class CreateDeadlineCommandValidator : AbstractValidator<CreateDeadlineCommand>
{
    public CreateDeadlineCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Status).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Type).NotEmpty().Must(DeadlineTypes.IsAllowed).WithMessage("Tipo de prazo invalido.");
    }
}

public sealed class UpdateDeadlineCommandValidator : AbstractValidator<UpdateDeadlineCommand>
{
    public UpdateDeadlineCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Status).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Type).NotEmpty().Must(DeadlineTypes.IsAllowed).WithMessage("Tipo de prazo invalido.");
    }
}

public sealed class ListDeadlinesQueryHandler(IApplicationDbContext dbContext) : IRequestHandler<ListDeadlinesQuery, IReadOnlyList<DeadlineDto>>
{
    public async Task<IReadOnlyList<DeadlineDto>> Handle(ListDeadlinesQuery request, CancellationToken cancellationToken)
    {
        return await Project(dbContext.Deadlines.AsNoTracking().Where(x => x.IsActive))
            .OrderBy(x => x.DueDate)
            .ToListAsync(cancellationToken);
    }

    internal static IQueryable<DeadlineDto> Project(IQueryable<Deadline> query)
    {
        return query.Select(x => new DeadlineDto(
            x.Id,
            x.Title,
            x.Description,
            x.DueDate,
            x.Status,
            x.Type,
            x.ClientId,
            x.Client != null ? x.Client.Name : null,
            x.TrademarkId,
            x.Trademark != null ? x.Trademark.Name : null,
            x.Trademark != null ? x.Trademark.ProcessNumber : null,
            x.InventionId,
            x.Invention != null ? x.Invention.Title : null,
            x.CreatedAtUtc,
            x.IsActive));
    }
}

public sealed class ListOperationalDeadlinesQueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<ListOperationalDeadlinesQuery, IReadOnlyList<OperationalDeadlineDto>>
{
    public async Task<IReadOnlyList<OperationalDeadlineDto>> Handle(ListOperationalDeadlinesQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var limit = today.AddDays(Math.Clamp(request.DaysAhead, 1, 3650));
        var items = new List<OperationalDeadlineDto>();

        var manualDeadlines = await ListDeadlinesQueryHandler.Project(dbContext.Deadlines.AsNoTracking().Where(x => x.IsActive))
            .ToListAsync(cancellationToken);
        items.AddRange(manualDeadlines.Select(x => new OperationalDeadlineDto(
            $"manual:{x.Id}",
            "Manual",
            "Prazo interno",
            x.Type,
            x.Title,
            x.Description,
            x.DueDate,
            DaysUntil(today, x.DueDate),
            ResolveStatus(today, x.DueDate, x.Status),
            ResolveStatusLabel(today, x.DueDate, x.Status),
            x.TrademarkId,
            x.TrademarkName,
            x.TrademarkProcessNumber,
            x.ClientId,
            x.ClientName,
            x.InventionId,
            x.InventionTitle,
            null,
            null,
            false)));

        var inpiDeadlines = await dbContext.InpiDeadlines
            .AsNoTracking()
            .Where(x => x.DueDate == null || x.DueDate <= limit)
            .Select(x => new
            {
                x.Id,
                x.Type,
                x.Source,
                x.BaseDate,
                x.DueDate,
                x.Status,
                x.IsInternal,
                x.Notes,
                x.LegalBasis,
                IPAssetId = x.IPAssetId,
                IPAssetTitle = x.IPAsset.Title,
                x.IPAsset.InpiProcessNumber
            })
            .ToListAsync(cancellationToken);

        items.AddRange(inpiDeadlines.Select(x => new OperationalDeadlineDto(
            $"inpi:{x.Id}",
            x.Source,
            x.IsInternal ? "Prazo interno de PI" : "Prazo INPI",
            x.Type,
            BuildInpiDeadlineTitle(x.Type, x.IPAssetTitle, x.InpiProcessNumber),
            x.Notes ?? x.LegalBasis,
            x.DueDate,
            x.DueDate.HasValue ? DaysUntil(today, x.DueDate.Value) : null,
            x.DueDate.HasValue ? ResolveStatus(today, x.DueDate.Value, x.Status) : "ManualReviewRequired",
            x.DueDate.HasValue ? ResolveStatusLabel(today, x.DueDate.Value, x.Status) : "Revisão manual necessária",
            null,
            null,
            x.InpiProcessNumber,
            null,
            null,
            null,
            null,
            x.IPAssetId,
            x.IPAssetTitle,
            !x.DueDate.HasValue || string.Equals(x.Status, "RevisaoManualNecessaria", StringComparison.OrdinalIgnoreCase))));

        var monitored = await dbContext.MonitoredTrademarks
            .AsNoTracking()
            .Where(x => x.UserId == currentUser.UserId && x.IsActive)
            .Select(x => new
            {
                x.TrademarkId,
                x.ProcessNumber,
                x.Trademark.Name,
                x.Trademark.ExpirationDate,
                x.Trademark.RegistrationDate,
                x.Trademark.InpiDetailUrl,
                x.LastKnownDispatchCode,
                x.LastKnownDispatchDate,
                x.HasPendingChanges
            })
            .ToListAsync(cancellationToken);

        foreach (var item in monitored)
        {
            if (item.ExpirationDate.HasValue)
            {
                var expiration = item.ExpirationDate.Value;
                var ordinaryStart = expiration.AddYears(-1);
                var ordinaryEnd = expiration;
                var extraordinaryStart = expiration.AddDays(1);
                var extraordinaryEnd = expiration.AddMonths(6);

                AddIfRelevant(items, today, limit, new OperationalDeadlineDto(
                    $"monitored-renewal-ordinary:{item.TrademarkId}",
                    "Marca monitorada",
                    "Prazo INPI",
                    "TrademarkRenewal",
                    $"Prorrogação ordinária - {item.Name}",
                    $"Processo {item.ProcessNumber}. Janela ordinária: {ordinaryStart:dd/MM/yyyy} a {ordinaryEnd:dd/MM/yyyy}.",
                    ordinaryEnd,
                    DaysUntil(today, ordinaryEnd),
                    ResolveStatus(today, ordinaryEnd, "Pendente"),
                    ResolveStatusLabel(today, ordinaryEnd, "Pendente"),
                    item.TrademarkId,
                    item.Name,
                    item.ProcessNumber,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    false));

                AddIfRelevant(items, today, limit, new OperationalDeadlineDto(
                    $"monitored-renewal-extraordinary:{item.TrademarkId}",
                    "Marca monitorada",
                    "Prazo INPI",
                    "TrademarkRenewal",
                    $"Prorrogação extraordinária - {item.Name}",
                    $"Processo {item.ProcessNumber}. Janela extraordinária: {extraordinaryStart:dd/MM/yyyy} a {extraordinaryEnd:dd/MM/yyyy}.",
                    extraordinaryEnd,
                    DaysUntil(today, extraordinaryEnd),
                    ResolveStatus(today, extraordinaryEnd, "Pendente"),
                    ResolveStatusLabel(today, extraordinaryEnd, "Pendente"),
                    item.TrademarkId,
                    item.Name,
                    item.ProcessNumber,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    false));
            }
            else if (request.IncludeManualReview)
            {
                items.Add(new OperationalDeadlineDto(
                    $"monitored-review:{item.TrademarkId}",
                    "Marca monitorada",
                    "Revisão de prazo",
                    "ManualReview",
                    $"Revisar prazos da marca - {item.Name}",
                    $"Processo {item.ProcessNumber}. Data de vigência ainda não está disponível localmente. Abra a ficha da marca para sincronizar o detalhe do INPI.",
                    null,
                    null,
                    "ManualReviewRequired",
                    "Revisão manual necessária",
                    item.TrademarkId,
                    item.Name,
                    item.ProcessNumber,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    true));
            }

            if (item.HasPendingChanges && request.IncludeManualReview)
            {
                items.Add(new OperationalDeadlineDto(
                    $"monitored-dispatch-review:{item.TrademarkId}",
                    "Marca monitorada",
                    "Revisão de despacho",
                    "DispatchReview",
                    $"Revisar novo despacho - {item.Name}",
                    $"Processo {item.ProcessNumber}. Último despacho conhecido: {item.LastKnownDispatchCode ?? "não informado"} em {(item.LastKnownDispatchDate.HasValue ? item.LastKnownDispatchDate.Value.ToString("dd/MM/yyyy") : "data não informada")}.",
                    item.LastKnownDispatchDate,
                    item.LastKnownDispatchDate.HasValue ? DaysUntil(today, item.LastKnownDispatchDate.Value) : null,
                    "ManualReviewRequired",
                    "Revisão manual necessária",
                    item.TrademarkId,
                    item.Name,
                    item.ProcessNumber,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    true));
            }
        }

        return items
            .Where(x => !x.DueDate.HasValue || x.DueDate.Value <= limit || x.Status == "Overdue")
            .OrderBy(x => x.DueDate.HasValue ? 0 : 1)
            .ThenBy(x => x.DueDate)
            .ThenBy(x => x.Title)
            .ToList();
    }

    private static void AddIfRelevant(List<OperationalDeadlineDto> items, DateOnly today, DateOnly limit, OperationalDeadlineDto deadline)
    {
        if (!deadline.DueDate.HasValue || deadline.DueDate.Value <= limit || deadline.DueDate.Value < today)
        {
            items.Add(deadline);
        }
    }

    private static int DaysUntil(DateOnly today, DateOnly dueDate) => dueDate.DayNumber - today.DayNumber;

    private static string ResolveStatus(DateOnly today, DateOnly dueDate, string currentStatus)
    {
        if (IsDone(currentStatus))
        {
            return "Completed";
        }

        var days = DaysUntil(today, dueDate);
        if (days < 0) return "Overdue";
        if (days == 0) return "DueToday";
        if (days <= 30) return "DueSoon";
        return "Upcoming";
    }

    private static string ResolveStatusLabel(DateOnly today, DateOnly dueDate, string currentStatus)
    {
        if (IsDone(currentStatus)) return "Concluído";
        var days = DaysUntil(today, dueDate);
        if (days < 0) return $"Vencido há {Math.Abs(days)} dia(s)";
        if (days == 0) return "Vence hoje";
        if (days == 1) return "Vence amanhã";
        return $"Vence em {days} dia(s)";
    }

    private static bool IsDone(string status)
    {
        return status.Equals("Concluido", StringComparison.OrdinalIgnoreCase)
            || status.Equals("Concluído", StringComparison.OrdinalIgnoreCase)
            || status.Equals("Completed", StringComparison.OrdinalIgnoreCase)
            || status.Equals("Done", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildInpiDeadlineTitle(string type, string assetTitle, string? processNumber)
    {
        var label = type switch
        {
            "TrademarkOpposition" => "Oposição de marca",
            "TrademarkManifestation" => "Manifestação de marca",
            "TrademarkAppeal" => "Recurso de marca",
            "TrademarkRenewal" => "Renovação de marca",
            "PatentAnnuity" => "Anuidade de patente",
            "PatentOfficeActionResponse" => "Resposta a exigência",
            "PatentAppeal" => "Recurso de patente",
            "InternalDeadline" => "Prazo interno",
            _ => "Prazo de PI"
        };

        return string.IsNullOrWhiteSpace(processNumber)
            ? $"{label} - {assetTitle}"
            : $"{label} - {assetTitle} ({processNumber})";
    }
}

public sealed class GetDeadlineQueryHandler(IApplicationDbContext dbContext) : IRequestHandler<GetDeadlineQuery, DeadlineDto>
{
    public async Task<DeadlineDto> Handle(GetDeadlineQuery request, CancellationToken cancellationToken)
    {
        var deadline = await ListDeadlinesQueryHandler.Project(dbContext.Deadlines.AsNoTracking().Where(x => x.Id == request.Id && x.IsActive))
            .SingleOrDefaultAsync(cancellationToken);

        return deadline ?? throw new NotFoundException("Prazo nao encontrado.");
    }
}

public sealed class CreateDeadlineCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : IRequestHandler<CreateDeadlineCommand, DeadlineDto>
{
    public async Task<DeadlineDto> Handle(CreateDeadlineCommand request, CancellationToken cancellationToken)
    {
        await EnsureReferencesExistAsync(request.ClientId, request.TrademarkId, request.InventionId, cancellationToken);

        var deadline = new Deadline
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = TrimToNull(request.Description),
            DueDate = request.DueDate,
            Status = request.Status.Trim(),
            Type = DeadlineTypes.Normalize(request.Type),
            ClientId = request.ClientId,
            TrademarkId = request.TrademarkId,
            InventionId = request.InventionId,
            CreatedAtUtc = DateTime.UtcNow,
            IsActive = true
        };

        dbContext.Deadlines.Add(deadline);
        dbContext.AuditLogs.Add(AuditLogHelper.Create(currentUser, "Operational", "Deadline", deadline.Id, "Created"));
        await dbContext.SaveChangesAsync(cancellationToken);

        return await LoadDtoAsync(deadline.Id, cancellationToken);
    }

    private async Task EnsureReferencesExistAsync(Guid? clientId, Guid? trademarkId, Guid? inventionId, CancellationToken cancellationToken)
    {
        if (clientId.HasValue && !await dbContext.Clients.AnyAsync(x => x.Id == clientId && x.IsActive, cancellationToken))
            throw new NotFoundException("Cliente nao encontrado.");

        if (trademarkId.HasValue && !await dbContext.Trademarks.AnyAsync(x => x.Id == trademarkId, cancellationToken))
            throw new NotFoundException("Marca nao encontrada.");

        if (inventionId.HasValue && !await dbContext.Inventions.AnyAsync(x => x.Id == inventionId && x.IsActive, cancellationToken))
            throw new NotFoundException("Invencao nao encontrada.");
    }

    private async Task<DeadlineDto> LoadDtoAsync(Guid id, CancellationToken cancellationToken)
    {
        return await ListDeadlinesQueryHandler.Project(dbContext.Deadlines.AsNoTracking().Where(x => x.Id == id)).SingleAsync(cancellationToken);
    }

    private static string? TrimToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class UpdateDeadlineCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : IRequestHandler<UpdateDeadlineCommand, DeadlineDto>
{
    public async Task<DeadlineDto> Handle(UpdateDeadlineCommand request, CancellationToken cancellationToken)
    {
        var deadline = await dbContext.Deadlines.SingleOrDefaultAsync(x => x.Id == request.Id && x.IsActive, cancellationToken)
            ?? throw new NotFoundException("Prazo nao encontrado.");

        await EnsureReferencesExistAsync(request.ClientId, request.TrademarkId, request.InventionId, cancellationToken);

        deadline.Title = request.Title.Trim();
        deadline.Description = TrimToNull(request.Description);
        deadline.DueDate = request.DueDate;
        deadline.Status = request.Status.Trim();
        deadline.Type = DeadlineTypes.Normalize(request.Type);
        deadline.ClientId = request.ClientId;
        deadline.TrademarkId = request.TrademarkId;
        deadline.InventionId = request.InventionId;
        dbContext.AuditLogs.Add(AuditLogHelper.Create(currentUser, "Operational", "Deadline", deadline.Id, "Updated"));
        await dbContext.SaveChangesAsync(cancellationToken);

        return await ListDeadlinesQueryHandler.Project(dbContext.Deadlines.AsNoTracking().Where(x => x.Id == deadline.Id)).SingleAsync(cancellationToken);
    }

    private async Task EnsureReferencesExistAsync(Guid? clientId, Guid? trademarkId, Guid? inventionId, CancellationToken cancellationToken)
    {
        if (clientId.HasValue && !await dbContext.Clients.AnyAsync(x => x.Id == clientId && x.IsActive, cancellationToken))
            throw new NotFoundException("Cliente nao encontrado.");

        if (trademarkId.HasValue && !await dbContext.Trademarks.AnyAsync(x => x.Id == trademarkId, cancellationToken))
            throw new NotFoundException("Marca nao encontrada.");

        if (inventionId.HasValue && !await dbContext.Inventions.AnyAsync(x => x.Id == inventionId && x.IsActive, cancellationToken))
            throw new NotFoundException("Invencao nao encontrada.");
    }

    private static string? TrimToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class DeleteDeadlineCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : IRequestHandler<DeleteDeadlineCommand>
{
    public async Task Handle(DeleteDeadlineCommand request, CancellationToken cancellationToken)
    {
        var deadline = await dbContext.Deadlines.SingleOrDefaultAsync(x => x.Id == request.Id && x.IsActive, cancellationToken)
            ?? throw new NotFoundException("Prazo nao encontrado.");

        deadline.IsActive = false;
        dbContext.AuditLogs.Add(AuditLogHelper.Create(currentUser, "Operational", "Deadline", deadline.Id, "Deleted"));
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
