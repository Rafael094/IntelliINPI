using IntelliINPI.Application.Abstractions;
using IntelliINPI.Domain.Entities;
using IntelliINPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.RegularExpressions;

namespace IntelliINPI.Infrastructure.Services;

public sealed class InpiSearchService(ApplicationDbContext dbContext, ILogger<InpiSearchService> logger) : IInpiSearchService
{
    private const string OnlineFallbackWarning = "Busca online do INPI indisponivel ou sem resultados seguros; retornando fallback local controlado.";
    private const string InpiBaseUrl = "https://busca.inpi.gov.br";

    public async Task<InpiSearchResponse<InpiTrademarkResult>> SearchTrademarksBasicAsync(InpiTrademarkSearchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var online = await SearchOnlineInpiTrademarksAsync(request, advanced: false, cancellationToken);
            if (online is not null)
            {
                return online;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "INPI online trademark basic search failed. Falling back to local database. Query={Query}", request.Query);
        }

        return await SearchLocalTrademarksAsync(request, InpiSearchResultSource.OnlineFailedLocalFallback, OnlineFallbackWarning, cancellationToken);
    }

    public async Task<InpiSearchResponse<InpiTrademarkResult>> SearchTrademarksAdvancedAsync(InpiTrademarkSearchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var online = await SearchOnlineInpiTrademarksAsync(request, advanced: true, cancellationToken);
            if (online is not null)
            {
                return online;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "INPI online trademark advanced search failed. Falling back to local database. Process={ProcessNumber} Name={Name}", request.ProcessNumber, request.TrademarkName);
        }

        return await SearchLocalTrademarksAsync(request, InpiSearchResultSource.OnlineFailedLocalFallback, OnlineFallbackWarning, cancellationToken);
    }

    public Task<InpiSearchResponse<InpiTrademarkResult>> SearchTrademarksBooleanAsync(InpiTrademarkSearchRequest request, CancellationToken cancellationToken)
    {
        logger.LogWarning("INPI online trademark boolean search unavailable. Falling back to local database. Expression={Expression}", request.Expression);
        return SearchLocalTrademarksAsync(request, InpiSearchResultSource.OnlineFailedLocalFallback, OnlineFallbackWarning, cancellationToken);
    }

    public async Task<InpiSearchResponse<InpiPatentResult>> SearchPatentsBasicAsync(InpiPatentSearchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var online = await SearchOnlineInpiPatentsAsync(request, advanced: false, cancellationToken);
            if (online is not null)
            {
                return online;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "INPI online patent basic search failed. Falling back to local database. Query={Query}", request.Query);
        }

        return await SearchLocalPatentsAsync(request, InpiSearchResultSource.OnlineFailedLocalFallback, OnlineFallbackWarning, cancellationToken);
    }

    public async Task<InpiSearchResponse<InpiPatentResult>> SearchPatentsAdvancedAsync(InpiPatentSearchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var online = await SearchOnlineInpiPatentsAsync(request, advanced: true, cancellationToken);
            if (online is not null)
            {
                return online;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "INPI online patent advanced search failed. Falling back to local database. Process={ProcessNumber} Title={Title}", request.ProcessNumber, request.Title);
        }

        return await SearchLocalPatentsAsync(request, InpiSearchResultSource.OnlineFailedLocalFallback, OnlineFallbackWarning, cancellationToken);
    }

    public Task<InpiSearchResponse<InpiPatentResult>> SearchPatentsBooleanAsync(InpiPatentSearchRequest request, CancellationToken cancellationToken)
    {
        logger.LogWarning("INPI online patent boolean search unavailable. Falling back to local database. Expression={Expression}", request.Expression);
        return SearchLocalPatentsAsync(request, InpiSearchResultSource.OnlineFailedLocalFallback, OnlineFallbackWarning, cancellationToken);
    }

    public async Task SyncTrademarkDetailAsync(string processNumber, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(processNumber))
        {
            return;
        }

        try
        {
            var detail = await FetchTrademarkDetailHtmlAsync(processNumber.Trim(), cancellationToken);
            if (detail is null)
            {
                return;
            }

            await UpsertTrademarkDetailAsync(
                processNumber.Trim(),
                detail.Value.Html,
                detail.Value.DetailUrl,
                detail.Value.LogoBytes,
                detail.Value.LogoContentType,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "INPI trademark detail sync failed. Process={ProcessNumber}", processNumber);
        }
    }

    public async Task<InpiTrademarkResult?> GetTrademarkDetailAsync(string processNumber, CancellationToken cancellationToken)
    {
        var response = await SearchLocalTrademarksAsync(
            new InpiTrademarkSearchRequest(processNumber, true, processNumber, null, null, null, null, null, null, null, null, 1, 1),
            InpiSearchResultSource.LocalDatabase,
            null,
            cancellationToken);

        return response.Items.FirstOrDefault();
    }

    private async Task<(string Html, string DetailUrl, byte[]? LogoBytes, string? LogoContentType)?> FetchTrademarkDetailHtmlAsync(string processNumber, CancellationToken cancellationToken)
    {
        using var handler = new HttpClientHandler
        {
            CookieContainer = new CookieContainer(),
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
        };
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(InpiBaseUrl),
            Timeout = TimeSpan.FromSeconds(45)
        };

        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) IntelliINPI/1.0");
        httpClient.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

        await httpClient.GetAsync("/pePI/servlet/LoginController?action=login", cancellationToken);
        await httpClient.GetAsync("/pePI/jsp/marcas/Pesquisa_num_processo.jsp", cancellationToken);

        var form = new Dictionary<string, string>
        {
            ["NumPedido"] = processNumber,
            ["NumGRU"] = string.Empty,
            ["NumProtocolo"] = string.Empty,
            ["NumInscricaoInternacional"] = string.Empty,
            ["botao"] = " pesquisar » ",
            ["Action"] = "searchMarca",
            ["tipoPesquisa"] = "BY_NUM_PROC"
        };

        using var searchResponse = await httpClient.PostAsync("/pePI/servlet/MarcasServletController", new FormUrlEncodedContent(form), cancellationToken);
        if (!searchResponse.IsSuccessStatusCode)
        {
            return null;
        }

        var searchHtml = await searchResponse.Content.ReadAsStringAsync(cancellationToken);
        if (LooksLikeLoginPage(searchHtml))
        {
            return null;
        }

        var detailUrl = ExtractDetailUrl(searchHtml);
        if (string.IsNullOrWhiteSpace(detailUrl))
        {
            return null;
        }

        using var detailResponse = await httpClient.GetAsync(detailUrl, cancellationToken);
        if (!detailResponse.IsSuccessStatusCode)
        {
            return null;
        }

        var detailHtml = await detailResponse.Content.ReadAsStringAsync(cancellationToken);
        if (LooksLikeLoginPage(detailHtml) || detailHtml.Contains("Pedido inexistente", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        byte[]? logoBytes = null;
        string? logoContentType = null;
        var logoUrl = ExtractLogoUrl(detailHtml);
        if (!string.IsNullOrWhiteSpace(logoUrl))
        {
            try
            {
                using var logoResponse = await httpClient.GetAsync(logoUrl, cancellationToken);
                if (logoResponse.IsSuccessStatusCode)
                {
                    logoBytes = await logoResponse.Content.ReadAsByteArrayAsync(cancellationToken);
                    logoContentType = logoResponse.Content.Headers.ContentType?.MediaType;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "INPI trademark logo download failed. Process={ProcessNumber}", processNumber);
            }
        }

        return (detailHtml, detailUrl, logoBytes, logoContentType);
    }

    private async Task UpsertTrademarkDetailAsync(
        string processNumber,
        string html,
        string detailUrl,
        byte[]? logoBytes,
        string? logoContentType,
        CancellationToken cancellationToken)
    {
        var trademark = await dbContext.Trademarks
            .SingleOrDefaultAsync(x => x.ProcessNumber == processNumber, cancellationToken);

        if (trademark is null)
        {
            trademark = new Trademark
            {
                Id = Guid.NewGuid(),
                ProcessNumber = processNumber,
                Name = "Sem nome",
                CreatedAtUtc = DateTime.UtcNow
            };
            dbContext.Trademarks.Add(trademark);
        }

        var name = ExtractDetailLabelValue(html, "Marca");
        if (!string.IsNullOrWhiteSpace(name))
        {
            trademark.Name = TrimTo(name, 200);
        }

        var status = ExtractDetailLabelValue(html, "Situação");
        if (!string.IsNullOrWhiteSpace(status))
        {
            trademark.StatusId = await ResolveOnlineStatusIdAsync(status, cancellationToken);
        }

        trademark.Presentation = CoalesceTrim(ExtractDetailLabelValue(html, "Apresentação"), trademark.Presentation, 80);
        trademark.Nature = CoalesceTrim(ExtractDetailLabelValue(html, "Natureza"), trademark.Nature, 120);
        trademark.LegalRepresentative = CoalesceTrim(ExtractRepresentative(html), trademark.LegalRepresentative, 200);
        trademark.InpiDetailUrl = detailUrl;
        if (logoBytes is { Length: > 0 })
        {
            var logo = await SaveTrademarkLogoAsync(processNumber, logoBytes, logoContentType, cancellationToken);
            trademark.LogoPath = logo.Path;
            trademark.LogoContentType = logo.ContentType;
        }

        var dates = ExtractDateRow(html);
        trademark.FilingDate = dates.FilingDate ?? trademark.FilingDate;
        trademark.RegistrationDate = dates.RegistrationDate ?? trademark.RegistrationDate;
        trademark.ExpirationDate = dates.ExpirationDate ?? trademark.ExpirationDate;

        await dbContext.SaveChangesAsync(cancellationToken);
        await SyncOnlineOwnersAsync(trademark.Id, ExtractOwners(html), cancellationToken);
        await SyncOnlineNiceClassesAsync(trademark.Id, ExtractOnlineNiceClasses(html), cancellationToken);
        await SyncOnlineViennaClassesAsync(trademark.Id, ExtractOnlineViennaClasses(html), cancellationToken);
        await SyncOnlinePetitionsAsync(trademark.Id, ExtractOnlinePetitions(html), cancellationToken);
        await SyncOnlineDispatchesAsync(trademark.Id, ExtractOnlinePublications(html), cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<(string Path, string ContentType)> SaveTrademarkLogoAsync(
        string processNumber,
        byte[] logoBytes,
        string? contentType,
        CancellationToken cancellationToken)
    {
        var safeProcess = Regex.Replace(processNumber, "[^0-9A-Za-z_-]", string.Empty);
        var normalizedContentType = string.IsNullOrWhiteSpace(contentType) ? "image/jpeg" : contentType;
        var extension = normalizedContentType.Contains("png", StringComparison.OrdinalIgnoreCase)
            ? ".png"
            : normalizedContentType.Contains("gif", StringComparison.OrdinalIgnoreCase)
                ? ".gif"
                : ".jpg";
        var directory = Path.Combine("data", "inpi", "trademarks", "logos");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, $"{safeProcess}{extension}");
        await File.WriteAllBytesAsync(path, logoBytes, cancellationToken);
        return (path, normalizedContentType);
    }

    private async Task<Guid> ResolveOnlineStatusIdAsync(string description, CancellationToken cancellationToken)
    {
        var normalized = TrimTo(description, 200);
        var existing = await dbContext.TrademarkStatuses
            .FirstOrDefaultAsync(x => x.Description == normalized, cancellationToken);

        if (existing is not null)
        {
            return existing.Id;
        }

        var codeBase = Regex.Replace(normalized.ToUpperInvariant(), "[^A-Z0-9]+", "");
        var code = TrimTo(string.IsNullOrWhiteSpace(codeBase) ? "INPI" : $"INPI{codeBase}", 40);
        var suffix = 1;
        while (await dbContext.TrademarkStatuses.AnyAsync(x => x.Code == code, cancellationToken))
        {
            code = TrimTo($"{codeBase}{suffix++}", 40);
        }

        var status = new TrademarkStatus
        {
            Id = Guid.NewGuid(),
            Code = code,
            Description = normalized
        };
        dbContext.TrademarkStatuses.Add(status);
        await dbContext.SaveChangesAsync(cancellationToken);
        return status.Id;
    }

    private async Task SyncOnlineOwnersAsync(Guid trademarkId, IReadOnlyList<string> owners, CancellationToken cancellationToken)
    {
        foreach (var ownerName in owners.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var owner = await dbContext.TrademarkOwners.FirstOrDefaultAsync(x => x.Name == ownerName, cancellationToken);
            if (owner is null)
            {
                owner = new TrademarkOwner { Id = Guid.NewGuid(), Name = TrimTo(ownerName, 200) };
                dbContext.TrademarkOwners.Add(owner);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            if (!await dbContext.TrademarkOwnerLinks.AnyAsync(x => x.TrademarkId == trademarkId && x.OwnerId == owner.Id, cancellationToken))
            {
                dbContext.TrademarkOwnerLinks.Add(new TrademarkOwnerLink { Id = Guid.NewGuid(), TrademarkId = trademarkId, OwnerId = owner.Id });
            }
        }
    }

    private async Task SyncOnlineNiceClassesAsync(Guid trademarkId, IReadOnlyList<OnlineNiceClass> classes, CancellationToken cancellationToken)
    {
        foreach (var item in classes)
        {
            var existing = await dbContext.TrademarkNiceClasses
                .FirstOrDefaultAsync(x => x.TrademarkId == trademarkId && x.Code == item.Code, cancellationToken);

            if (existing is null)
            {
                dbContext.TrademarkNiceClasses.Add(new TrademarkNiceClass
                {
                    Id = Guid.NewGuid(),
                    TrademarkId = trademarkId,
                    Code = item.Code,
                    ClassNumber = int.TryParse(item.Code, out var number) ? number : 0,
                    Specification = item.Specification
                });
            }
            else if (!string.IsNullOrWhiteSpace(item.Specification))
            {
                existing.Specification = TrimTo(item.Specification, 1000);
            }
        }
    }

    private async Task SyncOnlineViennaClassesAsync(Guid trademarkId, IReadOnlyList<OnlineViennaClass> classes, CancellationToken cancellationToken)
    {
        foreach (var item in classes)
        {
            var existing = await dbContext.TrademarkViennaClasses
                .FirstOrDefaultAsync(x => x.TrademarkId == trademarkId && x.Edition == item.Edition && x.Code == item.Code, cancellationToken);

            if (existing is null)
            {
                dbContext.TrademarkViennaClasses.Add(new TrademarkViennaClass
                {
                    Id = Guid.NewGuid(),
                    TrademarkId = trademarkId,
                    Edition = item.Edition,
                    Code = item.Code,
                    Description = item.Description
                });
            }
            else if (!string.IsNullOrWhiteSpace(item.Description))
            {
                existing.Description = TrimTo(item.Description, 300);
            }
        }
    }

    private async Task SyncOnlinePetitionsAsync(Guid trademarkId, IReadOnlyList<OnlinePetition> petitions, CancellationToken cancellationToken)
    {
        foreach (var item in petitions)
        {
            if (string.IsNullOrWhiteSpace(item.Protocol))
            {
                continue;
            }

            var existing = await dbContext.TrademarkPetitions
                .FirstOrDefaultAsync(x => x.TrademarkId == trademarkId && x.Protocol == item.Protocol, cancellationToken);

            if (existing is null)
            {
                dbContext.TrademarkPetitions.Add(new TrademarkPetition
                {
                    Id = Guid.NewGuid(),
                    TrademarkId = trademarkId,
                    Protocol = item.Protocol,
                    FiledAt = item.FiledAt,
                    ServiceCode = item.ServiceCode,
                    ClientName = item.ClientName,
                    Delivery = item.Delivery,
                    DeliveryDate = item.DeliveryDate
                });
            }
            else
            {
                existing.FiledAt = item.FiledAt ?? existing.FiledAt;
                existing.ServiceCode = item.ServiceCode ?? existing.ServiceCode;
                existing.ClientName = item.ClientName ?? existing.ClientName;
                existing.Delivery = item.Delivery ?? existing.Delivery;
                existing.DeliveryDate = item.DeliveryDate ?? existing.DeliveryDate;
            }
        }
    }

    private async Task SyncOnlineDispatchesAsync(Guid trademarkId, IReadOnlyList<OnlinePublication> publications, CancellationToken cancellationToken)
    {
        foreach (var item in publications)
        {
            var code = TrimTo(item.Dispatch, 40);
            if (string.IsNullOrWhiteSpace(code) || !item.PublishedAt.HasValue)
            {
                continue;
            }

            var exists = await dbContext.TrademarkDispatches
                .AnyAsync(x => x.TrademarkId == trademarkId && x.RpiNumber == item.RpiNumber && x.Code == code, cancellationToken);

            if (!exists)
            {
                dbContext.TrademarkDispatches.Add(new TrademarkDispatch
                {
                    Id = Guid.NewGuid(),
                    TrademarkId = trademarkId,
                    Code = code,
                    Description = TrimTo(string.IsNullOrWhiteSpace(item.Complement) ? item.Dispatch : $"{item.Dispatch} - {item.Complement}", 1000),
                    RpiNumber = item.RpiNumber,
                    PublishedAt = item.PublishedAt.Value
                });
            }
        }
    }

    public async Task<InpiPatentResult?> GetPatentDetailAsync(string processNumber, CancellationToken cancellationToken)
    {
        var response = await SearchLocalPatentsAsync(
            new InpiPatentSearchRequest(processNumber, true, processNumber, null, null, null, null, null, null, null, null, 1, 1),
            InpiSearchResultSource.LocalDatabase,
            null,
            cancellationToken);

        return response.Items.FirstOrDefault();
    }

    private async Task<InpiSearchResponse<InpiPatentResult>?> SearchOnlineInpiPatentsAsync(
        InpiPatentSearchRequest request,
        bool advanced,
        CancellationToken cancellationToken)
    {
        using var handler = new HttpClientHandler
        {
            CookieContainer = new CookieContainer(),
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
        };
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(InpiBaseUrl),
            Timeout = TimeSpan.FromSeconds(60)
        };

        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) IntelliINPI/1.0");
        httpClient.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

        using var loginResponse = await httpClient.GetAsync("/pePI/servlet/LoginController?action=login", cancellationToken);
        if (!loginResponse.IsSuccessStatusCode)
        {
            return null;
        }

        var formUrl = advanced
            ? "/pePI/jsp/patentes/PatenteSearchAvancado.jsp"
            : "/pePI/jsp/patentes/PatenteSearchBasico.jsp";
        using var formResponse = await httpClient.GetAsync(formUrl, cancellationToken);
        if (!formResponse.IsSuccessStatusCode)
        {
            return null;
        }

        var pageSize = NormalizeInpiPageSize(request.PageSize);
        var form = advanced
            ? BuildAdvancedPatentForm(request, pageSize)
            : BuildBasicPatentForm(request, pageSize);

        using var response = await httpClient.PostAsync(
            "/pePI/servlet/PatenteServletController",
            new FormUrlEncodedContent(form),
            cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var html = await response.Content.ReadAsStringAsync(cancellationToken);
        if (LooksLikeLoginPage(html))
        {
            logger.LogWarning("INPI returned login page while searching patents online.");
            return null;
        }

        var parsedItems = ParsePatentSearchRows(html);
        var totalItems = ExtractTotalItems(html);
        if (parsedItems.Count == 0 && totalItems is null)
        {
            return null;
        }

        var localIds = await UpsertOnlinePatentsAsync(parsedItems, cancellationToken);
        var items = parsedItems
            .Select(x => x with
            {
                LocalId = x.InpiProcessNumber is not null && localIds.TryGetValue(x.InpiProcessNumber, out var id)
                    ? id
                    : x.LocalId
            })
            .ToList();

        return new InpiSearchResponse<InpiPatentResult>(
            InpiSearchResultSource.OnlineInpi,
            items,
            Math.Max(1, request.Page),
            pageSize,
            totalItems ?? items.Count,
            null);
    }

    private static Dictionary<string, string> BuildBasicPatentForm(InpiPatentSearchRequest request, int pageSize)
    {
        var searchModes = new HashSet<string>(["todasPalavras", "expExata", "qualquerPalavra", "aproximacao"], StringComparer.Ordinal);
        var searchFields = new HashSet<string>(["Titulo", "Resumo", "NomeDepositante", "NomeInventor", "CpfCnpjDepositante"], StringComparer.Ordinal);
        var searchMode = searchModes.Contains(request.SearchMode ?? string.Empty)
            ? request.SearchMode!
            : request.Exact ? "expExata" : "todasPalavras";
        var searchField = searchFields.Contains(request.SearchField ?? string.Empty) ? request.SearchField! : "Titulo";

        return new Dictionary<string, string>
        {
            ["NumPedido"] = request.ProcessNumber ?? string.Empty,
            ["NumGru"] = request.GruNumber ?? string.Empty,
            ["NumProtocolo"] = request.ProtocolNumber ?? string.Empty,
            ["FormaPesquisa"] = searchMode,
            ["ExpressaoPesquisa"] = request.Query ?? string.Empty,
            ["Coluna"] = searchField,
            ["RegisterPerPage"] = pageSize.ToString(),
            ["botao"] = " pesquisar » ",
            ["Action"] = "SearchBasico"
        };
    }

    private static Dictionary<string, string> BuildAdvancedPatentForm(InpiPatentSearchRequest request, int pageSize)
    {
        var form = new Dictionary<string, string>
        {
            ["NumPedido"] = request.ProcessNumber ?? string.Empty,
            ["NumPrioridade"] = request.PriorityNumber ?? string.Empty,
            ["CodigoPct"] = request.PctNumber ?? string.Empty,
            ["DataDeposito1"] = FormatInpiDate(request.StartDate),
            ["DataDeposito2"] = FormatInpiDate(request.EndDate),
            ["DataPrioridade1"] = FormatInpiDate(request.PriorityStartDate),
            ["DataPrioridade2"] = FormatInpiDate(request.PriorityEndDate),
            ["DataDepositoPCT1"] = FormatInpiDate(request.PctDepositStartDate),
            ["DataDepositoPCT2"] = FormatInpiDate(request.PctDepositEndDate),
            ["DataPublicacaoPCT1"] = FormatInpiDate(request.PctPublicationStartDate),
            ["DataPublicacaoPCT2"] = FormatInpiDate(request.PctPublicationEndDate),
            ["ClassificacaoIPC"] = request.IpcClass ?? string.Empty,
            ["CatchWordIPC"] = request.IpcKeyword ?? string.Empty,
            ["Titulo"] = request.Title ?? string.Empty,
            ["Resumo"] = request.Abstract ?? string.Empty,
            ["NomeDepositante"] = request.Applicant ?? string.Empty,
            ["CpfCnpjDepositante"] = request.ApplicantDocument ?? string.Empty,
            ["NomeInventor"] = request.Inventor ?? string.Empty,
            ["RegisterPerPage"] = pageSize.ToString(),
            ["botao"] = " pesquisar » ",
            ["Action"] = "SearchAvancado"
        };

        if (request.GrantedOnly)
        {
            form["PesquisaPatente"] = "E";
        }

        return form;
    }

    private async Task<Dictionary<string, Guid>> UpsertOnlinePatentsAsync(
        IReadOnlyList<InpiPatentResult> items,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items.Where(x => !string.IsNullOrWhiteSpace(x.InpiProcessNumber)))
        {
            var processNumber = item.InpiProcessNumber!;
            var patent = await dbContext.Patents.SingleOrDefaultAsync(x => x.InpiProcessNumber == processNumber, cancellationToken);
            if (patent is null)
            {
                patent = new Patent
                {
                    Id = Guid.NewGuid(),
                    InpiProcessNumber = processNumber,
                    Title = item.Title,
                    Abstract = item.Abstract,
                    Applicants = JoinNames(item.Applicants),
                    Inventors = JoinNames(item.Inventors),
                    IpcClass = item.IpcClass,
                    FilingDate = item.FilingDate,
                    PublicationDate = item.PublicationDate,
                    GrantDate = item.GrantDate,
                    Status = item.Status,
                    CreatedAtUtc = DateTime.UtcNow,
                    IsActive = true
                };
                dbContext.Patents.Add(patent);
            }
            else
            {
                patent.Title = string.IsNullOrWhiteSpace(item.Title) ? patent.Title : item.Title;
                patent.IpcClass = string.IsNullOrWhiteSpace(item.IpcClass) ? patent.IpcClass : item.IpcClass;
                patent.FilingDate = item.FilingDate ?? patent.FilingDate;
                patent.UpdatedAtUtc = DateTime.UtcNow;
                patent.IsActive = true;
            }

            result[processNumber] = patent.Id;
        }

        if (items.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    private static int NormalizeInpiPageSize(int pageSize)
    {
        int[] allowed = [20, 40, 60, 80, 100];
        return allowed.Contains(pageSize) ? pageSize : 20;
    }

    private static string FormatInpiDate(DateOnly? date) => date?.ToString("dd/MM/yyyy") ?? string.Empty;

    private static string? JoinNames(IReadOnlyList<string> names) => names.Count == 0 ? null : string.Join("; ", names);

    private async Task<InpiSearchResponse<InpiTrademarkResult>?> SearchOnlineInpiTrademarksAsync(
        InpiTrademarkSearchRequest request,
        bool advanced,
        CancellationToken cancellationToken)
    {
        var trademarkName = request.TrademarkName ?? request.Query;
        if (string.IsNullOrWhiteSpace(trademarkName))
        {
            return null;
        }

        using var handler = new HttpClientHandler
        {
            CookieContainer = new CookieContainer(),
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
        };
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(InpiBaseUrl),
            Timeout = TimeSpan.FromSeconds(45)
        };

        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) IntelliINPI/1.0");
        httpClient.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

        await httpClient.GetAsync("/pePI/servlet/LoginController?action=login", cancellationToken);
        var formUrl = advanced
            ? "/pePI/jsp/marcas/Pesquisa_classe_avancada.jsp"
            : "/pePI/jsp/marcas/Pesquisa_classe_basica.jsp";
        await httpClient.GetAsync(formUrl, cancellationToken);

        var pageSize = Math.Clamp(request.PageSize, 20, 100);
        var form = advanced
            ? BuildAdvancedTrademarkForm(request, trademarkName, pageSize)
            : BuildBasicTrademarkForm(request, trademarkName, pageSize);

        using var response = await httpClient.PostAsync("/pePI/servlet/MarcasServletController", new FormUrlEncodedContent(form), cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var html = await response.Content.ReadAsStringAsync(cancellationToken);
        if (LooksLikeLoginPage(html))
        {
            logger.LogWarning("INPI returned login page while searching trademarks online.");
            return null;
        }

        var parsedItems = ParseTrademarkSearchRows(html);
        if (parsedItems.Count == 0)
        {
            return null;
        }

        var localIds = await UpsertOnlineTrademarksAsync(parsedItems, cancellationToken);
        var items = parsedItems
            .Select(x => x with { LocalId = localIds.TryGetValue(x.ProcessNumber, out var id) ? id : x.LocalId })
            .ToList();

        return new InpiSearchResponse<InpiTrademarkResult>(
            InpiSearchResultSource.OnlineInpi,
            items,
            Math.Max(1, request.Page),
            pageSize,
            ExtractTotalItems(html) ?? items.Count,
            null);
    }

    private static Dictionary<string, string> BuildBasicTrademarkForm(InpiTrademarkSearchRequest request, string trademarkName, int pageSize)
    {
        return new Dictionary<string, string>
        {
            ["buscaExata"] = request.Exact ? "sim" : "nao",
            ["txt"] = request.Exact ? "Pesquisa Exata" : "Pesquisa Radical",
            ["marca"] = trademarkName,
            ["classeInter"] = NormalizeNiceClassDigits(request.NiceClass),
            ["registerPerPage"] = pageSize.ToString(),
            ["botao"] = " pesquisar » ",
            ["Action"] = "searchMarca",
            ["tipoPesquisa"] = "BY_MARCA_CLASSIF_BASICA"
        };
    }

    private static Dictionary<string, string> BuildAdvancedTrademarkForm(InpiTrademarkSearchRequest request, string trademarkName, int pageSize)
    {
        var form = new Dictionary<string, string>
        {
            ["precisao"] = request.Exact ? "nao" : "sim",
            ["txt"] = request.Exact ? "Pesquisa Exata" : "Pesquisa Aproximação",
            ["marca"] = trademarkName,
            ["FormaApresentacao"] = string.IsNullOrWhiteSpace(request.Presentation) ? "0" : request.Presentation,
            ["FormaNatureza"] = string.IsNullOrWhiteSpace(request.Nature) ? "0" : request.Nature,
            ["classeInter"] = NormalizeNiceClassDigits(request.NiceClass),
            ["registerPerPage"] = pageSize.ToString(),
            ["botao"] = " pesquisar » ",
            ["Action"] = "searchMarca",
            ["tipoPesquisa"] = "BY_MARCA_CLASSIF_AVANCADA"
        };

        if (request.LiveOnly)
        {
            form["ListaTodosPedidos"] = "E";
        }

        return form;
    }

    private async Task<Dictionary<string, Guid>> UpsertOnlineTrademarksAsync(IReadOnlyList<InpiTrademarkResult> items, CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, Guid>();
        foreach (var item in items)
        {
            var trademark = await dbContext.Trademarks
                .SingleOrDefaultAsync(x => x.ProcessNumber == item.ProcessNumber, cancellationToken);

            if (trademark is null)
            {
                trademark = new Trademark
                {
                    Id = Guid.NewGuid(),
                    ProcessNumber = item.ProcessNumber,
                    Name = item.Name,
                    FilingDate = item.FilingDate,
                    RegistrationDate = item.RegistrationDate,
                    InpiDetailUrl = item.InpiDetailUrl,
                    CreatedAtUtc = DateTime.UtcNow
                };
                dbContext.Trademarks.Add(trademark);
            }
            else
            {
                trademark.Name = string.IsNullOrWhiteSpace(item.Name) ? trademark.Name : item.Name;
                trademark.FilingDate = item.FilingDate ?? trademark.FilingDate;
                trademark.RegistrationDate = item.RegistrationDate ?? trademark.RegistrationDate;
                trademark.InpiDetailUrl = item.InpiDetailUrl ?? trademark.InpiDetailUrl;
            }

            await UpsertOwnersAsync(trademark, item.Owners, cancellationToken);
            await UpsertNiceClassesAsync(trademark, item.NiceClasses, cancellationToken);
            result[item.ProcessNumber] = trademark.Id;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    private async Task UpsertOwnersAsync(Trademark trademark, IReadOnlyList<string> ownerNames, CancellationToken cancellationToken)
    {
        foreach (var ownerName in ownerNames.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var owner = await dbContext.TrademarkOwners.SingleOrDefaultAsync(x => x.Name == ownerName, cancellationToken);
            if (owner is null)
            {
                owner = new TrademarkOwner { Id = Guid.NewGuid(), Name = ownerName };
                dbContext.TrademarkOwners.Add(owner);
            }

            if (!await dbContext.TrademarkOwnerLinks.AnyAsync(x => x.TrademarkId == trademark.Id && x.OwnerId == owner.Id, cancellationToken))
            {
                dbContext.TrademarkOwnerLinks.Add(new TrademarkOwnerLink
                {
                    Id = Guid.NewGuid(),
                    TrademarkId = trademark.Id,
                    OwnerId = owner.Id
                });
            }
        }
    }

    private async Task UpsertNiceClassesAsync(Trademark trademark, IReadOnlyList<string> codes, CancellationToken cancellationToken)
    {
        foreach (var code in codes.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var exists = dbContext.TrademarkNiceClasses.Local.Any(x => x.TrademarkId == trademark.Id && x.Code == code)
                || await dbContext.TrademarkNiceClasses.AnyAsync(x => x.TrademarkId == trademark.Id && x.Code == code, cancellationToken);
            if (!exists)
            {
                dbContext.TrademarkNiceClasses.Add(new TrademarkNiceClass
                {
                    Id = Guid.NewGuid(),
                    TrademarkId = trademark.Id,
                    Code = code
                });
            }
        }
    }

    private async Task<InpiSearchResponse<InpiTrademarkResult>> SearchLocalTrademarksAsync(
        InpiTrademarkSearchRequest request,
        InpiSearchResultSource source,
        string? warning,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var query = dbContext.Trademarks.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var term = request.Query.Trim().ToLowerInvariant();
            query = request.Exact
                ? query.Where(x => x.ProcessNumber == request.Query || x.Name.ToLower() == term)
                : query.Where(x => x.ProcessNumber.Contains(request.Query) || x.Name.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(request.ProcessNumber))
        {
            var process = request.ProcessNumber.Trim();
            query = query.Where(x => x.ProcessNumber.Contains(process));
        }

        if (!string.IsNullOrWhiteSpace(request.TrademarkName))
        {
            var name = request.TrademarkName.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(request.Owner))
        {
            var owner = request.Owner.Trim().ToLowerInvariant();
            query = query.Where(x => x.OwnerLinks.Any(o => o.Owner.Name.ToLower().Contains(owner)) || (x.Owner != null && x.Owner.Name.ToLower().Contains(owner)));
        }

        if (!string.IsNullOrWhiteSpace(request.NiceClass))
        {
            var niceClass = NormalizeNiceClassCode(request.NiceClass);
            query = query.Where(x => x.NiceClasses.Any(c => c.Code == niceClass));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = request.Status.Trim().ToLowerInvariant();
            query = query.Where(x => x.Status != null && (x.Status.Code.ToLower().Contains(status) || x.Status.Description.ToLower().Contains(status)));
        }

        if (!string.IsNullOrWhiteSpace(request.DispatchCode))
        {
            var dispatchCode = request.DispatchCode.Trim();
            query = query.Where(x => x.Dispatches.Any(d => d.Code == dispatchCode));
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(x => x.FilingDate == null || x.FilingDate >= request.StartDate);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(x => x.FilingDate == null || x.FilingDate <= request.EndDate);
        }

        if (!string.IsNullOrWhiteSpace(request.Expression))
        {
            query = ApplyBooleanExpression(query, request.Expression);
        }

        var total = await query.CountAsync(cancellationToken);
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

        var items = rows.Select(x =>
            new InpiTrademarkResult(
                x.Id,
                x.ProcessNumber,
                x.Name,
                x.Status,
                x.NiceClasses,
                x.OwnerNames.Count > 0 ? x.OwnerNames : string.IsNullOrWhiteSpace(x.LegacyOwnerName) ? [] : [x.LegacyOwnerName],
                x.FilingDate,
                x.RegistrationDate,
                x.LastDispatchDate,
                x.InpiDetailUrl))
            .ToList();

        return new InpiSearchResponse<InpiTrademarkResult>(source, items, page, pageSize, total, warning);
    }

    private async Task<InpiSearchResponse<InpiPatentResult>> SearchLocalPatentsAsync(
        InpiPatentSearchRequest request,
        InpiSearchResultSource source,
        string? warning,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var query = dbContext.Patents.AsNoTracking().Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var term = request.Query.Trim().ToLowerInvariant();
            query = request.SearchField switch
            {
                "Resumo" => query.Where(x => x.Abstract != null && x.Abstract.ToLower().Contains(term)),
                "NomeDepositante" => query.Where(x => x.Applicants != null && x.Applicants.ToLower().Contains(term)),
                "NomeInventor" => query.Where(x => x.Inventors != null && x.Inventors.ToLower().Contains(term)),
                "CpfCnpjDepositante" => query.Where(_ => false),
                _ when request.Exact || request.SearchMode == "expExata" => query.Where(x => x.InpiProcessNumber == request.Query || x.Title.ToLower() == term),
                _ => query.Where(x => x.InpiProcessNumber.Contains(request.Query) || x.Title.ToLower().Contains(term))
            };
        }

        if (!string.IsNullOrWhiteSpace(request.ProcessNumber))
        {
            var process = request.ProcessNumber.Trim();
            query = query.Where(x => x.InpiProcessNumber.Contains(process));
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            var title = request.Title.Trim().ToLowerInvariant();
            query = query.Where(x => x.Title.ToLower().Contains(title));
        }

        if (!string.IsNullOrWhiteSpace(request.Applicant))
        {
            var applicant = request.Applicant.Trim().ToLowerInvariant();
            query = query.Where(x => x.Applicants != null && x.Applicants.ToLower().Contains(applicant));
        }

        if (!string.IsNullOrWhiteSpace(request.Inventor))
        {
            var inventor = request.Inventor.Trim().ToLowerInvariant();
            query = query.Where(x => x.Inventors != null && x.Inventors.ToLower().Contains(inventor));
        }

        if (!string.IsNullOrWhiteSpace(request.IpcClass))
        {
            var ipcClass = request.IpcClass.Trim().ToLowerInvariant();
            query = query.Where(x => x.IpcClass != null && x.IpcClass.ToLower().Contains(ipcClass));
        }

        if (!string.IsNullOrWhiteSpace(request.IpcKeyword))
        {
            var ipcKeyword = request.IpcKeyword.Trim().ToLowerInvariant();
            query = query.Where(x => x.IpcClass != null && x.IpcClass.ToLower().Contains(ipcKeyword));
        }

        if (!string.IsNullOrWhiteSpace(request.Abstract))
        {
            var abstractTerm = request.Abstract.Trim().ToLowerInvariant();
            query = query.Where(x => x.Abstract != null && x.Abstract.ToLower().Contains(abstractTerm));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = request.Status.Trim().ToLowerInvariant();
            query = query.Where(x => x.Status != null && x.Status.ToLower().Contains(status));
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(x => x.FilingDate == null || x.FilingDate >= request.StartDate);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(x => x.FilingDate == null || x.FilingDate <= request.EndDate);
        }

        if (!string.IsNullOrWhiteSpace(request.Expression))
        {
            query = ApplyPatentBooleanExpression(query, request.Expression);
        }

        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderBy(x => x.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.InpiProcessNumber,
                x.Title,
                x.FilingDate,
                x.GrantDate,
                x.Status,
                x.Applicants,
                x.Inventors,
                x.IpcClass,
                x.Abstract,
                x.PublicationDate
            })
            .ToListAsync(cancellationToken);

        var items = rows.Select(x => new InpiPatentResult(
                x.Id,
                x.InpiProcessNumber,
                x.Title,
                x.Abstract,
                SplitNames(x.Applicants),
                SplitNames(x.Inventors),
                x.IpcClass,
                x.FilingDate,
                x.PublicationDate,
                x.GrantDate,
                x.Status))
            .ToList();

        return new InpiSearchResponse<InpiPatentResult>(source, items, page, pageSize, total, warning);
    }

    private static IQueryable<Trademark> ApplyBooleanExpression(IQueryable<Trademark> query, string expression)
    {
        var terms = ExtractTerms(expression);
        foreach (var term in terms)
        {
            var current = term;
            query = query.Where(x => x.Name.ToLower().Contains(current) || x.ProcessNumber.Contains(current));
        }

        return query;
    }

    private static IQueryable<Patent> ApplyPatentBooleanExpression(IQueryable<Patent> query, string expression)
    {
        var terms = ExtractTerms(expression);
        foreach (var term in terms)
        {
            var current = term;
            query = query.Where(x => x.Title.ToLower().Contains(current) || x.InpiProcessNumber.Contains(current));
        }

        return query;
    }

    private static IReadOnlyList<string> ExtractTerms(string expression)
    {
        return expression
            .Split([' ', '"', '\'', '(', ')'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => !string.Equals(x, "AND", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(x, "OR", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(x, "NOT", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.ToLowerInvariant())
            .Distinct()
            .Take(8)
            .ToList();
    }

    private static string NormalizeNiceClassCode(string value)
    {
        var digits = new string(value.Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(digits) ? value.Trim() : digits.PadLeft(2, '0');
    }

    private static string NormalizeNiceClassDigits(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : new string(value.Where(char.IsDigit).ToArray());
    }

    private static IReadOnlyList<string> SplitNames(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }

    private static bool LooksLikeLoginPage(string html)
    {
        return html.Contains("name=\"T_Login\"", StringComparison.OrdinalIgnoreCase)
            || html.Contains("name=\"T_Senha\"", StringComparison.OrdinalIgnoreCase);
    }

    private sealed record OnlineNiceClass(string Code, string? Specification);
    private sealed record OnlineViennaClass(string Edition, string Code, string? Description);
    private sealed record OnlinePetition(string Protocol, DateOnly? FiledAt, string? ServiceCode, string? ClientName, string? Delivery, DateOnly? DeliveryDate);
    private sealed record OnlinePublication(int? RpiNumber, DateOnly? PublishedAt, string Dispatch, string? Complement);

    private static string TrimTo(string value, int maxLength)
    {
        var normalized = NormalizeWhitespace(value);
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string? CoalesceTrim(string? current, string? fallback, int maxLength)
    {
        return string.IsNullOrWhiteSpace(current) ? fallback : TrimTo(current, maxLength);
    }

    private static string? ExtractDetailLabelValue(string html, string label)
    {
        var pattern = $@"<font[^>]*>\s*{Regex.Escape(label)}:\s*</font>\s*</td>\s*<td[^>]*>[\s\S]*?<font[^>]*>\s*&nbsp;?(?<value>[\s\S]*?)</font>";
        var match = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
        return match.Success ? HtmlToText(match.Groups["value"].Value) : null;
    }

    private static string? ExtractRepresentative(string html)
    {
        var rows = ExtractSectionRows(html, "Representante Legal");
        return rows.Select(x => x.Count >= 2 ? x[1] : null).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
    }

    private static string? ExtractLogoUrl(string html)
    {
        var match = Regex.Match(html, @"<img[^>]+src\s*=\s*['""](?<src>[^'""]*LogoMarcasServletController[^'""]*)['""]", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return null;
        }

        var src = WebUtility.HtmlDecode(match.Groups["src"].Value);
        return src.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? src
            : src.StartsWith("/", StringComparison.Ordinal)
                ? $"{InpiBaseUrl}{src}"
                : $"{InpiBaseUrl}/pePI/{src.TrimStart('.', '/')}";
    }

    private static IReadOnlyList<string> ExtractOwners(string html)
    {
        return ExtractSectionRows(html, "Titulares")
            .Select(x => x.Count >= 2 ? x[1] : null)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => TrimTo(x!, 200))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static (DateOnly? FilingDate, DateOnly? RegistrationDate, DateOnly? ExpirationDate) ExtractDateRow(string html)
    {
        var row = ExtractSectionRows(html, "Datas")
            .FirstOrDefault(x => x.Count >= 3 && ParseBrazilianDate(x[0]).HasValue);

        return row is null
            ? (null, null, null)
            : (ParseBrazilianDate(row[0]), ParseBrazilianDate(row[1]), ParseBrazilianDate(row[2]));
    }

    private static IReadOnlyList<OnlineNiceClass> ExtractOnlineNiceClasses(string html)
    {
        return ExtractSectionRows(html, "Classificação de Produtos / Serviços")
            .Select(row =>
            {
                if (row.Count < 3)
                {
                    return null;
                }

                var codeMatch = Regex.Match(row[0], @"NCL\(\d+\)\s*(?<code>\d{1,2})", RegexOptions.IgnoreCase);
                if (!codeMatch.Success)
                {
                    return null;
                }

                var specification = CleanRepeatedText(row[2], "Especificação");
                return new OnlineNiceClass(codeMatch.Groups["code"].Value.PadLeft(2, '0'), string.IsNullOrWhiteSpace(specification) ? null : TrimTo(specification, 1000));
            })
            .Where(x => x is not null)
            .Select(x => x!)
            .DistinctBy(x => x.Code)
            .ToList();
    }

    private static IReadOnlyList<OnlineViennaClass> ExtractOnlineViennaClasses(string html)
    {
        return ExtractSectionRows(html, "Classificação Internacional de Viena")
            .Where(row => row.Count >= 3 && Regex.IsMatch(row[1], @"\d+\.\d+"))
            .Select(row => new OnlineViennaClass(TrimTo(row[0], 20), TrimTo(row[1], 40), TrimTo(row[2], 300)))
            .DistinctBy(x => $"{x.Edition}|{x.Code}")
            .ToList();
    }

    private static IReadOnlyList<OnlinePetition> ExtractOnlinePetitions(string html)
    {
        return ExtractSectionRows(html, "Petições")
            .Where(row => row.Count >= 7 && Regex.IsMatch(row[1], @"\d{6,}"))
            .Select(row => new OnlinePetition(
                TrimTo(row[1], 80),
                ParseBrazilianDate(row[2]),
                TrimTo(row.ElementAtOrDefault(5) ?? string.Empty, 40),
                TrimTo(row.ElementAtOrDefault(6) ?? string.Empty, 200),
                TrimTo(row.ElementAtOrDefault(7) ?? string.Empty, 80),
                ParseBrazilianDate(row.ElementAtOrDefault(8) ?? string.Empty)))
            .DistinctBy(x => x.Protocol)
            .ToList();
    }

    private static IReadOnlyList<OnlinePublication> ExtractOnlinePublications(string html)
    {
        return ExtractSectionRows(html, "Publicações")
            .Where(row => row.Count >= 3 && int.TryParse(row[0], out _))
            .Select(row => new OnlinePublication(
                int.TryParse(row[0], out var rpi) ? rpi : null,
                ParseBrazilianDate(row[1]),
                TrimTo(row[2], 200),
                TrimTo(row.ElementAtOrDefault(5) ?? string.Empty, 1000)))
            .Where(x => !string.IsNullOrWhiteSpace(x.Dispatch))
            .DistinctBy(x => $"{x.RpiNumber}|{x.PublishedAt}|{x.Dispatch}")
            .ToList();
    }

    private static IReadOnlyList<IReadOnlyList<string>> ExtractSectionRows(string html, string sectionTitle)
    {
        var start = html.IndexOf(sectionTitle, StringComparison.OrdinalIgnoreCase);
        if (start < 0)
        {
            return [];
        }

        var next = html.IndexOf("<div class=\"accordions\">", start + sectionTitle.Length, StringComparison.OrdinalIgnoreCase);
        var section = next > start ? html[start..next] : html[start..];
        return Regex.Matches(section, @"<tr[^>]*>(?<row>[\s\S]*?)</tr>", RegexOptions.IgnoreCase)
            .Select(match => Regex.Matches(match.Groups["row"].Value, @"<t[dh][^>]*>(?<cell>[\s\S]*?)</t[dh]>", RegexOptions.IgnoreCase)
                .Select(cell => CleanCell(HtmlToText(cell.Groups["cell"].Value)))
                .Where(cell => !string.IsNullOrWhiteSpace(cell))
                .ToList())
            .Where(cells => cells.Count > 0)
            .Cast<IReadOnlyList<string>>()
            .ToList();
    }

    private static string CleanCell(string value)
    {
        var cleaned = NormalizeWhitespace(value)
            .Replace("Leia-me", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("Informações do Banco:", string.Empty, StringComparison.OrdinalIgnoreCase);
        return NormalizeWhitespace(cleaned);
    }

    private static string CleanRepeatedText(string value, string marker)
    {
        var normalized = NormalizeWhitespace(value);
        var index = normalized.LastIndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (index >= 0)
        {
            normalized = normalized[(index + marker.Length)..];
        }

        return NormalizeWhitespace(normalized);
    }

    private static int? ExtractTotalItems(string html)
    {
        var match = Regex.Match(html, @"Foram encontrados(?:\s|<[^>]+>)*(\d+)", RegexOptions.IgnoreCase);
        return match.Success && int.TryParse(match.Groups[1].Value, out var total) ? total : null;
    }

    private static IReadOnlyList<InpiPatentResult> ParsePatentSearchRows(string html)
    {
        var results = new List<InpiPatentResult>();
        var rows = Regex.Matches(html, @"<tr[^>]*>([\s\S]*?)</tr>", RegexOptions.IgnoreCase);

        foreach (Match row in rows)
        {
            var rowHtml = row.Groups[1].Value;
            if (!rowHtml.Contains("PatenteServletController", StringComparison.OrdinalIgnoreCase)
                || !rowHtml.Contains("Action=detail", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var cells = Regex.Matches(rowHtml, @"<td[^>]*>([\s\S]*?)</td>", RegexOptions.IgnoreCase)
                .Select(x => x.Groups[1].Value)
                .ToList();
            if (cells.Count < 4)
            {
                continue;
            }

            var displayNumber = HtmlToText(cells[0]);
            var processNumber = Regex.Replace(displayNumber, "[^0-9A-Za-z]", string.Empty).ToUpperInvariant();
            if (processNumber.Length < 6)
            {
                continue;
            }

            var title = HtmlToText(cells[2]);
            var ipcClass = HtmlToText(cells[3]);
            results.Add(new InpiPatentResult(
                null,
                processNumber,
                string.IsNullOrWhiteSpace(title) ? "Sem título" : title,
                null,
                [],
                [],
                string.IsNullOrWhiteSpace(ipcClass) ? null : ipcClass,
                ParseBrazilianDate(HtmlToText(cells[1])),
                null,
                null,
                null));
        }

        return results
            .GroupBy(x => x.InpiProcessNumber, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .ToList();
    }

    private static IReadOnlyList<InpiTrademarkResult> ParseTrademarkSearchRows(string html)
    {
        var rows = Regex.Matches(html, @"<tr[^>]*bgColor\s*=\s*[""']?#E0E0E0[""']?[^>]*>([\s\S]*?)</tr>", RegexOptions.IgnoreCase);
        var results = new List<InpiTrademarkResult>();

        foreach (Match row in rows)
        {
            var rawCells = Regex.Matches(row.Groups[1].Value, @"<td[^>]*>([\s\S]*?)</td>", RegexOptions.IgnoreCase)
                .Select(x => x.Groups[1].Value)
                .ToList();
            var cells = rawCells.Select(HtmlToText).ToList();

            if (cells.Count < 8)
            {
                continue;
            }

            var processNumber = Regex.Match(cells[0], @"\d{6,}").Value;
            if (string.IsNullOrWhiteSpace(processNumber))
            {
                continue;
            }

            var brand = NormalizeWhitespace(cells[3]);
            var status = NormalizeWhitespace(cells[5]);
            var owners = SplitOwners(cells[6]);
            var niceClasses = ExtractNiceClasses(cells[7]);
            var detailUrl = ExtractDetailUrl(rawCells[0]);

            results.Add(new InpiTrademarkResult(
                null,
                processNumber,
                string.IsNullOrWhiteSpace(brand) ? "Sem nome" : brand,
                string.IsNullOrWhiteSpace(status) ? null : status,
                niceClasses,
                owners,
                ParseBrazilianDate(cells[1]),
                null,
                null,
                detailUrl));
        }

        return results;
    }

    private static string HtmlToText(string html)
    {
        var withoutTags = Regex.Replace(html, "<[^>]+>", " ");
        return NormalizeWhitespace(WebUtility.HtmlDecode(withoutTags));
    }

    private static string NormalizeWhitespace(string value)
    {
        return Regex.Replace(value, @"\s+", " ").Trim();
    }

    private static IReadOnlyList<string> SplitOwners(string value)
    {
        var normalized = NormalizeWhitespace(value);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return [];
        }

        var parts = Regex.Split(normalized, @"\s{2,}|;\s*")
            .Select(NormalizeWhitespace)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return parts.Count == 0 ? [normalized] : parts;
    }

    private static IReadOnlyList<string> ExtractNiceClasses(string value)
    {
        return Regex.Matches(value, @"NCL\(\d+\)\s*(\d+)|\b(\d{1,2})\b", RegexOptions.IgnoreCase)
            .Select(x => x.Groups[1].Success ? x.Groups[1].Value : x.Groups[2].Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.PadLeft(2, '0'))
            .Distinct()
            .ToList();
    }

    private static DateOnly? ParseBrazilianDate(string value)
    {
        var match = Regex.Match(value, @"\b(\d{2})/(\d{2})/(\d{4})\b");
        if (!match.Success)
        {
            return null;
        }

        return DateOnly.TryParseExact(match.Value, "dd/MM/yyyy", out var date) ? date : null;
    }

    private static string? ExtractDetailUrl(string html)
    {
        var match = Regex.Match(html, @"href\s*=\s*['""](?<href>[^'""]*Action=detail[^'""]*)['""]", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return null;
        }

        var href = WebUtility.HtmlDecode(match.Groups["href"].Value).Replace("../", "/pePI/", StringComparison.Ordinal);
        return href.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? href
            : $"{InpiBaseUrl}{href}";
    }
}
