using IntelliINPI.Application.Abstractions;
using IntelliINPI.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Application.Monitoring;

public sealed record CheckTrademarkMonitoringResult(int CheckedCount, int ChangedCount, int EventsCreated);
public sealed record CheckTrademarkMonitoringCommand : IRequest<CheckTrademarkMonitoringResult>;

public sealed class CheckTrademarkMonitoringCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<CheckTrademarkMonitoringCommand, CheckTrademarkMonitoringResult>
{
    private const string DispatchChangedEventType = "DispatchChanged";

    public async Task<CheckTrademarkMonitoringResult> Handle(CheckTrademarkMonitoringCommand request, CancellationToken cancellationToken)
    {
        var monitoredTrademarks = await dbContext.MonitoredTrademarks
            .Where(x => x.UserId == currentUser.UserId && x.IsActive)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var changedCount = 0;
        var eventsCreated = 0;

        foreach (var monitoredTrademark in monitoredTrademarks)
        {
            var latestDispatch = await dbContext.TrademarkDispatches
                .AsNoTracking()
                .Where(x => x.TrademarkId == monitoredTrademark.TrademarkId)
                .OrderByDescending(x => x.PublishedAt)
                .ThenByDescending(x => x.RpiNumber ?? 0)
                .ThenByDescending(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            monitoredTrademark.LastCheckedAtUtc = now;

            if (latestDispatch is null || monitoredTrademark.LastKnownDispatchId == latestDispatch.Id)
            {
                continue;
            }

            dbContext.TrademarkMonitoringEvents.Add(new TrademarkMonitoringEvent
            {
                Id = Guid.NewGuid(),
                MonitoredTrademarkId = monitoredTrademark.Id,
                TrademarkId = monitoredTrademark.TrademarkId,
                DispatchId = latestDispatch.Id,
                ProcessNumber = monitoredTrademark.ProcessNumber,
                EventType = DispatchChangedEventType,
                PreviousDispatchCode = monitoredTrademark.LastKnownDispatchCode,
                CurrentDispatchCode = latestDispatch.Code,
                PreviousDispatchDate = monitoredTrademark.LastKnownDispatchDate,
                CurrentDispatchDate = latestDispatch.PublishedAt,
                CreatedAtUtc = now,
                IsRead = false
            });

            monitoredTrademark.LastKnownDispatchId = latestDispatch.Id;
            monitoredTrademark.LastKnownDispatchCode = latestDispatch.Code;
            monitoredTrademark.LastKnownDispatchDate = latestDispatch.PublishedAt;
            monitoredTrademark.HasPendingChanges = true;

            changedCount++;
            eventsCreated++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return new CheckTrademarkMonitoringResult(monitoredTrademarks.Count, changedCount, eventsCreated);
    }
}
