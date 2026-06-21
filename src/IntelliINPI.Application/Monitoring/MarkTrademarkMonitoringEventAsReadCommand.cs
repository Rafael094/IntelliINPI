using FluentValidation;
using IntelliINPI.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Application.Monitoring;

public sealed record MarkTrademarkMonitoringEventAsReadCommand(Guid Id) : IRequest;

public sealed class MarkTrademarkMonitoringEventAsReadCommandValidator : AbstractValidator<MarkTrademarkMonitoringEventAsReadCommand>
{
    public MarkTrademarkMonitoringEventAsReadCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public sealed class MarkTrademarkMonitoringEventAsReadCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<MarkTrademarkMonitoringEventAsReadCommand>
{
    public async Task Handle(MarkTrademarkMonitoringEventAsReadCommand request, CancellationToken cancellationToken)
    {
        var monitoringEvent = await dbContext.TrademarkMonitoringEvents
            .Include(x => x.MonitoredTrademark)
            .SingleOrDefaultAsync(
                x => x.Id == request.Id && x.MonitoredTrademark.UserId == currentUser.UserId,
                cancellationToken);

        if (monitoringEvent is null)
        {
            return;
        }

        monitoringEvent.IsRead = true;

        var hasUnreadEvents = await dbContext.TrademarkMonitoringEvents
            .AnyAsync(
                x => x.MonitoredTrademarkId == monitoringEvent.MonitoredTrademarkId
                    && x.Id != monitoringEvent.Id
                    && !x.IsRead,
                cancellationToken);

        if (!hasUnreadEvents)
        {
            monitoringEvent.MonitoredTrademark.HasPendingChanges = false;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
