using IntelliINPI.Application.Abstractions;
using IntelliINPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Deadline> Deadlines => Set<Deadline>();
    public DbSet<IPAsset> IPAssets => Set<IPAsset>();
    public DbSet<InpiDeadline> InpiDeadlines => Set<InpiDeadline>();
    public DbSet<Patent> Patents => Set<Patent>();
    public DbSet<PatentDispatch> PatentDispatches => Set<PatentDispatch>();
    public DbSet<MonitoredPatent> MonitoredPatents => Set<MonitoredPatent>();
    public DbSet<PatentMonitoringEvent> PatentMonitoringEvents => Set<PatentMonitoringEvent>();
    public DbSet<Trademark> Trademarks => Set<Trademark>();
    public DbSet<TrademarkOwner> TrademarkOwners => Set<TrademarkOwner>();
    public DbSet<TrademarkOwnerLink> TrademarkOwnerLinks => Set<TrademarkOwnerLink>();
    public DbSet<TrademarkNiceClass> TrademarkNiceClasses => Set<TrademarkNiceClass>();
    public DbSet<TrademarkViennaClass> TrademarkViennaClasses => Set<TrademarkViennaClass>();
    public DbSet<TrademarkPetition> TrademarkPetitions => Set<TrademarkPetition>();
    public DbSet<TrademarkStatus> TrademarkStatuses => Set<TrademarkStatus>();
    public DbSet<TrademarkDispatch> TrademarkDispatches => Set<TrademarkDispatch>();
    public DbSet<MonitoredTrademark> MonitoredTrademarks => Set<MonitoredTrademark>();
    public DbSet<TrademarkMonitoringEvent> TrademarkMonitoringEvents => Set<TrademarkMonitoringEvent>();
    public DbSet<ImportJob> ImportJobs => Set<ImportJob>();
    public DbSet<ImportJobLog> ImportJobLogs => Set<ImportJobLog>();
    public DbSet<RpiImportCheckpoint> RpiImportCheckpoints => Set<RpiImportCheckpoint>();
    public DbSet<RpiHistoricalImportRun> RpiHistoricalImportRuns => Set<RpiHistoricalImportRun>();
    public DbSet<University> Universities => Set<University>();
    public DbSet<NitUserProfile> NitUserProfiles => Set<NitUserProfile>();
    public DbSet<Invention> Inventions => Set<Invention>();
    public DbSet<InventionDocument> InventionDocuments => Set<InventionDocument>();
    public DbSet<TechnologyTransferContract> TechnologyTransferContracts => Set<TechnologyTransferContract>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Researcher> Researchers => Set<Researcher>();
    public DbSet<InventionResearcher> InventionResearchers => Set<InventionResearcher>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<RoyaltyPayment> RoyaltyPayments => Set<RoyaltyPayment>();
    public DbSet<TechnologyTransferOpportunity> TechnologyTransferOpportunities => Set<TechnologyTransferOpportunity>();
    public DbSet<NitDocument> NitDocuments => Set<NitDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Email).HasMaxLength(160).IsRequired();
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.Property(x => x.Role).HasMaxLength(40).IsRequired();
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Name);
            entity.HasIndex(x => x.DocumentNumber);
            entity.HasIndex(x => x.IsActive);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.DocumentNumber).HasMaxLength(40);
            entity.Property(x => x.Email).HasMaxLength(160);
            entity.Property(x => x.Phone).HasMaxLength(40);
            entity.Property(x => x.Notes).HasMaxLength(2000);
        });

        modelBuilder.Entity<Deadline>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.DueDate);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.Type);
            entity.HasIndex(x => x.ClientId);
            entity.HasIndex(x => x.TrademarkId);
            entity.HasIndex(x => x.InventionId);
            entity.HasIndex(x => x.IsActive);
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.Property(x => x.Status).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Type).HasMaxLength(80).IsRequired();
            entity.HasOne(x => x.Client).WithMany(x => x.Deadlines).HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Trademark).WithMany().HasForeignKey(x => x.TrademarkId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Invention).WithMany().HasForeignKey(x => x.InventionId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<IPAsset>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Type);
            entity.HasIndex(x => x.InpiProcessNumber);
            entity.HasIndex(x => x.ClientId);
            entity.HasIndex(x => x.UniversityId);
            entity.HasIndex(x => x.IsActive);
            entity.HasIndex(x => x.IsMonitored);
            entity.Property(x => x.Type).HasMaxLength(40).IsRequired();
            entity.Property(x => x.InpiProcessNumber).HasMaxLength(80);
            entity.Property(x => x.Title).HasMaxLength(240).IsRequired();
            entity.Property(x => x.OwnerName).HasMaxLength(240);
            entity.Property(x => x.Status).HasMaxLength(80).IsRequired();
            entity.HasOne(x => x.Client).WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.University).WithMany().HasForeignKey(x => x.UniversityId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<InpiDeadline>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.IPAssetId);
            entity.HasIndex(x => x.Type);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.DueDate);
            entity.HasIndex(x => x.IsInternal);
            entity.Property(x => x.Type).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Source).HasMaxLength(80).IsRequired();
            entity.Property(x => x.SourceDispatchCode).HasMaxLength(40);
            entity.Property(x => x.LegalBasis).HasMaxLength(1000);
            entity.Property(x => x.Status).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(2000);
            entity.HasOne(x => x.IPAsset).WithMany().HasForeignKey(x => x.IPAssetId);
        });

        modelBuilder.Entity<Patent>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.InpiProcessNumber).IsUnique();
            entity.HasIndex(x => x.Title);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.IsActive);
            entity.Property(x => x.InpiProcessNumber).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(240).IsRequired();
            entity.Property(x => x.Abstract).HasMaxLength(4000);
            entity.Property(x => x.Applicants).HasMaxLength(2000);
            entity.Property(x => x.Inventors).HasMaxLength(2000);
            entity.Property(x => x.IpcClass).HasMaxLength(120);
            entity.Property(x => x.Status).HasMaxLength(80);
        });

        modelBuilder.Entity<PatentDispatch>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.PatentId);
            entity.HasIndex(x => new { x.PatentId, x.RpiNumber, x.DispatchCode }).IsUnique();
            entity.Property(x => x.DispatchCode).HasMaxLength(40).IsRequired();
            entity.Property(x => x.DispatchDescription).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.Complement).HasMaxLength(2000);
            entity.HasOne(x => x.Patent).WithMany(x => x.Dispatches).HasForeignKey(x => x.PatentId);
        });

        modelBuilder.Entity<MonitoredPatent>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.PatentId);
            entity.HasIndex(x => x.InpiProcessNumber);
            entity.HasIndex(x => new { x.UserId, x.PatentId }).IsUnique();
            entity.Property(x => x.InpiProcessNumber).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.LastKnownDispatchCode).HasMaxLength(40);
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
            entity.HasOne(x => x.Patent).WithMany(x => x.Monitors).HasForeignKey(x => x.PatentId);
            entity.HasOne(x => x.LastKnownDispatch).WithMany().HasForeignKey(x => x.LastKnownDispatchId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PatentMonitoringEvent>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.MonitoredPatentId);
            entity.HasIndex(x => x.DispatchId);
            entity.HasIndex(x => x.IsRead);
            entity.Property(x => x.InpiProcessNumber).HasMaxLength(80).IsRequired();
            entity.Property(x => x.EventType).HasMaxLength(80).IsRequired();
            entity.Property(x => x.PreviousDispatchCode).HasMaxLength(40);
            entity.Property(x => x.CurrentDispatchCode).HasMaxLength(40);
            entity.HasOne(x => x.MonitoredPatent).WithMany(x => x.Events).HasForeignKey(x => x.MonitoredPatentId);
            entity.HasOne(x => x.Patent).WithMany().HasForeignKey(x => x.PatentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Dispatch).WithMany().HasForeignKey(x => x.DispatchId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Trademark>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Name);
            entity.HasIndex(x => x.ProcessNumber).IsUnique();
            entity.HasIndex(x => x.StatusId);
            entity.Property(x => x.ProcessNumber).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Presentation).HasMaxLength(80);
            entity.Property(x => x.Nature).HasMaxLength(120);
            entity.Property(x => x.LegalRepresentative).HasMaxLength(200);
            entity.Property(x => x.InpiDetailUrl).HasMaxLength(500);
            entity.Property(x => x.LogoPath).HasMaxLength(500);
            entity.Property(x => x.LogoContentType).HasMaxLength(80);
            entity.HasOne(x => x.Owner).WithMany(x => x.Trademarks).HasForeignKey(x => x.OwnerId);
            entity.HasOne(x => x.Status).WithMany(x => x.Trademarks).HasForeignKey(x => x.StatusId);
        });

        modelBuilder.Entity<TrademarkOwner>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Name);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Document).HasMaxLength(40);
        });

        modelBuilder.Entity<TrademarkOwnerLink>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.TrademarkId, x.OwnerId }).IsUnique();
            entity.HasOne(x => x.Trademark).WithMany(x => x.OwnerLinks).HasForeignKey(x => x.TrademarkId);
            entity.HasOne(x => x.Owner).WithMany(x => x.TrademarkLinks).HasForeignKey(x => x.OwnerId);
        });

        modelBuilder.Entity<TrademarkStatus>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<TrademarkNiceClass>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Code);
            entity.HasIndex(x => new { x.TrademarkId, x.Code }).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Specification).HasMaxLength(1000);
            entity.HasOne(x => x.Trademark).WithMany(x => x.NiceClasses).HasForeignKey(x => x.TrademarkId);
        });

        modelBuilder.Entity<TrademarkViennaClass>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Code);
            entity.HasIndex(x => new { x.TrademarkId, x.Edition, x.Code }).IsUnique();
            entity.Property(x => x.Edition).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(300);
            entity.HasOne(x => x.Trademark).WithMany(x => x.ViennaClasses).HasForeignKey(x => x.TrademarkId);
        });

        modelBuilder.Entity<TrademarkPetition>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Protocol);
            entity.HasIndex(x => new { x.TrademarkId, x.Protocol }).IsUnique();
            entity.Property(x => x.Protocol).HasMaxLength(80).IsRequired();
            entity.Property(x => x.ServiceCode).HasMaxLength(40);
            entity.Property(x => x.ClientName).HasMaxLength(200);
            entity.Property(x => x.Delivery).HasMaxLength(80);
            entity.HasOne(x => x.Trademark).WithMany(x => x.Petitions).HasForeignKey(x => x.TrademarkId);
        });

        modelBuilder.Entity<TrademarkDispatch>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.TrademarkId, x.RpiNumber, x.Code }).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            entity.HasOne(x => x.Trademark).WithMany(x => x.Dispatches).HasForeignKey(x => x.TrademarkId);
        });

        modelBuilder.Entity<MonitoredTrademark>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.TrademarkId);
            entity.HasIndex(x => x.ProcessNumber);
            entity.HasIndex(x => new { x.UserId, x.TrademarkId }).IsUnique();
            entity.Property(x => x.ProcessNumber).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.LastKnownDispatchCode).HasMaxLength(40);
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
            entity.HasOne(x => x.Trademark).WithMany(x => x.Monitors).HasForeignKey(x => x.TrademarkId);
            entity.HasOne(x => x.LastKnownDispatch).WithMany().HasForeignKey(x => x.LastKnownDispatchId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TrademarkMonitoringEvent>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.MonitoredTrademarkId);
            entity.HasIndex(x => x.DispatchId);
            entity.HasIndex(x => x.IsRead);
            entity.Property(x => x.ProcessNumber).HasMaxLength(40).IsRequired();
            entity.Property(x => x.EventType).HasMaxLength(80).IsRequired();
            entity.Property(x => x.PreviousDispatchCode).HasMaxLength(40);
            entity.Property(x => x.CurrentDispatchCode).HasMaxLength(40);
            entity.HasOne(x => x.MonitoredTrademark).WithMany(x => x.Events).HasForeignKey(x => x.MonitoredTrademarkId);
            entity.HasOne(x => x.Trademark).WithMany().HasForeignKey(x => x.TrademarkId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Dispatch).WithMany().HasForeignKey(x => x.DispatchId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ImportJob>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Source).HasMaxLength(120).IsRequired();
            entity.Property(x => x.SourceType).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(40).IsRequired();
        });

        modelBuilder.Entity<ImportJobLog>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Message).HasMaxLength(2000).IsRequired();
            entity.HasOne(x => x.ImportJob).WithMany(x => x.Logs).HasForeignKey(x => x.ImportJobId);
        });

        modelBuilder.Entity<RpiImportCheckpoint>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.RpiNumber).IsUnique();
            entity.HasIndex(x => x.Status);
            entity.Property(x => x.Status).HasMaxLength(40).IsRequired();
            entity.Property(x => x.ErrorMessage).HasMaxLength(2000);
            entity.Property(x => x.ZipPath).HasMaxLength(1000);
        });

        modelBuilder.Entity<RpiHistoricalImportRun>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Status);
            entity.Property(x => x.Status).HasMaxLength(40).IsRequired();
            entity.Property(x => x.LastErrorSummary).HasMaxLength(2000);
            entity.Property(x => x.ErrorMessage).HasMaxLength(2000);
        });

        modelBuilder.Entity<University>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Cnpj);
            entity.HasIndex(x => x.IsActive);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Cnpj).HasMaxLength(20);
            entity.Property(x => x.Tier).HasMaxLength(40).IsRequired();
            entity.Property(x => x.TradeName).HasMaxLength(200);
            entity.Property(x => x.Type).HasMaxLength(60).IsRequired();
            entity.Property(x => x.Website).HasMaxLength(300);
            entity.Property(x => x.Email).HasMaxLength(200);
            entity.Property(x => x.Phone).HasMaxLength(40);
            entity.Property(x => x.ContactName).HasMaxLength(200);
            entity.Property(x => x.Status).HasMaxLength(40).IsRequired();
        });

        modelBuilder.Entity<NitUserProfile>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.UniversityId);
            entity.HasIndex(x => new { x.UserId, x.UniversityId }).IsUnique();
            entity.Property(x => x.Role).HasMaxLength(60).IsRequired();
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
            entity.HasOne(x => x.University).WithMany(x => x.NitUserProfiles).HasForeignKey(x => x.UniversityId);
        });

        modelBuilder.Entity<Invention>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.UniversityId);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.IsActive);
            entity.HasIndex(x => x.InpiProcessNumber);
            entity.Property(x => x.Title).HasMaxLength(240).IsRequired();
            entity.Property(x => x.Summary).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.Inventors).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(40).IsRequired();
            entity.Property(x => x.PatentNumber).HasMaxLength(80);
            entity.Property(x => x.InpiProcessNumber).HasMaxLength(80);
            entity.Property(x => x.ExecutiveSummary).HasMaxLength(4000);
            entity.Property(x => x.TechnicalDescription).HasMaxLength(12000);
            entity.Property(x => x.TechnologyArea).HasMaxLength(160);
            entity.Property(x => x.CommercialPotential).HasMaxLength(2000);
            entity.Property(x => x.TargetMarket).HasMaxLength(2000);
            entity.Property(x => x.ProtectionStatus).HasMaxLength(80);
            entity.Property(x => x.Responsible).HasMaxLength(200);
            entity.HasOne(x => x.University).WithMany(x => x.Inventions).HasForeignKey(x => x.UniversityId);
        });

        modelBuilder.Entity<InventionDocument>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.InventionId);
            entity.HasIndex(x => x.FileHash);
            entity.Property(x => x.Type).HasMaxLength(80).IsRequired();
            entity.Property(x => x.FileName).HasMaxLength(260).IsRequired();
            entity.Property(x => x.FileHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.StoragePath).HasMaxLength(1000).IsRequired();
            entity.HasOne(x => x.Invention).WithMany(x => x.Documents).HasForeignKey(x => x.InventionId);
        });

        modelBuilder.Entity<TechnologyTransferContract>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.InventionId);
            entity.HasIndex(x => x.Status);
            entity.Property(x => x.CompanyName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Cnpj).HasMaxLength(20);
            entity.Property(x => x.RoyaltyModel).HasMaxLength(120).IsRequired();
            entity.Property(x => x.RoyaltyValue).HasPrecision(18, 4);
            entity.Property(x => x.MinimumGuarantee).HasPrecision(18, 2);
            entity.Property(x => x.Status).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Number).HasMaxLength(80);
            entity.Property(x => x.Type).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Term).HasMaxLength(200);
            entity.Property(x => x.RoyaltyPercentage).HasPrecision(8, 4);
            entity.Property(x => x.FixedValue).HasPrecision(18, 2);
            entity.HasOne(x => x.Invention).WithMany(x => x.TechnologyTransferContracts).HasForeignKey(x => x.InventionId);
            entity.HasOne(x => x.University).WithMany().HasForeignKey(x => x.UniversityId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Company).WithMany(x => x.Contracts).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.UniversityId);
            entity.HasIndex(x => new { x.Module, x.EntityName, x.EntityId });
            entity.Property(x => x.Module).HasMaxLength(80).IsRequired();
            entity.Property(x => x.EntityName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Action).HasMaxLength(80).IsRequired();
            entity.Property(x => x.IpAddress).HasMaxLength(80);
            entity.Property(x => x.PreviousValue).HasColumnType("text");
            entity.Property(x => x.NewValue).HasColumnType("text");
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
            entity.HasOne(x => x.University).WithMany().HasForeignKey(x => x.UniversityId);
        });

        modelBuilder.Entity<Researcher>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.UniversityId);
            entity.HasIndex(x => x.Name);
            entity.HasIndex(x => x.Cpf);
            entity.HasIndex(x => x.IsActive);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Cpf).HasMaxLength(20);
            entity.Property(x => x.Email).HasMaxLength(200);
            entity.Property(x => x.Phone).HasMaxLength(40);
            entity.Property(x => x.Department).HasMaxLength(160);
            entity.Property(x => x.Position).HasMaxLength(120);
            entity.Property(x => x.LattesUrl).HasMaxLength(500);
            entity.Property(x => x.Orcid).HasMaxLength(40);
            entity.Property(x => x.Specialties).HasMaxLength(2000);
            entity.Property(x => x.TechnologyAreas).HasMaxLength(2000);
            entity.HasOne(x => x.University).WithMany(x => x.Researchers).HasForeignKey(x => x.UniversityId);
        });

        modelBuilder.Entity<InventionResearcher>(entity =>
        {
            entity.HasKey(x => new { x.InventionId, x.ResearcherId });
            entity.HasOne(x => x.Invention).WithMany(x => x.Researchers).HasForeignKey(x => x.InventionId);
            entity.HasOne(x => x.Researcher).WithMany(x => x.Inventions).HasForeignKey(x => x.ResearcherId);
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.LegalName);
            entity.HasIndex(x => x.Cnpj);
            entity.HasIndex(x => x.IsActive);
            entity.Property(x => x.LegalName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.TradeName).HasMaxLength(200);
            entity.Property(x => x.Cnpj).HasMaxLength(20);
            entity.Property(x => x.Segment).HasMaxLength(120);
            entity.Property(x => x.Size).HasMaxLength(40);
            entity.Property(x => x.ContactName).HasMaxLength(200);
            entity.Property(x => x.Email).HasMaxLength(200);
            entity.Property(x => x.Phone).HasMaxLength(40);
            entity.Property(x => x.Website).HasMaxLength(300);
            entity.Property(x => x.Notes).HasMaxLength(2000);
            entity.HasOne(x => x.University).WithMany(x => x.Companies).HasForeignKey(x => x.UniversityId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RoyaltyPayment>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.ContractId);
            entity.HasIndex(x => x.Competence);
            entity.HasIndex(x => x.ReceivedAt);
            entity.Property(x => x.AmountReceived).HasPrecision(18, 2);
            entity.Property(x => x.Notes).HasMaxLength(2000);
            entity.HasOne(x => x.Contract).WithMany(x => x.RoyaltyPayments).HasForeignKey(x => x.ContractId);
        });

        modelBuilder.Entity<TechnologyTransferOpportunity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Stage);
            entity.HasIndex(x => x.UniversityId);
            entity.HasIndex(x => x.IsActive);
            entity.Property(x => x.Stage).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(2000);
            entity.HasOne(x => x.Invention).WithMany(x => x.TransferOpportunities).HasForeignKey(x => x.InventionId);
            entity.HasOne(x => x.University).WithMany().HasForeignKey(x => x.UniversityId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Company).WithMany(x => x.TransferOpportunities).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<NitDocument>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.UniversityId);
            entity.HasIndex(x => x.Type);
            entity.HasIndex(x => x.UploadedAtUtc);
            entity.Property(x => x.Name).HasMaxLength(240).IsRequired();
            entity.Property(x => x.Type).HasMaxLength(80).IsRequired();
            entity.Property(x => x.FileName).HasMaxLength(260).IsRequired();
            entity.Property(x => x.OriginalFileName).HasMaxLength(260).IsRequired();
            entity.Property(x => x.StoredFileName).HasMaxLength(260).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(120).IsRequired();
            entity.Property(x => x.EncryptionAlgorithm).HasMaxLength(80);
            entity.Property(x => x.EncryptionIV).HasMaxLength(128);
            entity.Property(x => x.StoragePath).HasMaxLength(1000).IsRequired();
            entity.HasOne(x => x.University).WithMany().HasForeignKey(x => x.UniversityId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Invention).WithMany().HasForeignKey(x => x.InventionId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Contract).WithMany().HasForeignKey(x => x.ContractId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.UploadedByUser).WithMany().HasForeignKey(x => x.UploadedByUserId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
