using IntelliINPI.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Application.Nit;

public sealed record NitDashboardOverviewDto(
    int TotalInventions,
    int TotalDrafts,
    int TotalSubmittedToNit,
    int TotalFiledAtInpi,
    int TotalGranted,
    int TotalLicensed,
    int TotalContracts,
    decimal TotalRoyalties,
    string MaturityLevel,
    int TotalInstitutions,
    int TotalResearchers,
    int TotalCompanies,
    int TotalLicensedTechnologies,
    IReadOnlyList<NitChartItemDto> InventionsByStatus,
    IReadOnlyList<NitChartItemDto> ContractsByType,
    IReadOnlyList<NitChartItemDto> RoyaltiesByPeriod,
    IReadOnlyList<NitChartItemDto> TransferPipeline);

public sealed record NitChartItemDto(string Label, decimal Value);

public sealed record GetNitDashboardOverviewQuery : IRequest<NitDashboardOverviewDto>;

public sealed class GetNitDashboardOverviewQueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<GetNitDashboardOverviewQuery, NitDashboardOverviewDto>
{
    public async Task<NitDashboardOverviewDto> Handle(GetNitDashboardOverviewQuery request, CancellationToken cancellationToken)
    {
        var access = await NitAccessContext.LoadAsync(dbContext, currentUser.UserId, cancellationToken);
        var inventions = dbContext.Inventions.AsNoTracking().Where(x => x.IsActive);
        var contracts = dbContext.TechnologyTransferContracts.AsNoTracking().Where(x => x.Invention.IsActive);

        if (!access.IsGlobalAdmin)
        {
            inventions = inventions.Where(x => access.UniversityIds.Contains(x.UniversityId));
            contracts = contracts.Where(x => access.UniversityIds.Contains(x.Invention.UniversityId));
        }

        var totalInventions = await inventions.CountAsync(cancellationToken);
        var totalDrafts = await inventions.CountAsync(x => x.Status == "Draft", cancellationToken);
        var totalSubmittedToNit = await inventions.CountAsync(x => x.Status == "SubmittedToNit", cancellationToken);
        var totalFiledAtInpi = await inventions.CountAsync(x => x.Status == "FiledAtInpi", cancellationToken);
        var totalGranted = await inventions.CountAsync(x => x.Status == "Granted", cancellationToken);
        var totalLicensed = await inventions.CountAsync(x => x.Status == "Licensed", cancellationToken);
        var totalContracts = await contracts.CountAsync(cancellationToken);
        var totalRoyalties = await dbContext.RoyaltyPayments.AsNoTracking().Where(x => x.IsActive).SumAsync(x => x.AmountReceived, cancellationToken);
        var inventionsByStatus = await inventions.GroupBy(x => x.Status).Select(x => new NitChartItemDto(x.Key, x.Count())).ToListAsync(cancellationToken);
        var contractsByType = await contracts.GroupBy(x => x.Type).Select(x => new NitChartItemDto(x.Key, x.Count())).ToListAsync(cancellationToken);
        var royaltiesByPeriodData = await dbContext.RoyaltyPayments.AsNoTracking().Where(x => x.IsActive)
            .GroupBy(x => new { x.ReceivedAt.Year, x.ReceivedAt.Month })
            .Select(x => new { x.Key.Year, x.Key.Month, Total = x.Sum(y => y.AmountReceived) })
            .OrderBy(x => x.Year).ThenBy(x => x.Month).Take(12)
            .ToListAsync(cancellationToken);
        var royaltiesByPeriod = royaltiesByPeriodData
            .Select(x => new NitChartItemDto($"{x.Month:D2}/{x.Year}", x.Total))
            .ToList();
        var transferPipeline = await dbContext.TechnologyTransferOpportunities.AsNoTracking().Where(x => x.IsActive)
            .GroupBy(x => x.Stage).Select(x => new NitChartItemDto(x.Key, x.Count())).ToListAsync(cancellationToken);

        return new NitDashboardOverviewDto(
            totalInventions,
            totalDrafts,
            totalSubmittedToNit,
            totalFiledAtInpi,
            totalGranted,
            totalLicensed,
            totalContracts,
            totalRoyalties,
            ResolveMaturityLevel(totalInventions),
            await dbContext.Universities.CountAsync(x => x.IsActive, cancellationToken),
            await dbContext.Researchers.CountAsync(x => x.IsActive, cancellationToken),
            await dbContext.Companies.CountAsync(x => x.IsActive, cancellationToken),
            await inventions.CountAsync(x => x.Status == "Licenciada" || x.Status == "Licensed", cancellationToken),
            inventionsByStatus,
            contractsByType,
            royaltiesByPeriod,
            transferPipeline);
    }

    private static string ResolveMaturityLevel(int totalInventions)
    {
        return totalInventions switch
        {
            <= 10 => "Nascente",
            <= 50 => "Intermediario",
            _ => "Consolidado"
        };
    }
}
