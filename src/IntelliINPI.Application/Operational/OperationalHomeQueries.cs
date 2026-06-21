using IntelliINPI.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Application.Operational;

public sealed record OperationalHomeDto(
    int PendingDeadlinesToday,
    IReadOnlyList<DeadlineDto> UpcomingDeadlines,
    int UnreadMonitoringEvents,
    IReadOnlyList<OperationalHomePendingTrademarkDto> MonitoredTrademarksWithChanges,
    string? LastRpiImportStatus,
    int? LastRpiNumber,
    IReadOnlyList<OperationalHomeEventDto> RecentEvents);

public sealed record OperationalHomePendingTrademarkDto(
    Guid Id,
    Guid TrademarkId,
    string ProcessNumber,
    string TrademarkName,
    string? LastKnownDispatchCode,
    DateOnly? LastKnownDispatchDate);

public sealed record OperationalHomeEventDto(
    Guid Id,
    string ProcessNumber,
    string TrademarkName,
    string? PreviousDispatchCode,
    string? CurrentDispatchCode,
    DateOnly? CurrentDispatchDate,
    DateTime CreatedAtUtc,
    bool IsRead);

public sealed record GetOperationalHomeQuery : IRequest<OperationalHomeDto>;

public sealed class GetOperationalHomeQueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<GetOperationalHomeQuery, OperationalHomeDto>
{
    public async Task<OperationalHomeDto> Handle(GetOperationalHomeQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var upcomingLimit = today.AddDays(30);
        var finishedStatuses = new[] { "Done", "Completed", "Concluido", "Concluído", "Cancelado" };

        var pendingDeadlinesToday = await dbContext.Deadlines
            .AsNoTracking()
            .CountAsync(x => x.IsActive && x.DueDate <= today && !finishedStatuses.Contains(x.Status), cancellationToken);

        var upcomingDeadlines = await ListDeadlinesQueryHandler.Project(
                dbContext.Deadlines
                    .AsNoTracking()
                    .Where(x => x.IsActive && x.DueDate >= today && x.DueDate <= upcomingLimit && !finishedStatuses.Contains(x.Status)))
            .OrderBy(x => x.DueDate)
            .Take(10)
            .ToListAsync(cancellationToken);

        var unreadMonitoringEvents = await dbContext.TrademarkMonitoringEvents
            .AsNoTracking()
            .CountAsync(x => !x.IsRead && x.MonitoredTrademark.UserId == currentUser.UserId, cancellationToken);

        var monitoredTrademarksWithChanges = await dbContext.MonitoredTrademarks
            .AsNoTracking()
            .Where(x => x.UserId == currentUser.UserId && x.IsActive && x.HasPendingChanges)
            .OrderByDescending(x => x.LastKnownDispatchDate)
            .Take(10)
            .Select(x => new OperationalHomePendingTrademarkDto(
                x.Id,
                x.TrademarkId,
                x.ProcessNumber,
                x.Trademark.Name,
                x.LastKnownDispatchCode,
                x.LastKnownDispatchDate))
            .ToListAsync(cancellationToken);

        var lastRpi = await dbContext.RpiImportCheckpoints
            .AsNoTracking()
            .OrderByDescending(x => x.RpiNumber)
            .Select(x => new { x.RpiNumber, x.Status })
            .FirstOrDefaultAsync(cancellationToken);

        var recentEvents = await dbContext.TrademarkMonitoringEvents
            .AsNoTracking()
            .Where(x => x.MonitoredTrademark.UserId == currentUser.UserId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(10)
            .Select(x => new OperationalHomeEventDto(
                x.Id,
                x.ProcessNumber,
                x.Trademark.Name,
                x.PreviousDispatchCode,
                x.CurrentDispatchCode,
                x.CurrentDispatchDate,
                x.CreatedAtUtc,
                x.IsRead))
            .ToListAsync(cancellationToken);

        return new OperationalHomeDto(
            pendingDeadlinesToday,
            upcomingDeadlines,
            unreadMonitoringEvents,
            monitoredTrademarksWithChanges,
            lastRpi?.Status,
            lastRpi?.RpiNumber,
            recentEvents);
    }

}
