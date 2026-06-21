using IntelliINPI.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Application.Monitoring;

public sealed record MonitoredTrademarkDispatchDto(Guid Id, int? RpiNumber, string Code, string Description, DateOnly PublishedAt);

public sealed record MonitoredTrademarkDto(
    Guid Id,
    Guid TrademarkId,
    string ProcessNumber,
    string? InpiDetailUrl,
    string Name,
    string? Notes,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? LastCheckedAtUtc,
    Guid? LastKnownDispatchId,
    string? LastKnownDispatchCode,
    DateOnly? LastKnownDispatchDate,
    bool HasPendingChanges,
    IReadOnlyList<MonitoredTrademarkDispatchDto> RecentDispatches);

public sealed record ListMonitoredTrademarksQuery : IRequest<IReadOnlyList<MonitoredTrademarkDto>>;

public sealed class ListMonitoredTrademarksQueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<ListMonitoredTrademarksQuery, IReadOnlyList<MonitoredTrademarkDto>>
{
    public async Task<IReadOnlyList<MonitoredTrademarkDto>> Handle(ListMonitoredTrademarksQuery request, CancellationToken cancellationToken)
    {
        var rows = await dbContext.MonitoredTrademarks
            .AsNoTracking()
            .Include(x => x.Trademark)
                .ThenInclude(x => x.Dispatches)
            .Where(x => x.UserId == currentUser.UserId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new MonitoredTrademarkDto(
                x.Id,
                x.TrademarkId,
                x.ProcessNumber,
                x.Trademark.InpiDetailUrl,
                x.Trademark.Name,
                x.Notes,
                x.IsActive,
                x.CreatedAtUtc,
                x.LastCheckedAtUtc,
                x.LastKnownDispatchId,
                x.LastKnownDispatchCode,
                x.LastKnownDispatchDate,
                x.HasPendingChanges,
                x.Trademark.Dispatches
                    .OrderByDescending(d => d.PublishedAt)
                    .ThenByDescending(d => d.RpiNumber)
                    .Take(6)
                    .Select(d => new MonitoredTrademarkDispatchDto(d.Id, d.RpiNumber, d.Code, d.Description, d.PublishedAt))
                    .ToList()))
            .ToList();
    }
}
