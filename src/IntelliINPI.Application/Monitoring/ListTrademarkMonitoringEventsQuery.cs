using IntelliINPI.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Application.Monitoring;

public sealed record TrademarkMonitoringEventDto(
    Guid Id,
    string ProcessNumber,
    string TrademarkName,
    string EventType,
    string? PreviousDispatchCode,
    string? CurrentDispatchCode,
    DateOnly? PreviousDispatchDate,
    DateOnly? CurrentDispatchDate,
    DateTime CreatedAtUtc,
    bool IsRead);

public sealed record ListTrademarkMonitoringEventsQuery(bool UnreadOnly) : IRequest<IReadOnlyList<TrademarkMonitoringEventDto>>;

public sealed class ListTrademarkMonitoringEventsQueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<ListTrademarkMonitoringEventsQuery, IReadOnlyList<TrademarkMonitoringEventDto>>
{
    public async Task<IReadOnlyList<TrademarkMonitoringEventDto>> Handle(ListTrademarkMonitoringEventsQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.TrademarkMonitoringEvents
            .AsNoTracking()
            .Where(x => x.MonitoredTrademark.UserId == currentUser.UserId);

        if (request.UnreadOnly)
        {
            query = query.Where(x => !x.IsRead);
        }

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new TrademarkMonitoringEventDto(
                x.Id,
                x.ProcessNumber,
                x.Trademark.Name,
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
