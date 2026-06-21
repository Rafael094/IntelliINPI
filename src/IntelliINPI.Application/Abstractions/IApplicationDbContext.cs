using IntelliINPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Client> Clients { get; }
    DbSet<Deadline> Deadlines { get; }
    DbSet<IPAsset> IPAssets { get; }
    DbSet<InpiDeadline> InpiDeadlines { get; }
    DbSet<Patent> Patents { get; }
    DbSet<PatentDispatch> PatentDispatches { get; }
    DbSet<MonitoredPatent> MonitoredPatents { get; }
    DbSet<PatentMonitoringEvent> PatentMonitoringEvents { get; }
    DbSet<Trademark> Trademarks { get; }
    DbSet<TrademarkOwner> TrademarkOwners { get; }
    DbSet<TrademarkOwnerLink> TrademarkOwnerLinks { get; }
    DbSet<TrademarkNiceClass> TrademarkNiceClasses { get; }
    DbSet<TrademarkViennaClass> TrademarkViennaClasses { get; }
    DbSet<TrademarkPetition> TrademarkPetitions { get; }
    DbSet<TrademarkStatus> TrademarkStatuses { get; }
    DbSet<TrademarkDispatch> TrademarkDispatches { get; }
    DbSet<MonitoredTrademark> MonitoredTrademarks { get; }
    DbSet<TrademarkMonitoringEvent> TrademarkMonitoringEvents { get; }
    DbSet<ImportJob> ImportJobs { get; }
    DbSet<ImportJobLog> ImportJobLogs { get; }
    DbSet<RpiImportCheckpoint> RpiImportCheckpoints { get; }
    DbSet<RpiHistoricalImportRun> RpiHistoricalImportRuns { get; }
    DbSet<University> Universities { get; }
    DbSet<NitUserProfile> NitUserProfiles { get; }
    DbSet<Invention> Inventions { get; }
    DbSet<InventionDocument> InventionDocuments { get; }
    DbSet<TechnologyTransferContract> TechnologyTransferContracts { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Researcher> Researchers { get; }
    DbSet<InventionResearcher> InventionResearchers { get; }
    DbSet<Company> Companies { get; }
    DbSet<RoyaltyPayment> RoyaltyPayments { get; }
    DbSet<TechnologyTransferOpportunity> TechnologyTransferOpportunities { get; }
    DbSet<NitDocument> NitDocuments { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
