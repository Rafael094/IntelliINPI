using FluentValidation;
using IntelliINPI.Application.Abstractions;
using IntelliINPI.Application.Common.Exceptions;
using IntelliINPI.Application.Monitoring;
using IntelliINPI.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Application.IPAssets;

public sealed record RegisterAndMonitorIPAssetRequest(string Type, string Query, Guid? ClientId, Guid? UniversityId);

public sealed record RegisterAndMonitorIPAssetCommand(string Type, string Query, Guid? ClientId, Guid? UniversityId)
    : IRequest<RegisterAndMonitorIPAssetResult>;

public sealed record RegisterAndMonitorIPAssetResult(
    string Status,
    string Type,
    string Query,
    Guid? IPAssetId,
    Guid? TrademarkId,
    Guid? PatentId,
    Guid? MonitoringId,
    bool IsMonitored,
    string? Source,
    string? Warning,
    IReadOnlyList<IPAssetCandidateDto> Candidates);

public sealed record IPAssetCandidateDto(
    string Type,
    string? InpiProcessNumber,
    string Title,
    string? OwnerName,
    string? Status,
    DateOnly? FilingDate,
    DateOnly? GrantDate,
    Guid? LocalId);

public sealed class RegisterAndMonitorIPAssetCommandValidator : AbstractValidator<RegisterAndMonitorIPAssetCommand>
{
    public RegisterAndMonitorIPAssetCommandValidator()
    {
        RuleFor(x => x.Type).Must(IPAssetHelpers.IsValidType).WithMessage("Tipo de ativo de PI invalido.");
        RuleFor(x => x.Query).NotEmpty().MaximumLength(240);
    }
}

public sealed class RegisterAndMonitorIPAssetCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IInpiSearchService inpiSearchService) : IRequestHandler<RegisterAndMonitorIPAssetCommand, RegisterAndMonitorIPAssetResult>
{
    public async Task<RegisterAndMonitorIPAssetResult> Handle(RegisterAndMonitorIPAssetCommand request, CancellationToken cancellationToken)
    {
        await IPAssetHelpers.EnsureReferencesAsync(dbContext, request.ClientId, request.UniversityId, cancellationToken);

        var type = IPAssetHelpers.NormalizeType(request.Type);
        var query = request.Query.Trim();

        if (type == "Trademark")
        {
            return await RegisterTrademarkAsync(type, query, request.ClientId, request.UniversityId, cancellationToken);
        }

        if (type == "Patent")
        {
            return await RegisterPatentAsync(type, query, request.ClientId, request.UniversityId, cancellationToken);
        }

        var manualAsset = await CreateManualDraftAssetAsync(type, query, request.ClientId, request.UniversityId, cancellationToken);
        return new RegisterAndMonitorIPAssetResult("ManualDraftCreated", type, query, manualAsset.Id, null, null, null, false, null, "Tipo sem consulta INPI automatizada neste MVP; ativo salvo como Draft.", []);
    }

    private async Task<RegisterAndMonitorIPAssetResult> RegisterTrademarkAsync(string type, string query, Guid? clientId, Guid? universityId, CancellationToken cancellationToken)
    {
        var response = await inpiSearchService.SearchTrademarksBasicAsync(
            new InpiTrademarkSearchRequest(query, false, null, null, null, null, null, null, null, null, null, 1, 10),
            cancellationToken);

        if (response.TotalItems > 1)
        {
            return new RegisterAndMonitorIPAssetResult("MultipleResults", type, query, null, null, null, null, false, response.Source.ToString(), response.Warning, response.Items.Select(ToCandidate).ToList());
        }

        var result = response.Items.FirstOrDefault();
        if (result is null)
        {
            var asset = await CreateManualDraftAssetAsync(type, query, clientId, universityId, cancellationToken);
            return new RegisterAndMonitorIPAssetResult("ManualDraftCreated", type, query, asset.Id, null, null, null, false, response.Source.ToString(), "Nenhuma marca encontrada; ativo salvo como Draft para cadastro manual.", []);
        }

        var trademark = await UpsertTrademarkAsync(result, cancellationToken);
        var assetId = await UpsertTrademarkIPAssetAsync(trademark, result, clientId, universityId, true, cancellationToken);
        var monitoringId = await EnsureTrademarkMonitoringAsync(trademark, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new RegisterAndMonitorIPAssetResult("RegisteredAndMonitoringEnabled", type, query, assetId, trademark.Id, null, monitoringId, true, response.Source.ToString(), response.Warning, [ToCandidate(result)]);
    }

    private async Task<RegisterAndMonitorIPAssetResult> RegisterPatentAsync(string type, string query, Guid? clientId, Guid? universityId, CancellationToken cancellationToken)
    {
        var response = await inpiSearchService.SearchPatentsBasicAsync(
            new InpiPatentSearchRequest(query, false, null, null, null, null, null, null, null, null, null, 1, 10),
            cancellationToken);

        if (response.TotalItems > 1)
        {
            return new RegisterAndMonitorIPAssetResult("MultipleResults", type, query, null, null, null, null, false, response.Source.ToString(), response.Warning, response.Items.Select(ToCandidate).ToList());
        }

        var result = response.Items.FirstOrDefault();
        Patent patent;
        string? warning = response.Warning;

        if (result is null)
        {
            patent = await UpsertManualPatentAsync(query, cancellationToken);
            warning = "Nenhuma patente encontrada; patente minima criada como Draft para acompanhamento manual.";
        }
        else
        {
            patent = await UpsertPatentAsync(result, cancellationToken);
        }

        var assetId = await UpsertPatentIPAssetAsync(patent, clientId, universityId, true, cancellationToken);
        var monitoringId = await EnsurePatentMonitoringAsync(patent, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new RegisterAndMonitorIPAssetResult(
            result is null ? "ManualDraftCreatedAndMonitoringEnabled" : "RegisteredAndMonitoringEnabled",
            type,
            query,
            assetId,
            null,
            patent.Id,
            monitoringId,
            true,
            response.Source.ToString(),
            warning,
            result is null ? [] : [ToCandidate(result)]);
    }

    private async Task<IPAsset> CreateManualDraftAssetAsync(string type, string query, Guid? clientId, Guid? universityId, CancellationToken cancellationToken)
    {
        var asset = new IPAsset
        {
            Id = Guid.NewGuid(),
            Type = type,
            InpiProcessNumber = LooksLikeProcessNumber(query) ? query : null,
            Title = query,
            Status = "Draft",
            ClientId = clientId,
            UniversityId = universityId,
            IsMonitored = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            IsActive = true
        };

        dbContext.IPAssets.Add(asset);
        await dbContext.SaveChangesAsync(cancellationToken);
        return asset;
    }

    private async Task<Trademark> UpsertTrademarkAsync(InpiTrademarkResult result, CancellationToken cancellationToken)
    {
        var trademark = await dbContext.Trademarks
            .Include(x => x.Owner)
            .Include(x => x.Status)
            .SingleOrDefaultAsync(x => x.ProcessNumber == result.ProcessNumber, cancellationToken);

        if (trademark is null)
        {
            trademark = new Trademark
            {
                Id = Guid.NewGuid(),
                ProcessNumber = result.ProcessNumber,
                Name = result.Name,
                FilingDate = result.FilingDate,
                RegistrationDate = result.RegistrationDate,
                CreatedAtUtc = DateTime.UtcNow
            };
            dbContext.Trademarks.Add(trademark);
            return trademark;
        }

        trademark.Name = result.Name;
        trademark.FilingDate = result.FilingDate ?? trademark.FilingDate;
        trademark.RegistrationDate = result.RegistrationDate ?? trademark.RegistrationDate;
        return trademark;
    }

    private async Task<Guid> UpsertTrademarkIPAssetAsync(Trademark trademark, InpiTrademarkResult result, Guid? clientId, Guid? universityId, bool isMonitored, CancellationToken cancellationToken)
    {
        var asset = await dbContext.IPAssets.SingleOrDefaultAsync(x => x.Type == "Trademark" && x.InpiProcessNumber == trademark.ProcessNumber && x.IsActive, cancellationToken);
        var ownerName = result.Owners.Count == 0 ? trademark.Owner?.Name : string.Join(", ", result.Owners);

        if (asset is null)
        {
            asset = new IPAsset
            {
                Id = Guid.NewGuid(),
                Type = "Trademark",
                InpiProcessNumber = trademark.ProcessNumber,
                CreatedAtUtc = DateTime.UtcNow,
                IsActive = true
            };
            dbContext.IPAssets.Add(asset);
        }

        asset.Title = trademark.Name;
        asset.OwnerName = ownerName;
        asset.Status = result.Status ?? trademark.Status?.Description ?? "Status nao importado";
        asset.FilingDate = result.FilingDate ?? trademark.FilingDate;
        asset.GrantDate = result.RegistrationDate ?? trademark.RegistrationDate;
        asset.ClientId = clientId ?? asset.ClientId;
        asset.UniversityId = universityId ?? asset.UniversityId;
        asset.IsMonitored = isMonitored;
        asset.UpdatedAtUtc = DateTime.UtcNow;
        return asset.Id;
    }

    private async Task<Guid> EnsureTrademarkMonitoringAsync(Trademark trademark, CancellationToken cancellationToken)
    {
        var existing = await dbContext.MonitoredTrademarks.SingleOrDefaultAsync(x => x.UserId == currentUser.UserId && x.TrademarkId == trademark.Id, cancellationToken);
        if (existing is not null)
        {
            existing.IsActive = true;
            existing.LastCheckedAtUtc = DateTime.UtcNow;
            return existing.Id;
        }

        var latestDispatch = await dbContext.TrademarkDispatches
            .AsNoTracking()
            .Where(x => x.TrademarkId == trademark.Id)
            .OrderByDescending(x => x.PublishedAt)
            .ThenByDescending(x => x.RpiNumber ?? 0)
            .ThenByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var monitor = new MonitoredTrademark
        {
            Id = Guid.NewGuid(),
            UserId = currentUser.UserId,
            TrademarkId = trademark.Id,
            ProcessNumber = trademark.ProcessNumber,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            LastCheckedAtUtc = DateTime.UtcNow,
            LastKnownDispatchId = latestDispatch?.Id,
            LastKnownDispatchCode = latestDispatch?.Code,
            LastKnownDispatchDate = latestDispatch?.PublishedAt,
            HasPendingChanges = false
        };

        dbContext.MonitoredTrademarks.Add(monitor);
        return monitor.Id;
    }

    private async Task<Patent> UpsertManualPatentAsync(string query, CancellationToken cancellationToken)
    {
        var processNumber = LooksLikeProcessNumber(query) ? query : $"MANUAL-{Guid.NewGuid():N}"[..19];
        var patent = await dbContext.Patents.SingleOrDefaultAsync(x => x.InpiProcessNumber == processNumber, cancellationToken);
        if (patent is not null)
        {
            return patent;
        }

        patent = new Patent
        {
            Id = Guid.NewGuid(),
            InpiProcessNumber = processNumber,
            Title = query,
            Status = "Draft",
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            IsActive = true
        };
        dbContext.Patents.Add(patent);
        return patent;
    }

    private async Task<Patent> UpsertPatentAsync(InpiPatentResult result, CancellationToken cancellationToken)
    {
        var processNumber = result.InpiProcessNumber ?? $"MANUAL-{Guid.NewGuid():N}"[..19];
        var patent = await dbContext.Patents.SingleOrDefaultAsync(x => x.InpiProcessNumber == processNumber, cancellationToken);

        if (patent is null)
        {
            patent = new Patent
            {
                Id = Guid.NewGuid(),
                InpiProcessNumber = processNumber,
                CreatedAtUtc = DateTime.UtcNow,
                IsActive = true
            };
            dbContext.Patents.Add(patent);
        }

        patent.Title = result.Title;
        patent.Abstract = result.Abstract;
        patent.Applicants = result.Applicants.Count == 0 ? null : string.Join("; ", result.Applicants);
        patent.Inventors = result.Inventors.Count == 0 ? null : string.Join("; ", result.Inventors);
        patent.IpcClass = result.IpcClass;
        patent.FilingDate = result.FilingDate;
        patent.PublicationDate = result.PublicationDate;
        patent.GrantDate = result.GrantDate;
        patent.Status = result.Status ?? "Status nao importado";
        patent.UpdatedAtUtc = DateTime.UtcNow;
        return patent;
    }

    private async Task<Guid> UpsertPatentIPAssetAsync(Patent patent, Guid? clientId, Guid? universityId, bool isMonitored, CancellationToken cancellationToken)
    {
        var asset = await dbContext.IPAssets.SingleOrDefaultAsync(x => x.Type == "Patent" && x.InpiProcessNumber == patent.InpiProcessNumber && x.IsActive, cancellationToken);
        if (asset is null)
        {
            asset = new IPAsset
            {
                Id = Guid.NewGuid(),
                Type = "Patent",
                InpiProcessNumber = patent.InpiProcessNumber,
                CreatedAtUtc = DateTime.UtcNow,
                IsActive = true
            };
            dbContext.IPAssets.Add(asset);
        }

        asset.Title = patent.Title;
        asset.OwnerName = patent.Applicants;
        asset.Status = patent.Status ?? "Draft";
        asset.FilingDate = patent.FilingDate;
        asset.GrantDate = patent.GrantDate;
        asset.ClientId = clientId ?? asset.ClientId;
        asset.UniversityId = universityId ?? asset.UniversityId;
        asset.IsMonitored = isMonitored;
        asset.UpdatedAtUtc = DateTime.UtcNow;
        return asset.Id;
    }

    private async Task<Guid> EnsurePatentMonitoringAsync(Patent patent, CancellationToken cancellationToken)
    {
        var existing = await dbContext.MonitoredPatents.SingleOrDefaultAsync(x => x.UserId == currentUser.UserId && x.PatentId == patent.Id, cancellationToken);
        if (existing is not null)
        {
            existing.IsActive = true;
            existing.LastCheckedAtUtc = DateTime.UtcNow;
            return existing.Id;
        }

        var latestDispatch = await dbContext.PatentDispatches
            .AsNoTracking()
            .Where(x => x.PatentId == patent.Id)
            .OrderByDescending(x => x.DispatchDate)
            .ThenByDescending(x => x.RpiNumber ?? 0)
            .ThenByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var monitor = new MonitoredPatent
        {
            Id = Guid.NewGuid(),
            UserId = currentUser.UserId,
            PatentId = patent.Id,
            InpiProcessNumber = patent.InpiProcessNumber,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            LastCheckedAtUtc = DateTime.UtcNow,
            LastKnownDispatchId = latestDispatch?.Id,
            LastKnownDispatchCode = latestDispatch?.DispatchCode,
            LastKnownDispatchDate = latestDispatch?.DispatchDate,
            HasPendingChanges = false
        };

        dbContext.MonitoredPatents.Add(monitor);
        return monitor.Id;
    }

    private static IPAssetCandidateDto ToCandidate(InpiTrademarkResult result)
    {
        return new IPAssetCandidateDto(
            "Trademark",
            result.ProcessNumber,
            result.Name,
            result.Owners.Count == 0 ? null : string.Join(", ", result.Owners),
            result.Status,
            result.FilingDate,
            result.RegistrationDate,
            result.LocalId);
    }

    private static IPAssetCandidateDto ToCandidate(InpiPatentResult result)
    {
        return new IPAssetCandidateDto(
            "Patent",
            result.InpiProcessNumber,
            result.Title,
            result.Applicants.Count == 0 ? null : string.Join(", ", result.Applicants),
            result.Status,
            result.FilingDate,
            result.GrantDate,
            result.LocalId);
    }

    private static bool LooksLikeProcessNumber(string value)
    {
        var digits = new string(value.Where(char.IsDigit).ToArray());
        return digits.Length >= 6 && digits.Length >= value.Length / 2;
    }
}
