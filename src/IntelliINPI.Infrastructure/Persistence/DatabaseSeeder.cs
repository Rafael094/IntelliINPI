using IntelliINPI.Application.Abstractions;
using IntelliINPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IntelliINPI.Infrastructure.Persistence;

public sealed class DatabaseSeeder(
    ApplicationDbContext dbContext,
    IPasswordHasher passwordHasher,
    IConfiguration configuration,
    IHostEnvironment environment,
    ILogger<DatabaseSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var adminEmail = configuration["SeedAdmin:Email"];
        var adminPassword = configuration["SeedAdmin:Password"];
        if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
        {
            var normalizedEmail = adminEmail.Trim().ToLowerInvariant();
            var admin = await dbContext.Users.SingleOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
            if (admin is null)
            {
                dbContext.Users.Add(new User
                {
                    Id = Guid.NewGuid(),
                    Email = normalizedEmail,
                    PasswordHash = passwordHasher.Hash(adminPassword),
                    Role = "Admin",
                    CreatedAtUtc = DateTime.UtcNow
                });
            }
            else if (configuration.GetValue("SeedAdmin:ResetPassword", false))
            {
                admin.PasswordHash = passwordHasher.Hash(adminPassword);
                admin.Role = "Admin";
                logger.LogWarning("Admin password hash reset from startup configuration for {Email}. Disable SeedAdmin:ResetPassword after deployment.", normalizedEmail);
            }
        }
        else if (!environment.IsDevelopment())
        {
            logger.LogWarning("Admin seed skipped because SeedAdmin:Email or SeedAdmin:Password is not configured.");
        }

        if (!await dbContext.TrademarkStatuses.AnyAsync(cancellationToken))
        {
            dbContext.TrademarkStatuses.Add(new TrademarkStatus
            {
                Id = Guid.NewGuid(),
                Code = "UNKNOWN",
                Description = "Status não importado"
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        if (configuration.GetValue("SeedNitDemo:Enabled", false))
        {
            await SeedNitDemoAsync(cancellationToken);
        }
    }

    private async Task SeedNitDemoAsync(CancellationToken cancellationToken)
    {
        const string demoCnpj = "00.000.000/0001-00";
        if (await dbContext.Universities.AnyAsync(x => x.Cnpj == demoCnpj, cancellationToken)) return;

        var now = DateTime.UtcNow;
        var institution = new University { Id = Guid.NewGuid(), Name = "Instituto Demo de Inovação", TradeName = "IDI", Cnpj = demoCnpj, Tier = "Intermediário", Type = "ICT", Website = "https://example.local", Email = "nit@example.local", ContactName = "Coordenação do NIT", Status = "Ativa", CreatedAtUtc = now, IsActive = true };
        var researcher = new Researcher { Id = Guid.NewGuid(), UniversityId = institution.Id, Name = "Pesquisador de Demonstração", Email = "pesquisador@example.local", Department = "Engenharia", Position = "Pesquisador", Specialties = "Materiais avançados", TechnologyAreas = "Engenharia; Sustentabilidade", CreatedAtUtc = now, IsActive = true };
        var company = new Company { Id = Guid.NewGuid(), UniversityId = institution.Id, LegalName = "Empresa Parceira Demonstração Ltda.", TradeName = "Parceira Demo", Cnpj = "11.111.111/0001-11", Segment = "Tecnologia", Size = "Pequena", ContactName = "Contato Demonstração", Email = "contato@example.local", CreatedAtUtc = now, IsActive = true };
        var invention = new Invention { Id = Guid.NewGuid(), UniversityId = institution.Id, Title = "Tecnologia sustentável de demonstração", Summary = "Registro demonstrativo para validação do fluxo operacional do NIT.", ExecutiveSummary = "Tecnologia demonstrativa com potencial de aplicação industrial.", TechnicalDescription = "Descrição técnica de demonstração.", TechnologyArea = "Sustentabilidade", Trl = 4, CommercialPotential = "Mercado nacional", TargetMarket = "Indústria", ProtectionStatus = "Em avaliação", CreationDate = DateOnly.FromDateTime(now), Responsible = researcher.Name, Inventors = researcher.Name, Status = "Em Avaliação", CreatedAtUtc = now, IsActive = true };
        var contract = new TechnologyTransferContract { Id = Guid.NewGuid(), InventionId = invention.Id, UniversityId = institution.Id, CompanyId = company.Id, CompanyName = company.TradeName, Cnpj = company.Cnpj, Number = "DEMO-001", Type = "Licenciamento", StartDate = DateOnly.FromDateTime(now.AddMonths(-2)), EndDate = DateOnly.FromDateTime(now.AddYears(2)), AutomaticRenewal = false, RoyaltyPercentage = 3, RoyaltyModel = "FixedPercentage", RoyaltyValue = 3, MinimumGuarantee = 10000, Status = "Ativo", CreatedAtUtc = now, IsActive = true };
        dbContext.Universities.Add(institution); dbContext.Researchers.Add(researcher); dbContext.Companies.Add(company); dbContext.Inventions.Add(invention);
        dbContext.InventionResearchers.Add(new InventionResearcher { InventionId = invention.Id, ResearcherId = researcher.Id });
        dbContext.TechnologyTransferContracts.Add(contract);
        dbContext.RoyaltyPayments.Add(new RoyaltyPayment { Id = Guid.NewGuid(), ContractId = contract.Id, Competence = new DateOnly(now.Year, now.Month, 1), AmountReceived = 2500, Notes = "Recebimento demonstrativo", ReceivedAt = DateOnly.FromDateTime(now), CreatedAtUtc = now, IsActive = true });
        dbContext.TechnologyTransferOpportunities.Add(new TechnologyTransferOpportunity { Id = Guid.NewGuid(), InventionId = invention.Id, UniversityId = institution.Id, CompanyId = company.Id, Stage = "Em Operação", Notes = "Pipeline demonstrativo", CreatedAtUtc = now, UpdatedAtUtc = now, IsActive = true });
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
