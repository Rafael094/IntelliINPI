using FluentValidation;
using IntelliINPI.Application.Abstractions;
using IntelliINPI.Application.Common.Exceptions;
using IntelliINPI.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Application.Monitoring;

public sealed record AddMonitoredTrademarkRequest(Guid? TrademarkId, string? ProcessNumber, string? Notes);
public sealed record AddMonitoredTrademarkCommand(Guid? TrademarkId, string? ProcessNumber, string? Notes) : IRequest<Guid>;

public sealed class AddMonitoredTrademarkCommandValidator : AbstractValidator<AddMonitoredTrademarkCommand>
{
    public AddMonitoredTrademarkCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => x.TrademarkId.HasValue || !string.IsNullOrWhiteSpace(x.ProcessNumber))
            .WithMessage("Informe trademarkId ou processNumber.");

        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public sealed class AddMonitoredTrademarkCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<AddMonitoredTrademarkCommand, Guid>
{
    public async Task<Guid> Handle(AddMonitoredTrademarkCommand request, CancellationToken cancellationToken)
    {
        var processNumber = request.ProcessNumber?.Trim();
        var trademark = request.TrademarkId.HasValue
            ? await dbContext.Trademarks
                .AsNoTracking()
                .Include(x => x.Owner)
                .Include(x => x.Status)
                .SingleOrDefaultAsync(x => x.Id == request.TrademarkId.Value, cancellationToken)
            : await dbContext.Trademarks
                .AsNoTracking()
                .Include(x => x.Owner)
                .Include(x => x.Status)
                .SingleOrDefaultAsync(x => x.ProcessNumber == processNumber, cancellationToken);

        if (trademark is null)
        {
            throw new NotFoundException("Marca nao encontrada na base local.");
        }

        var existingMonitor = await dbContext.MonitoredTrademarks
            .SingleOrDefaultAsync(x => x.TrademarkId == trademark.Id && x.UserId == currentUser.UserId, cancellationToken);

        if (existingMonitor is not null)
        {
            existingMonitor.IsActive = true;
            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                existingMonitor.Notes = request.Notes.Trim();
            }

            await MonitoringIPAssetSync.SyncTrademarkAsync(dbContext, trademark, true, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return existingMonitor.Id;
        }

        var latestDispatch = await dbContext.TrademarkDispatches
            .AsNoTracking()
            .Where(x => x.TrademarkId == trademark.Id)
            .OrderByDescending(x => x.PublishedAt)
            .ThenByDescending(x => x.RpiNumber ?? 0)
            .ThenByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var monitoredTrademark = new MonitoredTrademark
        {
            Id = Guid.NewGuid(),
            TrademarkId = trademark.Id,
            UserId = currentUser.UserId,
            ProcessNumber = trademark.ProcessNumber,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            IsActive = true,
            CreatedAtUtc = now,
            LastCheckedAtUtc = now,
            LastKnownDispatchId = latestDispatch?.Id,
            LastKnownDispatchCode = latestDispatch?.Code,
            LastKnownDispatchDate = latestDispatch?.PublishedAt,
            HasPendingChanges = false
        };

        dbContext.MonitoredTrademarks.Add(monitoredTrademark);
        await MonitoringIPAssetSync.SyncTrademarkAsync(dbContext, trademark, true, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return monitoredTrademark.Id;
    }
}
