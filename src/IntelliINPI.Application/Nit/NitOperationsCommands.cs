using System.Text.Json;
using FluentValidation;
using IntelliINPI.Application.Abstractions;
using IntelliINPI.Application.Common.Exceptions;
using IntelliINPI.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Application.Nit;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalItems);

public sealed record InstitutionDto(Guid Id, string Name, string? TradeName, string? Cnpj, string Tier, string Type,
    string? Website, string? Email, string? Phone, string? ContactName, string Status, DateTime CreatedAtUtc, bool IsActive);
public sealed record InstitutionRequest(string Name, string? TradeName, string? Cnpj, string? Tier, string Type,
    string? Website, string? Email, string? Phone, string? ContactName, string? Status);
public sealed record ListInstitutionsQuery : IRequest<IReadOnlyList<InstitutionDto>>;
public sealed record GetInstitutionQuery(Guid Id) : IRequest<InstitutionDto>;
public sealed record SaveInstitutionCommand(Guid? Id, InstitutionRequest Data) : IRequest<InstitutionDto>;
public sealed record DeleteInstitutionCommand(Guid Id) : IRequest;

public sealed class InstitutionRequestValidator : AbstractValidator<InstitutionRequest>
{
    private static readonly string[] Types = ["Universidade", "ICT", "Instituto", "Fundação", "Empresa", "Centro de Pesquisa", "Governo"];
    public InstitutionRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).Must(x => Types.Contains(x)).WithMessage("Tipo de instituição inválido.");
        RuleFor(x => x.Cnpj).MaximumLength(20);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}

public sealed class InstitutionHandlers(IApplicationDbContext db, ICurrentUser user) :
    IRequestHandler<ListInstitutionsQuery, IReadOnlyList<InstitutionDto>>,
    IRequestHandler<GetInstitutionQuery, InstitutionDto>,
    IRequestHandler<SaveInstitutionCommand, InstitutionDto>,
    IRequestHandler<DeleteInstitutionCommand>
{
    public async Task<IReadOnlyList<InstitutionDto>> Handle(ListInstitutionsQuery request, CancellationToken ct) =>
        await db.Universities.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name).Select(Map()).ToListAsync(ct);

    public async Task<InstitutionDto> Handle(GetInstitutionQuery request, CancellationToken ct) =>
        await db.Universities.AsNoTracking().Where(x => x.Id == request.Id && x.IsActive).Select(Map()).SingleOrDefaultAsync(ct)
        ?? throw new NotFoundException("Instituição não encontrada.");

    public async Task<InstitutionDto> Handle(SaveInstitutionCommand request, CancellationToken ct)
    {
        University entity;
        object? previous = null;
        if (request.Id.HasValue)
        {
            entity = await db.Universities.SingleOrDefaultAsync(x => x.Id == request.Id && x.IsActive, ct)
                ?? throw new NotFoundException("Instituição não encontrada.");
            previous = Snapshot(entity);
        }
        else
        {
            entity = new University { Id = Guid.NewGuid(), CreatedAtUtc = DateTime.UtcNow, IsActive = true };
            db.Universities.Add(entity);
        }

        entity.Name = request.Data.Name.Trim();
        entity.TradeName = Clean(request.Data.TradeName);
        entity.Cnpj = Clean(request.Data.Cnpj);
        entity.Tier = Clean(request.Data.Tier) ?? "Intermediário";
        entity.Type = request.Data.Type;
        entity.Website = Clean(request.Data.Website);
        entity.Email = Clean(request.Data.Email);
        entity.Phone = Clean(request.Data.Phone);
        entity.ContactName = Clean(request.Data.ContactName);
        entity.Status = Clean(request.Data.Status) ?? "Ativa";
        db.AuditLogs.Add(Audit(entity.Id, entity.Id, request.Id.HasValue ? "Updated" : "Created", previous, Snapshot(entity)));
        await db.SaveChangesAsync(ct);
        return await db.Universities.AsNoTracking().Where(x => x.Id == entity.Id).Select(Map()).SingleAsync(ct);
    }

    public async Task Handle(DeleteInstitutionCommand request, CancellationToken ct)
    {
        var entity = await db.Universities.SingleOrDefaultAsync(x => x.Id == request.Id && x.IsActive, ct)
            ?? throw new NotFoundException("Instituição não encontrada.");
        var previous = Snapshot(entity);
        entity.IsActive = false;
        entity.Status = "Inativa";
        db.AuditLogs.Add(Audit(entity.Id, entity.Id, "Deleted", previous, Snapshot(entity)));
        await db.SaveChangesAsync(ct);
    }

    private AuditLog Audit(Guid universityId, Guid entityId, string action, object? oldValue, object? newValue)
    {
        var log = NitAuditLogFactory.Create(user, universityId, "Institution", entityId, action);
        log.PreviousValue = oldValue is null ? null : JsonSerializer.Serialize(oldValue);
        log.NewValue = newValue is null ? null : JsonSerializer.Serialize(newValue);
        return log;
    }
    private static object Snapshot(University x) => new { x.Name, x.TradeName, x.Cnpj, x.Type, x.Website, x.Email, x.Phone, x.ContactName, x.Status };
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static System.Linq.Expressions.Expression<Func<University, InstitutionDto>> Map() => x => new(x.Id, x.Name, x.TradeName, x.Cnpj, x.Tier, x.Type, x.Website, x.Email, x.Phone, x.ContactName, x.Status, x.CreatedAtUtc, x.IsActive);
}

public sealed record ResearcherDto(Guid Id, Guid InstitutionId, string InstitutionName, string Name, string? Cpf, string? Email,
    string? Phone, string? Department, string? Position, string? LattesUrl, string? Orcid, string? Specialties,
    string? TechnologyAreas, int InventionsCount, DateTime CreatedAtUtc);
public sealed record ResearcherRequest(Guid InstitutionId, string Name, string? Cpf, string? Email, string? Phone,
    string? Department, string? Position, string? LattesUrl, string? Orcid, string? Specialties, string? TechnologyAreas);
public sealed record ListResearchersQuery(string? Search, Guid? InstitutionId, string? TechnologyArea, int Page = 1, int PageSize = 20) : IRequest<PagedResult<ResearcherDto>>;
public sealed record GetResearcherQuery(Guid Id) : IRequest<ResearcherDto>;
public sealed record SaveResearcherCommand(Guid? Id, ResearcherRequest Data) : IRequest<ResearcherDto>;
public sealed record DeleteResearcherCommand(Guid Id) : IRequest;

public sealed class ResearcherRequestValidator : AbstractValidator<ResearcherRequest>
{
    public ResearcherRequestValidator()
    {
        RuleFor(x => x.InstitutionId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}

public sealed class ResearcherHandlers(IApplicationDbContext db, ICurrentUser user) :
    IRequestHandler<ListResearchersQuery, PagedResult<ResearcherDto>>, IRequestHandler<GetResearcherQuery, ResearcherDto>,
    IRequestHandler<SaveResearcherCommand, ResearcherDto>, IRequestHandler<DeleteResearcherCommand>
{
    public async Task<PagedResult<ResearcherDto>> Handle(ListResearchersQuery r, CancellationToken ct)
    {
        var access = await NitAccessContext.LoadAsync(db, user.UserId, ct);
        var q = db.Researchers.AsNoTracking().Where(x => x.IsActive);
        if (!access.IsGlobalAdmin) q = q.Where(x => access.UniversityIds.Contains(x.UniversityId));
        if (r.InstitutionId.HasValue) q = q.Where(x => x.UniversityId == r.InstitutionId);
        if (!string.IsNullOrWhiteSpace(r.Search)) { var s = r.Search.Trim().ToLower(); q = q.Where(x => x.Name.ToLower().Contains(s) || (x.Cpf != null && x.Cpf.Contains(s)) || (x.Email != null && x.Email.ToLower().Contains(s))); }
        if (!string.IsNullOrWhiteSpace(r.TechnologyArea)) { var a = r.TechnologyArea.Trim().ToLower(); q = q.Where(x => x.TechnologyAreas != null && x.TechnologyAreas.ToLower().Contains(a)); }
        var total = await q.CountAsync(ct); var page = Math.Max(r.Page, 1); var size = Math.Clamp(r.PageSize, 1, 100);
        var items = await q.OrderBy(x => x.Name).Skip((page - 1) * size).Take(size).Select(Map()).ToListAsync(ct);
        return new(items, page, size, total);
    }
    public async Task<ResearcherDto> Handle(GetResearcherQuery r, CancellationToken ct) =>
        await db.Researchers.AsNoTracking().Where(x => x.Id == r.Id && x.IsActive).Select(Map()).SingleOrDefaultAsync(ct) ?? throw new NotFoundException("Pesquisador não encontrado.");
    public async Task<ResearcherDto> Handle(SaveResearcherCommand r, CancellationToken ct)
    {
        var access = await NitAccessContext.LoadAsync(db, user.UserId, ct); if (!access.CanAccess(r.Data.InstitutionId)) throw new UnauthorizedAppException("Sem acesso à instituição.");
        if (!await db.Universities.AnyAsync(x => x.Id == r.Data.InstitutionId && x.IsActive, ct)) throw new NotFoundException("Instituição não encontrada.");
        var x = r.Id.HasValue ? await db.Researchers.SingleOrDefaultAsync(x => x.Id == r.Id && x.IsActive, ct) ?? throw new NotFoundException("Pesquisador não encontrado.") : new Researcher { Id = Guid.NewGuid(), CreatedAtUtc = DateTime.UtcNow, IsActive = true };
        var old = r.Id.HasValue ? JsonSerializer.Serialize(x) : null;
        x.UniversityId = r.Data.InstitutionId; x.Name = r.Data.Name.Trim(); x.Cpf = Clean(r.Data.Cpf); x.Email = Clean(r.Data.Email); x.Phone = Clean(r.Data.Phone); x.Department = Clean(r.Data.Department); x.Position = Clean(r.Data.Position); x.LattesUrl = Clean(r.Data.LattesUrl); x.Orcid = Clean(r.Data.Orcid); x.Specialties = Clean(r.Data.Specialties); x.TechnologyAreas = Clean(r.Data.TechnologyAreas); x.UpdatedAtUtc = DateTime.UtcNow;
        if (!r.Id.HasValue) db.Researchers.Add(x);
        db.AuditLogs.Add(Audit(x.UniversityId, "Researcher", x.Id, r.Id.HasValue ? "Updated" : "Created", old, JsonSerializer.Serialize(x)));
        await db.SaveChangesAsync(ct); return await db.Researchers.AsNoTracking().Where(y => y.Id == x.Id).Select(Map()).SingleAsync(ct);
    }
    public async Task Handle(DeleteResearcherCommand r, CancellationToken ct) { var x = await db.Researchers.SingleOrDefaultAsync(x => x.Id == r.Id && x.IsActive, ct) ?? throw new NotFoundException("Pesquisador não encontrado."); x.IsActive = false; x.UpdatedAtUtc = DateTime.UtcNow; db.AuditLogs.Add(Audit(x.UniversityId, "Researcher", x.Id, "Deleted", null, null)); await db.SaveChangesAsync(ct); }
    private AuditLog Audit(Guid uid, string name, Guid id, string action, string? oldValue, string? newValue) { var log = NitAuditLogFactory.Create(user, uid, name, id, action); log.PreviousValue = oldValue; log.NewValue = newValue; return log; }
    private static string? Clean(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();
    private static System.Linq.Expressions.Expression<Func<Researcher, ResearcherDto>> Map() => x => new(x.Id, x.UniversityId, x.University.Name, x.Name, x.Cpf, x.Email, x.Phone, x.Department, x.Position, x.LattesUrl, x.Orcid, x.Specialties, x.TechnologyAreas, x.Inventions.Count, x.CreatedAtUtc);
}

public sealed record CompanyDto(Guid Id, Guid? InstitutionId, string? InstitutionName, string LegalName, string? TradeName, string? Cnpj,
    string? Segment, string? Size, string? ContactName, string? Email, string? Phone, string? Website, string? Notes, int ContractsCount, DateTime CreatedAtUtc);
public sealed record CompanyRequest(Guid? InstitutionId, string LegalName, string? TradeName, string? Cnpj, string? Segment,
    string? Size, string? ContactName, string? Email, string? Phone, string? Website, string? Notes);
public sealed record ListCompaniesQuery(string? Search = null) : IRequest<IReadOnlyList<CompanyDto>>;
public sealed record GetCompanyQuery(Guid Id) : IRequest<CompanyDto>;
public sealed record SaveCompanyCommand(Guid? Id, CompanyRequest Data) : IRequest<CompanyDto>;
public sealed record DeleteCompanyCommand(Guid Id) : IRequest;

public sealed class CompanyHandlers(IApplicationDbContext db, ICurrentUser user) : IRequestHandler<ListCompaniesQuery, IReadOnlyList<CompanyDto>>, IRequestHandler<GetCompanyQuery, CompanyDto>, IRequestHandler<SaveCompanyCommand, CompanyDto>, IRequestHandler<DeleteCompanyCommand>
{
    public async Task<IReadOnlyList<CompanyDto>> Handle(ListCompaniesQuery r, CancellationToken ct) { var q = db.Companies.AsNoTracking().Where(x => x.IsActive); if (!string.IsNullOrWhiteSpace(r.Search)) { var s = r.Search.ToLower(); q = q.Where(x => x.LegalName.ToLower().Contains(s) || (x.TradeName != null && x.TradeName.ToLower().Contains(s)) || (x.Cnpj != null && x.Cnpj.Contains(s))); } return await q.OrderBy(x => x.LegalName).Select(Map()).ToListAsync(ct); }
    public async Task<CompanyDto> Handle(GetCompanyQuery r, CancellationToken ct) => await db.Companies.AsNoTracking().Where(x => x.Id == r.Id && x.IsActive).Select(Map()).SingleOrDefaultAsync(ct) ?? throw new NotFoundException("Empresa não encontrada.");
    public async Task<CompanyDto> Handle(SaveCompanyCommand r, CancellationToken ct) { var x = r.Id.HasValue ? await db.Companies.SingleOrDefaultAsync(x => x.Id == r.Id && x.IsActive, ct) ?? throw new NotFoundException("Empresa não encontrada.") : new Company { Id = Guid.NewGuid(), CreatedAtUtc = DateTime.UtcNow, IsActive = true }; x.UniversityId = r.Data.InstitutionId; x.LegalName = r.Data.LegalName.Trim(); x.TradeName = C(r.Data.TradeName); x.Cnpj = C(r.Data.Cnpj); x.Segment = C(r.Data.Segment); x.Size = C(r.Data.Size); x.ContactName = C(r.Data.ContactName); x.Email = C(r.Data.Email); x.Phone = C(r.Data.Phone); x.Website = C(r.Data.Website); x.Notes = C(r.Data.Notes); x.UpdatedAtUtc = DateTime.UtcNow; if (!r.Id.HasValue) db.Companies.Add(x); db.AuditLogs.Add(new AuditLog { Id = Guid.NewGuid(), UserId = user.UserId, UniversityId = x.UniversityId, Module = "NIT", EntityName = "Company", EntityId = x.Id, Action = r.Id.HasValue ? "Updated" : "Created", IpAddress = user.IpAddress, CreatedAtUtc = DateTime.UtcNow }); await db.SaveChangesAsync(ct); return await db.Companies.AsNoTracking().Where(y => y.Id == x.Id).Select(Map()).SingleAsync(ct); }
    public async Task Handle(DeleteCompanyCommand r, CancellationToken ct) { var x = await db.Companies.SingleOrDefaultAsync(x => x.Id == r.Id && x.IsActive, ct) ?? throw new NotFoundException("Empresa não encontrada."); x.IsActive = false; await db.SaveChangesAsync(ct); }
    private static string? C(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();
    private static System.Linq.Expressions.Expression<Func<Company, CompanyDto>> Map() => x => new(x.Id, x.UniversityId, x.University == null ? null : x.University.Name, x.LegalName, x.TradeName, x.Cnpj, x.Segment, x.Size, x.ContactName, x.Email, x.Phone, x.Website, x.Notes, x.Contracts.Count(c => c.IsActive), x.CreatedAtUtc);
}

public sealed record RoyaltyDto(Guid Id, Guid ContractId, string ContractNumber, string InventionTitle, DateOnly Competence, decimal AmountReceived, string? Notes, DateOnly ReceivedAt);
public sealed record RoyaltyRequest(Guid ContractId, DateOnly Competence, decimal AmountReceived, string? Notes, DateOnly ReceivedAt);
public sealed record RoyaltySummaryDto(decimal TotalReceived, decimal ReceivedThisYear, decimal ReceivedThisMonth, IReadOnlyList<RoyaltyTopContractDto> TopContracts);
public sealed record RoyaltyTopContractDto(Guid ContractId, string ContractNumber, decimal Total);
public sealed record ListRoyaltiesQuery : IRequest<IReadOnlyList<RoyaltyDto>>;
public sealed record GetRoyaltySummaryQuery : IRequest<RoyaltySummaryDto>;
public sealed record SaveRoyaltyCommand(Guid? Id, RoyaltyRequest Data) : IRequest<RoyaltyDto>;
public sealed record DeleteRoyaltyCommand(Guid Id) : IRequest;

public sealed class RoyaltyHandlers(IApplicationDbContext db, ICurrentUser user) : IRequestHandler<ListRoyaltiesQuery, IReadOnlyList<RoyaltyDto>>, IRequestHandler<GetRoyaltySummaryQuery, RoyaltySummaryDto>, IRequestHandler<SaveRoyaltyCommand, RoyaltyDto>, IRequestHandler<DeleteRoyaltyCommand>
{
    public async Task<IReadOnlyList<RoyaltyDto>> Handle(ListRoyaltiesQuery r, CancellationToken ct) => await db.RoyaltyPayments.AsNoTracking().Where(x => x.IsActive).OrderByDescending(x => x.ReceivedAt).Select(Map()).ToListAsync(ct);
    public async Task<RoyaltySummaryDto> Handle(GetRoyaltySummaryQuery r, CancellationToken ct) { var now = DateTime.UtcNow; var q = db.RoyaltyPayments.AsNoTracking().Where(x => x.IsActive); var total = await q.SumAsync(x => x.AmountReceived, ct); var year = await q.Where(x => x.ReceivedAt.Year == now.Year).SumAsync(x => x.AmountReceived, ct); var month = await q.Where(x => x.ReceivedAt.Year == now.Year && x.ReceivedAt.Month == now.Month).SumAsync(x => x.AmountReceived, ct); var top = await q.GroupBy(x => new { x.ContractId, x.Contract.Number }).Select(g => new RoyaltyTopContractDto(g.Key.ContractId, g.Key.Number ?? "Sem número", g.Sum(x => x.AmountReceived))).OrderByDescending(x => x.Total).Take(5).ToListAsync(ct); return new(total, year, month, top); }
    public async Task<RoyaltyDto> Handle(SaveRoyaltyCommand r, CancellationToken ct) { var contract = await db.TechnologyTransferContracts.Include(x => x.Invention).SingleOrDefaultAsync(x => x.Id == r.Data.ContractId && x.IsActive, ct) ?? throw new NotFoundException("Contrato não encontrado."); var x = r.Id.HasValue ? await db.RoyaltyPayments.SingleOrDefaultAsync(x => x.Id == r.Id && x.IsActive, ct) ?? throw new NotFoundException("Royalty não encontrado.") : new RoyaltyPayment { Id = Guid.NewGuid(), CreatedAtUtc = DateTime.UtcNow, IsActive = true }; x.ContractId = contract.Id; x.Competence = r.Data.Competence; x.AmountReceived = r.Data.AmountReceived; x.Notes = r.Data.Notes; x.ReceivedAt = r.Data.ReceivedAt; if (!r.Id.HasValue) db.RoyaltyPayments.Add(x); db.AuditLogs.Add(NitAuditLogFactory.Create(user, contract.Invention.UniversityId, "RoyaltyPayment", x.Id, r.Id.HasValue ? "Updated" : "Created")); await db.SaveChangesAsync(ct); return await db.RoyaltyPayments.AsNoTracking().Where(y => y.Id == x.Id).Select(Map()).SingleAsync(ct); }
    public async Task Handle(DeleteRoyaltyCommand r, CancellationToken ct) { var x = await db.RoyaltyPayments.SingleOrDefaultAsync(x => x.Id == r.Id && x.IsActive, ct) ?? throw new NotFoundException("Royalty não encontrado."); x.IsActive = false; await db.SaveChangesAsync(ct); }
    private static System.Linq.Expressions.Expression<Func<RoyaltyPayment, RoyaltyDto>> Map() => x => new(x.Id, x.ContractId, x.Contract.Number ?? "Sem número", x.Contract.Invention.Title, x.Competence, x.AmountReceived, x.Notes, x.ReceivedAt);
}

public static class TransferStages { public static readonly string[] All = ["Nova Tecnologia", "Em Prospecção", "Empresa Interessada", "NDA Assinado", "Negociação", "Licenciamento", "Em Operação", "Gerando Royalties"]; }
public sealed record TransferOpportunityDto(Guid Id, Guid InventionId, string InventionTitle, Guid InstitutionId, string InstitutionName, Guid? CompanyId, string? CompanyName, string Stage, string? Notes, int SortOrder, DateTime UpdatedAtUtc);
public sealed record TransferOpportunityRequest(Guid InventionId, Guid? CompanyId, string? Stage, string? Notes);
public sealed record ListTransferPipelineQuery : IRequest<IReadOnlyList<TransferOpportunityDto>>;
public sealed record SaveTransferOpportunityCommand(Guid? Id, TransferOpportunityRequest Data) : IRequest<TransferOpportunityDto>;
public sealed record MoveTransferOpportunityCommand(Guid Id, string Stage, int SortOrder) : IRequest<TransferOpportunityDto>;
public sealed record DeleteTransferOpportunityCommand(Guid Id) : IRequest;

public sealed class TransferPipelineHandlers(IApplicationDbContext db, ICurrentUser user) : IRequestHandler<ListTransferPipelineQuery, IReadOnlyList<TransferOpportunityDto>>, IRequestHandler<SaveTransferOpportunityCommand, TransferOpportunityDto>, IRequestHandler<MoveTransferOpportunityCommand, TransferOpportunityDto>, IRequestHandler<DeleteTransferOpportunityCommand>
{
    public async Task<IReadOnlyList<TransferOpportunityDto>> Handle(ListTransferPipelineQuery r, CancellationToken ct) => await db.TechnologyTransferOpportunities.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.SortOrder).Select(Map()).ToListAsync(ct);
    public async Task<TransferOpportunityDto> Handle(SaveTransferOpportunityCommand r, CancellationToken ct) { var invention = await db.Inventions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == r.Data.InventionId && x.IsActive, ct) ?? throw new NotFoundException("Invenção não encontrada."); var x = r.Id.HasValue ? await db.TechnologyTransferOpportunities.SingleOrDefaultAsync(x => x.Id == r.Id && x.IsActive, ct) ?? throw new NotFoundException("Oportunidade não encontrada.") : new TechnologyTransferOpportunity { Id = Guid.NewGuid(), CreatedAtUtc = DateTime.UtcNow, IsActive = true }; x.InventionId = invention.Id; x.UniversityId = invention.UniversityId; x.CompanyId = r.Data.CompanyId; x.Stage = TransferStages.All.Contains(r.Data.Stage) ? r.Data.Stage! : TransferStages.All[0]; x.Notes = r.Data.Notes; x.UpdatedAtUtc = DateTime.UtcNow; if (!r.Id.HasValue) db.TechnologyTransferOpportunities.Add(x); db.AuditLogs.Add(NitAuditLogFactory.Create(user, invention.UniversityId, "TechnologyTransferOpportunity", x.Id, r.Id.HasValue ? "Updated" : "Created")); await db.SaveChangesAsync(ct); return await db.TechnologyTransferOpportunities.AsNoTracking().Where(y => y.Id == x.Id).Select(Map()).SingleAsync(ct); }
    public async Task<TransferOpportunityDto> Handle(MoveTransferOpportunityCommand r, CancellationToken ct) { if (!TransferStages.All.Contains(r.Stage)) throw new ValidationException("Etapa inválida."); var x = await db.TechnologyTransferOpportunities.SingleOrDefaultAsync(x => x.Id == r.Id && x.IsActive, ct) ?? throw new NotFoundException("Oportunidade não encontrada."); x.Stage = r.Stage; x.SortOrder = r.SortOrder; x.UpdatedAtUtc = DateTime.UtcNow; db.AuditLogs.Add(NitAuditLogFactory.Create(user, x.UniversityId, "TechnologyTransferOpportunity", x.Id, "StageChanged")); await db.SaveChangesAsync(ct); return await db.TechnologyTransferOpportunities.AsNoTracking().Where(y => y.Id == x.Id).Select(Map()).SingleAsync(ct); }
    public async Task Handle(DeleteTransferOpportunityCommand r, CancellationToken ct) { var x = await db.TechnologyTransferOpportunities.SingleOrDefaultAsync(x => x.Id == r.Id && x.IsActive, ct) ?? throw new NotFoundException("Oportunidade não encontrada."); x.IsActive = false; await db.SaveChangesAsync(ct); }
    private static System.Linq.Expressions.Expression<Func<TechnologyTransferOpportunity, TransferOpportunityDto>> Map() => x => new(x.Id, x.InventionId, x.Invention.Title, x.UniversityId, x.University.Name, x.CompanyId, x.Company == null ? null : x.Company.TradeName ?? x.Company.LegalName, x.Stage, x.Notes, x.SortOrder, x.UpdatedAtUtc);
}

public sealed record NitDocumentDto(Guid Id, string Name, string Type, Guid InstitutionId, string InstitutionName, Guid? InventionId, string? InventionTitle, Guid? ContractId, string FileName, string ContentType, long FileSize, bool IsEncrypted, string? EncryptionAlgorithm, DateTime UploadedAtUtc, string UploadedBy);
public sealed record ListNitDocumentsQuery : IRequest<IReadOnlyList<NitDocumentDto>>;
public sealed record GetNitDocumentQuery(Guid Id) : IRequest<NitDocument>;
public sealed record CreateNitDocumentCommand(string Name, string Type, Guid InstitutionId, Guid? InventionId, Guid? ContractId, string OriginalFileName, string StoredFileName, string ContentType, long FileSize, string StoragePath, bool IsEncrypted, string? EncryptionAlgorithm, string? EncryptionIV) : IRequest<NitDocumentDto>;
public sealed record RecordNitDocumentDownloadCommand(Guid Id) : IRequest;
public sealed record DeleteNitDocumentCommand(Guid Id) : IRequest<string>;

public sealed class NitDocumentHandlers(IApplicationDbContext db, ICurrentUser user) : IRequestHandler<ListNitDocumentsQuery, IReadOnlyList<NitDocumentDto>>, IRequestHandler<GetNitDocumentQuery, NitDocument>, IRequestHandler<CreateNitDocumentCommand, NitDocumentDto>, IRequestHandler<RecordNitDocumentDownloadCommand>, IRequestHandler<DeleteNitDocumentCommand, string>
{
    public async Task<IReadOnlyList<NitDocumentDto>> Handle(ListNitDocumentsQuery r, CancellationToken ct) => await db.NitDocuments.AsNoTracking().Where(x => x.IsActive).OrderByDescending(x => x.UploadedAtUtc).Select(Map()).ToListAsync(ct);
    public async Task<NitDocument> Handle(GetNitDocumentQuery r, CancellationToken ct) => await db.NitDocuments.AsNoTracking().SingleOrDefaultAsync(x => x.Id == r.Id && x.IsActive, ct) ?? throw new NotFoundException("Documento não encontrado.");
    public async Task<NitDocumentDto> Handle(CreateNitDocumentCommand r, CancellationToken ct) { var x = new NitDocument { Id = Guid.NewGuid(), Name = r.Name.Trim(), Type = r.Type, UniversityId = r.InstitutionId, InventionId = r.InventionId, ContractId = r.ContractId, FileName = r.OriginalFileName, OriginalFileName = r.OriginalFileName, StoredFileName = r.StoredFileName, ContentType = r.ContentType, FileSize = r.FileSize, StoragePath = r.StoragePath, IsEncrypted = r.IsEncrypted, EncryptionAlgorithm = r.EncryptionAlgorithm, EncryptionIV = r.EncryptionIV, UploadedByUserId = user.UserId, UploadedAtUtc = DateTime.UtcNow, IsActive = true }; db.NitDocuments.Add(x); db.AuditLogs.Add(NitAuditLogFactory.Create(user, x.UniversityId, "NitDocument", x.Id, "Uploaded")); await db.SaveChangesAsync(ct); return await db.NitDocuments.AsNoTracking().Where(y => y.Id == x.Id).Select(Map()).SingleAsync(ct); }
    public async Task Handle(RecordNitDocumentDownloadCommand r, CancellationToken ct) { var x = await db.NitDocuments.AsNoTracking().SingleOrDefaultAsync(x => x.Id == r.Id && x.IsActive, ct) ?? throw new NotFoundException("Documento não encontrado."); db.AuditLogs.Add(NitAuditLogFactory.Create(user, x.UniversityId, "NitDocument", x.Id, "Downloaded")); await db.SaveChangesAsync(ct); }
    public async Task<string> Handle(DeleteNitDocumentCommand r, CancellationToken ct) { var x = await db.NitDocuments.SingleOrDefaultAsync(x => x.Id == r.Id && x.IsActive, ct) ?? throw new NotFoundException("Documento não encontrado."); x.IsActive = false; db.AuditLogs.Add(NitAuditLogFactory.Create(user, x.UniversityId, "NitDocument", x.Id, "Deleted")); await db.SaveChangesAsync(ct); return x.StoragePath; }
    private static System.Linq.Expressions.Expression<Func<NitDocument, NitDocumentDto>> Map() => x => new(x.Id, x.Name, x.Type, x.UniversityId, x.University.Name, x.InventionId, x.Invention == null ? null : x.Invention.Title, x.ContractId, x.OriginalFileName, x.ContentType, x.FileSize, x.IsEncrypted, x.EncryptionAlgorithm, x.UploadedAtUtc, x.UploadedByUser.Email);
}

public static class NitDocumentUploadPolicy
{
    public const long MaximumFileSize = 25_000_000;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg" };
    private static readonly HashSet<string> BlockedExtensions = new(StringComparer.OrdinalIgnoreCase) { ".exe", ".bat", ".cmd", ".js", ".sh", ".dll", ".msi" };
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "image/png",
        "image/jpeg"
    };

    public static void Validate(string fileName, string contentType, long size)
    {
        if (size <= 0) throw new ValidationException("O arquivo está vazio.");
        if (size > MaximumFileSize) throw new ValidationException("O arquivo excede o limite de 25 MB.");
        var extension = Path.GetExtension(fileName);
        if (BlockedExtensions.Contains(extension)) throw new ValidationException("Tipo de arquivo perigoso bloqueado.");
        if (!AllowedExtensions.Contains(extension)) throw new ValidationException("Tipo de arquivo não permitido. Use PDF, DOC, DOCX, XLS, XLSX, PNG, JPG ou JPEG.");
        if (!AllowedContentTypes.Contains(contentType)) throw new ValidationException("O tipo de conteúdo do arquivo não é permitido.");
    }
}
