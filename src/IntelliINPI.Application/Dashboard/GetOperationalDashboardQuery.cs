using IntelliINPI.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Application.Dashboard;

public sealed record OperationalDashboardDto(
    int TotalMonitoredIPAssets,
    int TotalMonitoredTrademarks,
    int TotalActiveMonitoredTrademarks,
    int TotalMonitoredPatents,
    int TotalActiveMonitoredPatents,
    int TotalPendingChanges,
    int TotalUnreadEvents,
    DateTime? LastMonitoringCheckAtUtc,
    IReadOnlyList<OperationalDashboardDeadlineDto> UpcomingInpiDeadlines,
    IReadOnlyList<OperationalDashboardDeadlineDto> UpcomingInternalDeadlines,
    IReadOnlyList<OperationalDashboardDispatchDto> LatestDispatches,
    int? LastImportedRpiNumber,
    string? LastRpiImportStatus,
    DateTime? LastRpiImportDateUtc,
    string? HistoricalImportStatus,
    int? HistoricalImportCurrentRpi,
    decimal? HistoricalImportPercentage,
    IReadOnlyList<string> InpiSyncFailures,
    IReadOnlyList<OperationalDashboardEventDto> RecentMonitoringEvents,
    IReadOnlyList<OperationalDashboardPendingTrademarkDto> MonitoredTrademarksWithPendingChanges);

public sealed record OperationalDashboardEventDto(
    Guid Id,
    string ProcessNumber,
    string TrademarkName,
    string? PreviousDispatchCode,
    string? CurrentDispatchCode,
    DateOnly? CurrentDispatchDate,
    DateTime CreatedAtUtc,
    bool IsRead);

public sealed record OperationalDashboardPendingTrademarkDto(
    Guid Id,
    Guid TrademarkId,
    string ProcessNumber,
    string TrademarkName,
    string? LastKnownDispatchCode,
    DateOnly? LastKnownDispatchDate,
    DateTime? LastCheckedAtUtc);

public sealed record OperationalDashboardDeadlineDto(
    Guid Id,
    Guid IPAssetId,
    string IPAssetType,
    string IPAssetTitle,
    string? InpiProcessNumber,
    string Type,
    DateOnly? DueDate,
    string Status,
    bool IsInternal,
    string? Notes);

public sealed record OperationalDashboardDispatchDto(
    string AssetType,
    string ProcessNumber,
    string Title,
    string DispatchCode,
    DateOnly? DispatchDate,
    int? RpiNumber);

public sealed record GetOperationalDashboardQuery : IRequest<OperationalDashboardDto>;

public sealed class GetOperationalDashboardQueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<GetOperationalDashboardQuery, OperationalDashboardDto>
{
    public async Task<OperationalDashboardDto> Handle(GetOperationalDashboardQuery request, CancellationToken cancellationToken)
    {
        var monitoredQuery = dbContext.MonitoredTrademarks
            .AsNoTracking()
            .Where(x => x.UserId == currentUser.UserId);

        var totalMonitored = await monitoredQuery.CountAsync(cancellationToken);
        var totalActive = await monitoredQuery.CountAsync(x => x.IsActive, cancellationToken);
        var patentMonitoredQuery = dbContext.MonitoredPatents
            .AsNoTracking()
            .Where(x => x.UserId == currentUser.UserId);

        var totalPatentsMonitored = await patentMonitoredQuery.CountAsync(cancellationToken);
        var totalActivePatents = await patentMonitoredQuery.CountAsync(x => x.IsActive, cancellationToken);
        var totalPending = await monitoredQuery.CountAsync(x => x.HasPendingChanges, cancellationToken)
            + await patentMonitoredQuery.CountAsync(x => x.HasPendingChanges, cancellationToken);
        var lastTrademarkCheckAt = await monitoredQuery.MaxAsync(x => (DateTime?)x.LastCheckedAtUtc, cancellationToken);
        var lastPatentCheckAt = await patentMonitoredQuery.MaxAsync(x => (DateTime?)x.LastCheckedAtUtc, cancellationToken);
        var lastCheckAt = new[] { lastTrademarkCheckAt, lastPatentCheckAt }.Where(x => x.HasValue).Max();

        var totalUnreadEvents = await dbContext.TrademarkMonitoringEvents
            .AsNoTracking()
            .CountAsync(x => x.MonitoredTrademark.UserId == currentUser.UserId && !x.IsRead, cancellationToken)
            + await dbContext.PatentMonitoringEvents
                .AsNoTracking()
                .CountAsync(x => x.MonitoredPatent.UserId == currentUser.UserId && !x.IsRead, cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var deadlineLimit = today.AddDays(60);
        var inpiDeadlines = await dbContext.InpiDeadlines
            .AsNoTracking()
            .Where(x => !x.IsInternal && (x.DueDate == null || x.DueDate <= deadlineLimit))
            .OrderBy(x => x.DueDate == null)
            .ThenBy(x => x.DueDate)
            .Take(10)
            .Select(x => new OperationalDashboardDeadlineDto(
                x.Id,
                x.IPAssetId,
                x.IPAsset.Type,
                x.IPAsset.Title,
                x.IPAsset.InpiProcessNumber,
                x.Type,
                x.DueDate,
                x.Status,
                x.IsInternal,
                x.Notes))
            .ToListAsync(cancellationToken);

        var internalDeadlines = await dbContext.InpiDeadlines
            .AsNoTracking()
            .Where(x => x.IsInternal && (x.DueDate == null || x.DueDate <= deadlineLimit))
            .OrderBy(x => x.DueDate == null)
            .ThenBy(x => x.DueDate)
            .Take(10)
            .Select(x => new OperationalDashboardDeadlineDto(
                x.Id,
                x.IPAssetId,
                x.IPAsset.Type,
                x.IPAsset.Title,
                x.IPAsset.InpiProcessNumber,
                x.Type,
                x.DueDate,
                x.Status,
                x.IsInternal,
                x.Notes))
            .ToListAsync(cancellationToken);

        var lastRpi = await dbContext.RpiImportCheckpoints
            .AsNoTracking()
            .Where(x => x.FinishedAtUtc != null)
            .OrderByDescending(x => x.RpiNumber)
            .Select(x => new
            {
                x.RpiNumber,
                x.Status,
                x.FinishedAtUtc
            })
            .FirstOrDefaultAsync(cancellationToken);

        var historicalRun = await dbContext.RpiHistoricalImportRuns
            .AsNoTracking()
            .OrderByDescending(x => x.StartedAtUtc)
            .Select(x => new
            {
                x.Status,
                x.CurrentRpi,
                x.TotalRpis,
                x.SuccessfulRpis,
                x.FailedRpis,
                x.SkippedRpis
            })
            .FirstOrDefaultAsync(cancellationToken);

        var historicalPercentage = historicalRun is null || historicalRun.TotalRpis == 0
            ? (decimal?)null
            : Math.Round(
                Math.Clamp(historicalRun.SuccessfulRpis + historicalRun.FailedRpis + historicalRun.SkippedRpis, 0, historicalRun.TotalRpis) * 100m / historicalRun.TotalRpis,
                2);

        var recentEvents = await dbContext.TrademarkMonitoringEvents
            .AsNoTracking()
            .Where(x => x.MonitoredTrademark.UserId == currentUser.UserId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(10)
            .Select(x => new OperationalDashboardEventDto(
                x.Id,
                x.ProcessNumber,
                x.Trademark.Name,
                x.PreviousDispatchCode,
                x.CurrentDispatchCode,
                x.CurrentDispatchDate,
                x.CreatedAtUtc,
                x.IsRead))
            .ToListAsync(cancellationToken);

        var latestTrademarkDispatches = await dbContext.TrademarkDispatches
            .AsNoTracking()
            .OrderByDescending(x => x.PublishedAt)
            .ThenByDescending(x => x.RpiNumber ?? 0)
            .Take(5)
            .Select(x => new OperationalDashboardDispatchDto(
                "Trademark",
                x.Trademark.ProcessNumber,
                x.Trademark.Name,
                x.Code,
                x.PublishedAt,
                x.RpiNumber))
            .ToListAsync(cancellationToken);

        var latestPatentDispatches = await dbContext.PatentDispatches
            .AsNoTracking()
            .OrderByDescending(x => x.DispatchDate)
            .ThenByDescending(x => x.RpiNumber ?? 0)
            .Take(5)
            .Select(x => new OperationalDashboardDispatchDto(
                "Patent",
                x.Patent.InpiProcessNumber,
                x.Patent.Title,
                x.DispatchCode,
                x.DispatchDate,
                x.RpiNumber))
            .ToListAsync(cancellationToken);

        var latestDispatches = latestTrademarkDispatches
            .Concat(latestPatentDispatches)
            .OrderByDescending(x => x.DispatchDate)
            .Take(10)
            .ToList();

        var failures = await dbContext.ImportJobs
            .AsNoTracking()
            .Where(x => x.Status == "Failed" || x.Status == "CompletedWithWarnings")
            .OrderByDescending(x => x.StartedAtUtc)
            .Take(5)
            .Select(x => $"{x.Source}: {x.Status}")
            .ToListAsync(cancellationToken);

        var pendingTrademarks = await monitoredQuery
            .Where(x => x.HasPendingChanges)
            .OrderByDescending(x => x.LastCheckedAtUtc)
            .Take(10)
            .Select(x => new OperationalDashboardPendingTrademarkDto(
                x.Id,
                x.TrademarkId,
                x.ProcessNumber,
                x.Trademark.Name,
                x.LastKnownDispatchCode,
                x.LastKnownDispatchDate,
                x.LastCheckedAtUtc))
            .ToListAsync(cancellationToken);

        return new OperationalDashboardDto(
            totalActive + totalActivePatents,
            totalMonitored,
            totalActive,
            totalPatentsMonitored,
            totalActivePatents,
            totalPending,
            totalUnreadEvents,
            lastCheckAt,
            inpiDeadlines,
            internalDeadlines,
            latestDispatches,
            lastRpi?.RpiNumber,
            lastRpi?.Status,
            lastRpi?.FinishedAtUtc,
            historicalRun?.Status,
            historicalRun?.CurrentRpi,
            historicalPercentage,
            failures,
            recentEvents,
            pendingTrademarks);
    }
}
