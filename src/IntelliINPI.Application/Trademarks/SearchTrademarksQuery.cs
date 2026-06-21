using FluentValidation;
using IntelliINPI.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Application.Trademarks;

public sealed record TrademarkDto(Guid Id, string ProcessNumber, string Name, string? OwnerName, string? StatusDescription);
public sealed record TrademarkSearchItemDto(
    Guid Id,
    string ProcessNumber,
    string Name,
    string? Status,
    IReadOnlyList<string> NiceClasses,
    IReadOnlyList<string> Owners,
    DateOnly? FilingDate,
    DateOnly? RegistrationDate,
    DateOnly? LastDispatchDate,
    string? InpiDetailUrl);

public sealed record TrademarkDetailNiceClassDto(string Code, int ClassNumber, string? Specification);
public sealed record TrademarkDetailViennaClassDto(string Edition, string Code, string? Description);
public sealed record TrademarkDetailPetitionDto(string Protocol, DateOnly? FiledAt, string? ServiceCode, string? ClientName, string? Delivery, DateOnly? DeliveryDate);
public sealed record TrademarkDetailDispatchDto(int? RpiNumber, DateOnly PublishedAt, string Code, string Description);
public sealed record TrademarkRenewalWindowDto(DateOnly? OrdinaryStart, DateOnly? OrdinaryEnd, DateOnly? ExtraordinaryStart, DateOnly? ExtraordinaryEnd);
public sealed record TrademarkDetailDto(
    Guid Id,
    string ProcessNumber,
    string Name,
    string? Status,
    string? Presentation,
    string? Nature,
    string? LegalRepresentative,
    DateOnly? FilingDate,
    DateOnly? RegistrationDate,
    DateOnly? ExpirationDate,
    TrademarkRenewalWindowDto RenewalWindow,
    string? InpiDetailUrl,
    string? LogoUrl,
    IReadOnlyList<string> Owners,
    IReadOnlyList<TrademarkDetailNiceClassDto> NiceClasses,
    IReadOnlyList<TrademarkDetailViennaClassDto> ViennaClasses,
    IReadOnlyList<TrademarkDetailPetitionDto> Petitions,
    IReadOnlyList<TrademarkDetailDispatchDto> Dispatches);

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalItems);
public sealed record SearchTrademarksQuery(string? Term) : IRequest<IReadOnlyList<TrademarkDto>>;
public sealed record SearchLocalTrademarksQuery(
    string? Query,
    string? NiceClass,
    string? Status,
    string? Owner,
    int Page,
    int PageSize) : IRequest<PagedResult<TrademarkSearchItemDto>>;
public sealed record GetTrademarkDetailQuery(string ProcessNumber) : IRequest<TrademarkDetailDto?>;

public sealed class SearchTrademarksQueryValidator : AbstractValidator<SearchTrademarksQuery>
{
    public SearchTrademarksQueryValidator()
    {
        RuleFor(x => x.Term).MaximumLength(120);
    }
}

public sealed class SearchLocalTrademarksQueryValidator : AbstractValidator<SearchLocalTrademarksQuery>
{
    public SearchLocalTrademarksQueryValidator()
    {
        RuleFor(x => x.Query).MaximumLength(120);
        RuleFor(x => x.NiceClass).MaximumLength(20);
        RuleFor(x => x.Status).MaximumLength(120);
        RuleFor(x => x.Owner).MaximumLength(120);
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class SearchTrademarksQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<SearchTrademarksQuery, IReadOnlyList<TrademarkDto>>
{
    public async Task<IReadOnlyList<TrademarkDto>> Handle(SearchTrademarksQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Trademarks.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Term))
        {
            var term = request.Term.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(term) || x.ProcessNumber.Contains(term));
        }

        return await query
            .OrderBy(x => x.Name)
            .Take(50)
            .Select(x => new TrademarkDto(
                x.Id,
                x.ProcessNumber,
                x.Name,
                x.Owner == null ? null : x.Owner.Name,
                x.Status == null ? null : x.Status.Description))
            .ToListAsync(cancellationToken);
    }
}

public sealed class SearchLocalTrademarksQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<SearchLocalTrademarksQuery, PagedResult<TrademarkSearchItemDto>>
{
    public async Task<PagedResult<TrademarkSearchItemDto>> Handle(SearchLocalTrademarksQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var query = dbContext.Trademarks.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var term = request.Query.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(term) || x.ProcessNumber.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(request.NiceClass))
        {
            var niceClass = NormalizeNiceClassCode(request.NiceClass);
            var niceClassNumber = int.TryParse(niceClass, out var parsedNiceClass) ? parsedNiceClass : -1;
            query = query.Where(x => x.NiceClasses.Any(c => c.Code == niceClass || c.ClassNumber == niceClassNumber));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = request.Status.Trim().ToLowerInvariant();
            query = query.Where(x => x.Status != null && (x.Status.Code.ToLower().Contains(status) || x.Status.Description.ToLower().Contains(status)));
        }

        if (!string.IsNullOrWhiteSpace(request.Owner))
        {
            var owner = request.Owner.Trim().ToLowerInvariant();
            query = query.Where(x => x.Owner != null && x.Owner.Name.ToLower().Contains(owner));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.ProcessNumber,
                x.Name,
                Status = x.Status == null ? null : x.Status.Description,
                NiceClasses = x.NiceClasses.OrderBy(c => c.Code).Select(c => c.Code).ToList(),
                OwnerNames = x.OwnerLinks.OrderBy(o => o.Owner.Name).Select(o => o.Owner.Name).ToList(),
                LegacyOwnerName = x.Owner == null ? null : x.Owner.Name,
                x.FilingDate,
                x.RegistrationDate,
                LastDispatchDate = x.Dispatches.OrderByDescending(d => d.PublishedAt).Select(d => (DateOnly?)d.PublishedAt).FirstOrDefault(),
                x.InpiDetailUrl
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(x =>
            {
                var owners = x.OwnerNames.Count > 0
                    ? x.OwnerNames
                    : string.IsNullOrWhiteSpace(x.LegacyOwnerName) ? new List<string>() : new List<string> { x.LegacyOwnerName };

                return new TrademarkSearchItemDto(
                    x.Id,
                    x.ProcessNumber,
                    x.Name,
                    x.Status,
                    x.NiceClasses,
                    owners,
                    x.FilingDate,
                    x.RegistrationDate,
                    x.LastDispatchDate,
                    x.InpiDetailUrl);
            })
            .ToList();

        return new PagedResult<TrademarkSearchItemDto>(items, page, pageSize, totalItems);
    }

    private static string NormalizeNiceClassCode(string value)
    {
        var digits = new string(value.Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(digits) ? value.Trim() : digits.PadLeft(2, '0');
    }
}

public sealed class GetTrademarkDetailQueryValidator : AbstractValidator<GetTrademarkDetailQuery>
{
    public GetTrademarkDetailQueryValidator()
    {
        RuleFor(x => x.ProcessNumber).NotEmpty().MaximumLength(40);
    }
}

public sealed class GetTrademarkDetailQueryHandler(IApplicationDbContext dbContext, IInpiSearchService inpiSearchService)
    : IRequestHandler<GetTrademarkDetailQuery, TrademarkDetailDto?>
{
    public async Task<TrademarkDetailDto?> Handle(GetTrademarkDetailQuery request, CancellationToken cancellationToken)
    {
        var processNumber = request.ProcessNumber.Trim();
        try
        {
            await inpiSearchService.SyncTrademarkDetailAsync(processNumber, cancellationToken);
        }
        catch
        {
            // The local detail remains usable when the public INPI session is unavailable.
        }

        var trademark = await dbContext.Trademarks
            .AsNoTracking()
            .Include(x => x.Status)
            .Include(x => x.Owner)
            .Include(x => x.OwnerLinks)
                .ThenInclude(x => x.Owner)
            .Include(x => x.NiceClasses)
            .Include(x => x.ViennaClasses)
            .Include(x => x.Petitions)
            .Include(x => x.Dispatches)
            .FirstOrDefaultAsync(x => x.ProcessNumber == processNumber, cancellationToken);

        if (trademark is null)
        {
            return null;
        }

        var owners = trademark.OwnerLinks
            .Select(x => x.Owner.Name)
            .Concat(trademark.Owner is null ? [] : new[] { trademark.Owner.Name })
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        var niceClasses = trademark.NiceClasses
            .OrderBy(x => x.ClassNumber)
            .ThenBy(x => x.Code)
            .Select(x => new TrademarkDetailNiceClassDto(x.Code, x.ClassNumber, x.Specification))
            .ToList();

        var dispatches = trademark.Dispatches
            .OrderByDescending(x => x.PublishedAt)
            .ThenByDescending(x => x.RpiNumber)
            .Select(x => new TrademarkDetailDispatchDto(x.RpiNumber, x.PublishedAt, x.Code, x.Description))
            .ToList();

        var viennaClasses = trademark.ViennaClasses
            .OrderBy(x => x.Edition)
            .ThenBy(x => x.Code)
            .Select(x => new TrademarkDetailViennaClassDto(x.Edition, x.Code, x.Description))
            .ToList();

        var petitions = trademark.Petitions
            .OrderByDescending(x => x.FiledAt)
            .ThenBy(x => x.Protocol)
            .Select(x => new TrademarkDetailPetitionDto(x.Protocol, x.FiledAt, x.ServiceCode, x.ClientName, x.Delivery, x.DeliveryDate))
            .ToList();

        return new TrademarkDetailDto(
            trademark.Id,
            trademark.ProcessNumber,
            trademark.Name,
            trademark.Status?.Description,
            trademark.Presentation,
            trademark.Nature,
            trademark.LegalRepresentative,
            trademark.FilingDate,
            trademark.RegistrationDate,
            trademark.ExpirationDate,
            BuildRenewalWindow(trademark.ExpirationDate),
            trademark.InpiDetailUrl,
            string.IsNullOrWhiteSpace(trademark.LogoPath) ? null : $"/api/trademarks/{trademark.ProcessNumber}/logo",
            owners,
            niceClasses,
            viennaClasses,
            petitions,
            dispatches);
    }

    private static TrademarkRenewalWindowDto BuildRenewalWindow(DateOnly? expirationDate)
    {
        if (!expirationDate.HasValue)
        {
            return new TrademarkRenewalWindowDto(null, null, null, null);
        }

        var ordinaryEnd = expirationDate.Value;
        var ordinaryStart = ordinaryEnd.AddYears(-1);
        var extraordinaryStart = ordinaryEnd.AddDays(1);
        var extraordinaryEnd = ordinaryEnd.AddMonths(6);
        return new TrademarkRenewalWindowDto(ordinaryStart, ordinaryEnd, extraordinaryStart, extraordinaryEnd);
    }
}
