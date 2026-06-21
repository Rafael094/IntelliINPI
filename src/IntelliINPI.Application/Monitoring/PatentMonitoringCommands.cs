using FluentValidation;
using IntelliINPI.Application.Abstractions;
using IntelliINPI.Application.Common.Exceptions;
using IntelliINPI.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Application.Monitoring;

public sealed record AddMonitoredPatentRequest(Guid? PatentId, string? InpiProcessNumber, string? Notes);
public sealed record AddMonitoredPatentCommand(Guid? PatentId, string? InpiProcessNumber, string? Notes) : IRequest<Guid>;
public sealed record RemoveMonitoredPatentCommand(Guid Id) : IRequest;
public sealed record CheckPatentMonitoringCommand : IRequest<CheckPatentMonitoringResult>;
public sealed record CheckPatentMonitoringResult(int CheckedCount, int ChangedCount, int EventsCreated);
public sealed record ListMonitoredPatentsQuery : IRequest<IReadOnlyList<MonitoredPatentDto>>;
public sealed record ListPatentMonitoringEventsQuery : IRequest<IReadOnlyList<PatentMonitoringEventDto>>;

public sealed record MonitoredPatentDto(
    Guid Id,
    Guid PatentId,
    string InpiProcessNumber,
    string Title,
    string? Notes,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? LastCheckedAtUtc,
    Guid? LastKnownDispatchId,
    string? LastKnownDispatchCode,
    DateOnly? LastKnownDispatchDate,
    bool HasPendingChanges);

public sealed record PatentMonitoringEventDto(
    Guid Id,
    Guid MonitoredPatentId,
    Guid PatentId,
    string InpiProcessNumber,
    string Title,
    string EventType,
    string? PreviousDispatchCode,
    string? CurrentDispatchCode,
    DateOnly? PreviousDispatchDate,
    DateOnly? CurrentDispatchDate,
    DateTime CreatedAtUtc,
    bool IsRead);

public sealed class AddMonitoredPatentCommandValidator : AbstractValidator<AddMonitoredPatentCommand>
{
    public AddMonitoredPatentCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => x.PatentId.HasValue || !string.IsNullOrWhiteSpace(x.InpiProcessNumber))
            .WithMessage("Informe patentId ou inpiProcessNumber.");

        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public sealed class AddMonitoredPatentCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<AddMonitoredPatentCommand, Guid>
{
    public async Task<Guid> Handle(AddMonitoredPatentCommand request, CancellationToken cancellationToken)
    {
        var processNumber = request.InpiProcessNumber?.Trim();
        var patent = request.PatentId.HasValue
            ? await dbContext.Patents.SingleOrDefaultAsync(x => x.Id == request.PatentId.Value && x.IsActive, cancellationToken)
            : await dbContext.Patents.SingleOrDefaultAsync(x => x.InpiProcessNumber == processNumber && x.IsActive, cancellationToken);

        if (patent is null && !string.IsNullOrWhiteSpace(processNumber))
        {
            patent = new Patent
            {
                Id = Guid.NewGuid(),
                InpiProcessNumber = processNumber,
                Title = processNumber,
                Status = "Draft",
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                IsActive = true
            };
            dbContext.Patents.Add(patent);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (patent is null)
        {
            throw new NotFoundException("Patente nao encontrada na base local.");
        }

        var existingMonitor = await dbContext.MonitoredPatents
            .SingleOrDefaultAsync(x => x.PatentId == patent.Id && x.UserId == currentUser.UserId, cancellationToken);

        if (existingMonitor is not null)
        {
            existingMonitor.IsActive = true;
            existingMonitor.LastCheckedAtUtc = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                existingMonitor.Notes = request.Notes.Trim();
            }

            await MonitoringIPAssetSync.SyncPatentAsync(dbContext, patent, true, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return existingMonitor.Id;
        }

        var latestDispatch = await dbContext.PatentDispatches
            .AsNoTracking()
            .Where(x => x.PatentId == patent.Id)
            .OrderByDescending(x => x.DispatchDate)
            .ThenByDescending(x => x.RpiNumber ?? 0)
            .ThenByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var monitoredPatent = new MonitoredPatent
        {
            Id = Guid.NewGuid(),
            PatentId = patent.Id,
            UserId = currentUser.UserId,
            InpiProcessNumber = patent.InpiProcessNumber,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            IsActive = true,
            CreatedAtUtc = now,
            LastCheckedAtUtc = now,
            LastKnownDispatchId = latestDispatch?.Id,
            LastKnownDispatchCode = latestDispatch?.DispatchCode,
            LastKnownDispatchDate = latestDispatch?.DispatchDate,
            HasPendingChanges = false
        };

        dbContext.MonitoredPatents.Add(monitoredPatent);
        await MonitoringIPAssetSync.SyncPatentAsync(dbContext, patent, true, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return monitoredPatent.Id;
    }
}

public sealed class ListMonitoredPatentsQueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<ListMonitoredPatentsQuery, IReadOnlyList<MonitoredPatentDto>>
{
    public async Task<IReadOnlyList<MonitoredPatentDto>> Handle(ListMonitoredPatentsQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.MonitoredPatents
            .AsNoTracking()
            .Where(x => x.UserId == currentUser.UserId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new MonitoredPatentDto(
                x.Id,
                x.PatentId,
                x.InpiProcessNumber,
                x.Patent.Title,
                x.Notes,
                x.IsActive,
                x.CreatedAtUtc,
                x.LastCheckedAtUtc,
                x.LastKnownDispatchId,
                x.LastKnownDispatchCode,
                x.LastKnownDispatchDate,
                x.HasPendingChanges))
            .ToListAsync(cancellationToken);
    }
}

public sealed class RemoveMonitoredPatentCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<RemoveMonitoredPatentCommand>
{
    public async Task Handle(RemoveMonitoredPatentCommand request, CancellationToken cancellationToken)
    {
        var monitor = await dbContext.MonitoredPatents
            .Include(x => x.Patent)
            .SingleOrDefaultAsync(x => x.Id == request.Id && x.UserId == currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException("Monitoramento de patente nao encontrado.");

        var hasOtherActiveMonitors = await dbContext.MonitoredPatents
            .AnyAsync(x => x.PatentId == monitor.PatentId && x.Id != monitor.Id && x.IsActive, cancellationToken);

        monitor.IsActive = false;
        await MonitoringIPAssetSync.SyncPatentAsync(dbContext, monitor.Patent, hasOtherActiveMonitors, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

public sealed class CheckPatentMonitoringCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<CheckPatentMonitoringCommand, CheckPatentMonitoringResult>
{
    private const string DispatchChangedEventType = "DispatchChanged";

    public async Task<CheckPatentMonitoringResult> Handle(CheckPatentMonitoringCommand request, CancellationToken cancellationToken)
    {
        var monitoredPatents = await dbContext.MonitoredPatents
            .Where(x => x.UserId == currentUser.UserId && x.IsActive)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var changedCount = 0;
        var eventsCreated = 0;

        foreach (var monitoredPatent in monitoredPatents)
        {
            var latestDispatch = await dbContext.PatentDispatches
                .AsNoTracking()
                .Where(x => x.PatentId == monitoredPatent.PatentId)
                .OrderByDescending(x => x.DispatchDate)
                .ThenByDescending(x => x.RpiNumber ?? 0)
                .ThenByDescending(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            monitoredPatent.LastCheckedAtUtc = now;

            if (latestDispatch is null || monitoredPatent.LastKnownDispatchId == latestDispatch.Id)
            {
                continue;
            }

            dbContext.PatentMonitoringEvents.Add(new PatentMonitoringEvent
            {
                Id = Guid.NewGuid(),
                MonitoredPatentId = monitoredPatent.Id,
                PatentId = monitoredPatent.PatentId,
                DispatchId = latestDispatch.Id,
                InpiProcessNumber = monitoredPatent.InpiProcessNumber,
                EventType = DispatchChangedEventType,
                PreviousDispatchCode = monitoredPatent.LastKnownDispatchCode,
                CurrentDispatchCode = latestDispatch.DispatchCode,
                PreviousDispatchDate = monitoredPatent.LastKnownDispatchDate,
                CurrentDispatchDate = latestDispatch.DispatchDate,
                CreatedAtUtc = now,
                IsRead = false
            });

            monitoredPatent.LastKnownDispatchId = latestDispatch.Id;
            monitoredPatent.LastKnownDispatchCode = latestDispatch.DispatchCode;
            monitoredPatent.LastKnownDispatchDate = latestDispatch.DispatchDate;
            monitoredPatent.HasPendingChanges = true;

            changedCount++;
            eventsCreated++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return new CheckPatentMonitoringResult(monitoredPatents.Count, changedCount, eventsCreated);
    }
}

public sealed class ListPatentMonitoringEventsQueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<ListPatentMonitoringEventsQuery, IReadOnlyList<PatentMonitoringEventDto>>
{
    public async Task<IReadOnlyList<PatentMonitoringEventDto>> Handle(ListPatentMonitoringEventsQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.PatentMonitoringEvents
            .AsNoTracking()
            .Where(x => x.MonitoredPatent.UserId == currentUser.UserId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new PatentMonitoringEventDto(
                x.Id,
                x.MonitoredPatentId,
                x.PatentId,
                x.InpiProcessNumber,
                x.Patent.Title,
                x.EventType,
                x.PreviousDispatchCode,
                x.CurrentDispatchCode,
                x.PreviousDispatchDate,
                x.CurrentDispatchDate,
                x.CreatedAtUtc,
                x.IsRead))
            .ToListAsync(cancellationToken);
    }
}

internal static class MonitoringIPAssetSync
{
    public static async Task SyncPatentAsync(IApplicationDbContext dbContext, Patent patent, bool isMonitored, CancellationToken cancellationToken)
    {
        var asset = await dbContext.IPAssets
            .SingleOrDefaultAsync(x => x.Type == "Patent" && x.InpiProcessNumber == patent.InpiProcessNumber && x.IsActive, cancellationToken);

        if (asset is null)
        {
            dbContext.IPAssets.Add(new IPAsset
            {
                Id = Guid.NewGuid(),
                Type = "Patent",
                InpiProcessNumber = patent.InpiProcessNumber,
                Title = patent.Title,
                OwnerName = patent.Applicants,
                Status = patent.Status ?? "Draft",
                FilingDate = patent.FilingDate,
                GrantDate = patent.GrantDate,
                IsMonitored = isMonitored,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                IsActive = true
            });
            return;
        }

        asset.Title = patent.Title;
        asset.OwnerName = patent.Applicants;
        asset.Status = patent.Status ?? asset.Status;
        asset.FilingDate = patent.FilingDate ?? asset.FilingDate;
        asset.GrantDate = patent.GrantDate ?? asset.GrantDate;
        asset.IsMonitored = isMonitored;
        asset.UpdatedAtUtc = DateTime.UtcNow;
    }

    public static async Task SyncTrademarkAsync(IApplicationDbContext dbContext, Trademark trademark, bool isMonitored, CancellationToken cancellationToken)
    {
        var asset = await dbContext.IPAssets
            .SingleOrDefaultAsync(x => x.Type == "Trademark" && x.InpiProcessNumber == trademark.ProcessNumber && x.IsActive, cancellationToken);

        var ownerName = trademark.Owner?.Name;
        var status = trademark.Status?.Description ?? "Status nao importado";

        if (asset is null)
        {
            dbContext.IPAssets.Add(new IPAsset
            {
                Id = Guid.NewGuid(),
                Type = "Trademark",
                InpiProcessNumber = trademark.ProcessNumber,
                Title = trademark.Name,
                OwnerName = ownerName,
                Status = status,
                FilingDate = trademark.FilingDate,
                GrantDate = trademark.RegistrationDate,
                IsMonitored = isMonitored,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                IsActive = true
            });
            return;
        }

        asset.Title = trademark.Name;
        asset.OwnerName = ownerName ?? asset.OwnerName;
        asset.Status = status;
        asset.FilingDate = trademark.FilingDate ?? asset.FilingDate;
        asset.GrantDate = trademark.RegistrationDate ?? asset.GrantDate;
        asset.IsMonitored = isMonitored;
        asset.UpdatedAtUtc = DateTime.UtcNow;
    }
}
