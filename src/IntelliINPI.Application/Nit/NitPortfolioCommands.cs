using FluentValidation;
using IntelliINPI.Application.Abstractions;
using IntelliINPI.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Application.Nit;

public sealed record PortfolioInventionDto(Guid Id, Guid InstitutionId, string InstitutionName, string Title, string Summary,
    string? ExecutiveSummary, string? TechnicalDescription, string? TechnologyArea, int? Trl, string? CommercialPotential,
    string? TargetMarket, string? ProtectionStatus, DateOnly? CreationDate, string? Responsible, string Status,
    IReadOnlyList<Guid> ResearcherIds, IReadOnlyList<string> Researchers, DateTime CreatedAtUtc, DateTime? UpdatedAtUtc);
public sealed record PortfolioInventionRequest(Guid InstitutionId, string Title, string Summary, string? ExecutiveSummary,
    string? TechnicalDescription, string? TechnologyArea, int? Trl, string? CommercialPotential, string? TargetMarket,
    string? ProtectionStatus, DateOnly? CreationDate, string? Responsible, string Status, IReadOnlyList<Guid>? ResearcherIds);
public sealed record ListPortfolioInventionsQuery : IRequest<IReadOnlyList<PortfolioInventionDto>>;
public sealed record SavePortfolioInventionCommand(Guid? Id, PortfolioInventionRequest Data) : IRequest<PortfolioInventionDto>;

public sealed class PortfolioInventionRequestValidator : AbstractValidator<PortfolioInventionRequest>
{
    private static readonly string[] Statuses = ["Rascunho", "Em Avaliação", "Aprovada", "Rejeitada", "Protegida", "Licenciada"];
    public PortfolioInventionRequestValidator()
    {
        RuleFor(x => x.InstitutionId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(240);
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Trl).InclusiveBetween(1, 9).When(x => x.Trl.HasValue);
        RuleFor(x => x.Status).Must(Statuses.Contains).WithMessage("Status de invenção inválido.");
    }
}

public sealed class PortfolioInventionHandlers(IApplicationDbContext db, ICurrentUser user) :
    IRequestHandler<ListPortfolioInventionsQuery, IReadOnlyList<PortfolioInventionDto>>,
    IRequestHandler<SavePortfolioInventionCommand, PortfolioInventionDto>
{
    public async Task<IReadOnlyList<PortfolioInventionDto>> Handle(ListPortfolioInventionsQuery request, CancellationToken ct)
    {
        var access = await NitAccessContext.LoadAsync(db, user.UserId, ct);
        var query = db.Inventions.AsNoTracking().Include(x => x.University).Include(x => x.Researchers).ThenInclude(x => x.Researcher).Where(x => x.IsActive);
        if (!access.IsGlobalAdmin) query = query.Where(x => access.UniversityIds.Contains(x.UniversityId));
        var items = await query.OrderByDescending(x => x.CreatedAtUtc).ToListAsync(ct);
        return items.Select(Map).ToList();
    }

    public async Task<PortfolioInventionDto> Handle(SavePortfolioInventionCommand request, CancellationToken ct)
    {
        var access = await NitAccessContext.LoadAsync(db, user.UserId, ct);
        if (!access.CanAccess(request.Data.InstitutionId)) throw new UnauthorizedAppException("Sem acesso à instituição.");
        var entity = request.Id.HasValue
            ? await db.Inventions.Include(x => x.Researchers).SingleOrDefaultAsync(x => x.Id == request.Id && x.IsActive, ct) ?? throw new NotFoundException("Invenção não encontrada.")
            : new Domain.Entities.Invention { Id = Guid.NewGuid(), Inventors = string.Empty, CreatedAtUtc = DateTime.UtcNow, IsActive = true };
        entity.UniversityId = request.Data.InstitutionId;
        entity.Title = request.Data.Title.Trim(); entity.Summary = request.Data.Summary.Trim();
        entity.ExecutiveSummary = Clean(request.Data.ExecutiveSummary); entity.TechnicalDescription = Clean(request.Data.TechnicalDescription);
        entity.TechnologyArea = Clean(request.Data.TechnologyArea); entity.Trl = request.Data.Trl;
        entity.CommercialPotential = Clean(request.Data.CommercialPotential); entity.TargetMarket = Clean(request.Data.TargetMarket);
        entity.ProtectionStatus = Clean(request.Data.ProtectionStatus); entity.CreationDate = request.Data.CreationDate;
        entity.Responsible = Clean(request.Data.Responsible); entity.Status = request.Data.Status; entity.UpdatedAtUtc = DateTime.UtcNow;
        if (!request.Id.HasValue) db.Inventions.Add(entity);
        var ids = (request.Data.ResearcherIds ?? []).Distinct().ToList();
        var valid = await db.Researchers.Where(x => ids.Contains(x.Id) && x.UniversityId == entity.UniversityId && x.IsActive).Select(x => x.Id).ToListAsync(ct);
        db.InventionResearchers.RemoveRange(entity.Researchers.Where(x => !valid.Contains(x.ResearcherId)));
        foreach (var id in valid.Where(id => entity.Researchers.All(x => x.ResearcherId != id))) entity.Researchers.Add(new Domain.Entities.InventionResearcher { InventionId = entity.Id, ResearcherId = id });
        entity.Inventors = string.Join(", ", await db.Researchers.Where(x => valid.Contains(x.Id)).OrderBy(x => x.Name).Select(x => x.Name).ToListAsync(ct));
        db.AuditLogs.Add(NitAuditLogFactory.Create(user, entity.UniversityId, "Invention", entity.Id, request.Id.HasValue ? "Updated" : "Created"));
        await db.SaveChangesAsync(ct);
        var saved = await db.Inventions.AsNoTracking().Include(x => x.University).Include(x => x.Researchers).ThenInclude(x => x.Researcher).SingleAsync(x => x.Id == entity.Id, ct);
        return Map(saved);
    }
    private static PortfolioInventionDto Map(Domain.Entities.Invention x) => new(x.Id, x.UniversityId, x.University.Name, x.Title, x.Summary, x.ExecutiveSummary, x.TechnicalDescription, x.TechnologyArea, x.Trl, x.CommercialPotential, x.TargetMarket, x.ProtectionStatus, x.CreationDate, x.Responsible, x.Status, x.Researchers.Select(r => r.ResearcherId).ToList(), x.Researchers.Select(r => r.Researcher.Name).Order().ToList(), x.CreatedAtUtc, x.UpdatedAtUtc);
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed record OperationalContractDto(Guid Id, string? Number, Guid InstitutionId, string InstitutionName, Guid CompanyId,
    string CompanyName, Guid InventionId, string InventionTitle, string Type, DateOnly? StartDate, DateOnly? EndDate,
    string? Term, bool AutomaticRenewal, decimal? RoyaltyPercentage, decimal? MinimumGuarantee, decimal? FixedValue,
    string Status, DateTime CreatedAtUtc);
public sealed record OperationalContractRequest(string? Number, Guid InstitutionId, Guid CompanyId, Guid InventionId, string Type,
    DateOnly? StartDate, DateOnly? EndDate, string? Term, bool AutomaticRenewal, decimal? RoyaltyPercentage,
    decimal? MinimumGuarantee, decimal? FixedValue, string Status);
public sealed record ListOperationalContractsQuery : IRequest<IReadOnlyList<OperationalContractDto>>;
public sealed record SaveOperationalContractCommand(Guid? Id, OperationalContractRequest Data) : IRequest<OperationalContractDto>;

public sealed class OperationalContractRequestValidator : AbstractValidator<OperationalContractRequest>
{
    private static readonly string[] Types = ["Licenciamento", "Cessão", "NDA", "Parceria", "Prestação de Serviços", "P&D", "Co-titularidade"];
    private static readonly string[] Statuses = ["Rascunho", "Em Negociação", "Assinado", "Ativo", "Encerrado", "Cancelado"];
    public OperationalContractRequestValidator()
    {
        RuleFor(x => x.InstitutionId).NotEmpty(); RuleFor(x => x.CompanyId).NotEmpty(); RuleFor(x => x.InventionId).NotEmpty();
        RuleFor(x => x.Type).Must(Types.Contains); RuleFor(x => x.Status).Must(Statuses.Contains);
        RuleFor(x => x.RoyaltyPercentage).InclusiveBetween(0, 100).When(x => x.RoyaltyPercentage.HasValue);
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate).When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }
}

public sealed class OperationalContractHandlers(IApplicationDbContext db, ICurrentUser user) : IRequestHandler<ListOperationalContractsQuery, IReadOnlyList<OperationalContractDto>>, IRequestHandler<SaveOperationalContractCommand, OperationalContractDto>
{
    public async Task<IReadOnlyList<OperationalContractDto>> Handle(ListOperationalContractsQuery request, CancellationToken ct) =>
        await db.TechnologyTransferContracts.AsNoTracking().Where(x => x.IsActive && x.CompanyId != null && x.UniversityId != null).OrderByDescending(x => x.CreatedAtUtc).Select(Map()).ToListAsync(ct);
    public async Task<OperationalContractDto> Handle(SaveOperationalContractCommand r, CancellationToken ct)
    {
        var company = await db.Companies.SingleOrDefaultAsync(x => x.Id == r.Data.CompanyId && x.IsActive, ct) ?? throw new NotFoundException("Empresa não encontrada.");
        var invention = await db.Inventions.SingleOrDefaultAsync(x => x.Id == r.Data.InventionId && x.IsActive && x.UniversityId == r.Data.InstitutionId, ct) ?? throw new NotFoundException("Invenção não encontrada.");
        var x = r.Id.HasValue ? await db.TechnologyTransferContracts.SingleOrDefaultAsync(x => x.Id == r.Id && x.IsActive, ct) ?? throw new NotFoundException("Contrato não encontrado.") : new Domain.Entities.TechnologyTransferContract { Id = Guid.NewGuid(), CreatedAtUtc = DateTime.UtcNow, IsActive = true };
        x.Number = Clean(r.Data.Number); x.UniversityId = r.Data.InstitutionId; x.CompanyId = company.Id; x.InventionId = invention.Id;
        x.CompanyName = company.TradeName ?? company.LegalName; x.Cnpj = company.Cnpj; x.Type = r.Data.Type; x.StartDate = r.Data.StartDate; x.EndDate = r.Data.EndDate; x.Term = Clean(r.Data.Term); x.AutomaticRenewal = r.Data.AutomaticRenewal; x.RoyaltyPercentage = r.Data.RoyaltyPercentage; x.RoyaltyValue = r.Data.RoyaltyPercentage; x.MinimumGuarantee = r.Data.MinimumGuarantee; x.FixedValue = r.Data.FixedValue; x.RoyaltyModel = r.Data.RoyaltyPercentage.HasValue ? "FixedPercentage" : "MinimumGuarantee"; x.Status = r.Data.Status; x.UpdatedAtUtc = DateTime.UtcNow;
        if (!r.Id.HasValue) db.TechnologyTransferContracts.Add(x);
        db.AuditLogs.Add(NitAuditLogFactory.Create(user, invention.UniversityId, "TechnologyTransferContract", x.Id, r.Id.HasValue ? "Updated" : "Created")); await db.SaveChangesAsync(ct);
        return await db.TechnologyTransferContracts.AsNoTracking().Where(y => y.Id == x.Id).Select(Map()).SingleAsync(ct);
    }
    private static System.Linq.Expressions.Expression<Func<Domain.Entities.TechnologyTransferContract, OperationalContractDto>> Map() => x => new(x.Id, x.Number, x.UniversityId!.Value, x.University!.Name, x.CompanyId!.Value, x.Company!.TradeName ?? x.Company.LegalName, x.InventionId, x.Invention.Title, x.Type, x.StartDate, x.EndDate, x.Term, x.AutomaticRenewal, x.RoyaltyPercentage, x.MinimumGuarantee, x.FixedValue, x.Status, x.CreatedAtUtc);
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
