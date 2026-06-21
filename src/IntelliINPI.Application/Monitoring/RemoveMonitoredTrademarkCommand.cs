using FluentValidation;
using IntelliINPI.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Application.Monitoring;

public sealed record RemoveMonitoredTrademarkCommand(Guid Id) : IRequest;

public sealed class RemoveMonitoredTrademarkCommandValidator : AbstractValidator<RemoveMonitoredTrademarkCommand>
{
    public RemoveMonitoredTrademarkCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public sealed class RemoveMonitoredTrademarkCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<RemoveMonitoredTrademarkCommand>
{
    public async Task Handle(RemoveMonitoredTrademarkCommand request, CancellationToken cancellationToken)
    {
        var monitoredTrademark = await dbContext.MonitoredTrademarks
            .Include(x => x.Trademark)
            .ThenInclude(x => x.Owner)
            .Include(x => x.Trademark)
            .ThenInclude(x => x.Status)
            .SingleOrDefaultAsync(x => x.Id == request.Id && x.UserId == currentUser.UserId, cancellationToken);

        if (monitoredTrademark is null)
        {
            return;
        }

        var trademark = monitoredTrademark.Trademark;
        var hasOtherActiveMonitors = await dbContext.MonitoredTrademarks
            .AnyAsync(x => x.TrademarkId == trademark.Id && x.Id != monitoredTrademark.Id && x.IsActive, cancellationToken);

        dbContext.MonitoredTrademarks.Remove(monitoredTrademark);
        await MonitoringIPAssetSync.SyncTrademarkAsync(dbContext, trademark, hasOtherActiveMonitors, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
