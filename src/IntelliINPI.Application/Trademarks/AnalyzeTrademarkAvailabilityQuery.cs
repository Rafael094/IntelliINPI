using System.Globalization;
using System.Text;
using FluentValidation;
using IntelliINPI.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Application.Trademarks;

public sealed record AnalyzeTrademarkAvailabilityRequest(string ProposedName, string? ActivityDescription);

public sealed record AnalyzeTrademarkAvailabilityQuery(string ProposedName, string? ActivityDescription)
    : IRequest<TrademarkAvailabilityAnalysisDto>;

public sealed record TrademarkAvailabilityAnalysisDto(
    string ProposedName,
    string NormalizedBrand,
    string RiskLevel,
    string Summary,
    string ConflictSearchSource,
    string? ConflictSearchWarning,
    IReadOnlyList<NiceClassSuggestionDto> SuggestedClasses,
    IReadOnlyList<TrademarkConflictDto> LocalConflicts,
    IReadOnlyList<ExternalBrandPresenceResultDto> ExternalResults,
    IReadOnlyList<WebPresenceCheckDto> WebPresenceChecks);

public sealed record NiceClassSuggestionDto(
    string Code,
    string Title,
    string Reason,
    IReadOnlyList<string> MatchedKeywords);

public sealed record TrademarkConflictDto(
    Guid Id,
    string ProcessNumber,
    string Name,
    string? Status,
    IReadOnlyList<string> NiceClasses,
    IReadOnlyList<string> Owners,
    DateOnly? LastDispatchDate,
    int SimilarityScore,
    string ConflictReason);

public sealed record WebPresenceCheckDto(
    string Source,
    string Query,
    string Url,
    string Status,
    string Notes);

public sealed record ExternalBrandPresenceResultDto(
    string Source,
    string Query,
    string Title,
    string Url,
    string? Snippet,
    decimal? Score,
    string Category);

public sealed class AnalyzeTrademarkAvailabilityQueryValidator : AbstractValidator<AnalyzeTrademarkAvailabilityQuery>
{
    public AnalyzeTrademarkAvailabilityQueryValidator()
    {
        RuleFor(x => x.ProposedName).NotEmpty().MaximumLength(160);
        RuleFor(x => x.ActivityDescription).MaximumLength(500);
    }
}

public sealed class AnalyzeTrademarkAvailabilityQueryHandler(
    IApplicationDbContext dbContext,
    IInpiSearchService inpiSearchService,
    IExternalBrandSearchClient externalBrandSearchClient)
    : IRequestHandler<AnalyzeTrademarkAvailabilityQuery, TrademarkAvailabilityAnalysisDto>
{
    private static readonly NiceClassRule[] NiceClassRules =
    [
        new("09", "Software, midias digitais e conteudo baixavel", ["software", "aplicativo", "app", "plataforma", "download", "arquivo digital", "midia digital", "video baixavel"]),
        new("35", "Publicidade, gestao comercial e comercio", ["publicidade", "marketing", "agencia", "comercio", "loja", "varejo", "atacado", "marketplace", "gestao comercial"]),
        new("38", "Telecomunicacoes, transmissao e streaming", ["streaming", "transmissao", "canal", "broadcast", "podcast", "telecom", "comunicacao digital"]),
        new("41", "Educacao, entretenimento e producao audiovisual", ["filme", "filmes", "cinema", "audiovisual", "video", "videos", "produtora", "producao", "serie", "series", "documentario", "entretenimento", "canal"]),
        new("42", "Tecnologia, SaaS, sites e plataformas digitais", ["saas", "tecnologia", "site", "sistema", "desenvolvimento", "hospedagem", "software como servico", "plataforma digital"])
    ];

    public async Task<TrademarkAvailabilityAnalysisDto> Handle(AnalyzeTrademarkAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var proposedName = request.ProposedName.Trim();
        var normalizedBrand = NormalizeBrand(proposedName);
        var analysisText = NormalizeSearchText($"{proposedName} {request.ActivityDescription}");
        var meaningfulTokens = ExtractMeaningfulTokens(normalizedBrand);
        var suggestedClasses = SuggestNiceClasses(analysisText);
        var conflictSearch = await FindInpiFirstConflictsAsync(normalizedBrand, meaningfulTokens, cancellationToken);
        var conflicts = conflictSearch.Conflicts;
        var externalResults = await SafeFindExternalPresenceAsync(normalizedBrand, cancellationToken);
        var riskLevel = ResolveRiskLevel(conflicts);
        var summary = BuildSummary(normalizedBrand, riskLevel, suggestedClasses.Count, conflicts.Count, conflictSearch.Source);

        return new TrademarkAvailabilityAnalysisDto(
            proposedName,
            normalizedBrand,
            riskLevel,
            summary,
            conflictSearch.Source,
            conflictSearch.Warning,
            suggestedClasses,
            conflicts,
            externalResults,
            BuildWebPresenceChecks(normalizedBrand));
    }

    private async Task<ConflictSearchResult> FindInpiFirstConflictsAsync(
        string normalizedBrand,
        IReadOnlyList<string> meaningfulTokens,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await inpiSearchService.SearchTrademarksBasicAsync(
                new InpiTrademarkSearchRequest(
                    normalizedBrand,
                    Exact: false,
                    ProcessNumber: null,
                    TrademarkName: normalizedBrand,
                    Owner: null,
                    NiceClass: null,
                    Status: null,
                    DispatchCode: null,
                    StartDate: null,
                    EndDate: null,
                    Expression: null,
                    Page: 1,
                    PageSize: 100),
                cancellationToken);

            var conflicts = response.Items
                .Select(item =>
                {
                    var normalizedExisting = NormalizeBrand(item.Name);
                    var score = ScoreSimilarity(normalizedBrand, normalizedExisting);
                    return new TrademarkConflictDto(
                        item.LocalId ?? Guid.Empty,
                        item.ProcessNumber,
                        item.Name,
                        item.Status,
                        item.NiceClasses,
                        item.Owners,
                        item.LastDispatchDate,
                        score,
                        BuildConflictReason(normalizedBrand, normalizedExisting, score));
                })
                .Where(x => x.SimilarityScore >= 35)
                .OrderByDescending(x => x.SimilarityScore)
                .ThenBy(x => x.Name)
                .Take(25)
                .ToList();

            if (conflicts.Count > 0 || response.Source == InpiSearchResultSource.OnlineInpi)
            {
                return new ConflictSearchResult(
                    conflicts,
                    response.Source == InpiSearchResultSource.OnlineInpi ? "OnlineInpi" : "LocalDatabaseFallback",
                    response.Warning);
            }
        }
        catch
        {
            // Se o INPI falhar, a analise continua com o banco local.
        }

        var localConflicts = await FindLocalConflictsAsync(normalizedBrand, meaningfulTokens, cancellationToken);
        return new ConflictSearchResult(localConflicts, "LocalDatabase", "INPI online indisponivel ou sem retorno seguro; conflitos avaliados no banco local.");
    }

    private async Task<IReadOnlyList<ExternalBrandPresenceResultDto>> SafeFindExternalPresenceAsync(
        string normalizedBrand,
        CancellationToken cancellationToken)
    {
        try
        {
            return await FindExternalPresenceAsync(normalizedBrand, cancellationToken);
        }
        catch
        {
            return [];
        }
    }

    private async Task<IReadOnlyList<ExternalBrandPresenceResultDto>> FindExternalPresenceAsync(string normalizedBrand, CancellationToken cancellationToken)
    {
        var queries = new[]
        {
            ($"\"{normalizedBrand}\" Brasil empresa marca site:.br OR site:.com.br", "Web"),
            ($"\"{normalizedBrand}\" Brasil instagram site:instagram.com", "Instagram"),
            ($"\"{normalizedBrand}\" Brasil facebook site:facebook.com", "Facebook"),
            ($"\"{normalizedBrand}\" site:.com.br OR site:.br", "Domain")
        };

        var results = new List<ExternalBrandPresenceResultDto>();
        foreach (var (query, category) in queries)
        {
            try
            {
                var queryResults = await externalBrandSearchClient.SearchAsync(query, 5, cancellationToken);
                results.AddRange(queryResults
                    .Where(x => !string.IsNullOrWhiteSpace(x.Url))
                    .Select(x => new ExternalBrandPresenceResultDto(
                        CleanText(x.Source, 80),
                        CleanText(x.Query, 240),
                        CleanText(x.Title, 300),
                        CleanText(x.Url, 1000),
                        string.IsNullOrWhiteSpace(x.Snippet) ? null : CleanText(x.Snippet, 1000),
                        x.Score,
                        category)));
            }
            catch
            {
                // A busca externa nao pode impedir a analise local do INPI.
            }
        }

        return results
            .Where(IsBrazilRelevantResult)
            .GroupBy(x => x.Url, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.OrderByDescending(item => item.Score ?? 0).First())
            .Take(20)
            .ToList();
    }

    private static bool IsBrazilRelevantResult(ExternalBrandPresenceResultDto result)
    {
        var url = result.Url.ToLowerInvariant();
        var title = result.Title.ToLowerInvariant();
        var snippet = result.Snippet?.ToLowerInvariant() ?? string.Empty;
        var query = result.Query.ToLowerInvariant();

        if (url.Contains(".br/", StringComparison.OrdinalIgnoreCase)
            || url.EndsWith(".br", StringComparison.OrdinalIgnoreCase)
            || url.Contains("instagram.com", StringComparison.OrdinalIgnoreCase)
            || url.Contains("facebook.com", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var brazilSignals = new[]
        {
            "brasil", "brazil", "sao paulo", "são paulo", "rio de janeiro", "curitiba", "belo horizonte",
            "recife", "salvador", "porto alegre", "cnpj", "ltda", "me", "eireli", "com.br", "empresa brasileira"
        };

        return brazilSignals.Any(signal =>
            title.Contains(signal, StringComparison.OrdinalIgnoreCase)
            || snippet.Contains(signal, StringComparison.OrdinalIgnoreCase)
            || query.Contains(signal, StringComparison.OrdinalIgnoreCase));
    }

    private static string CleanText(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var cleaned = new string(value
            .Where(character => !char.IsControl(character) || character is '\t' or '\r' or '\n')
            .ToArray())
            .Trim();

        return cleaned.Length <= maxLength ? cleaned : cleaned[..maxLength];
    }

    private async Task<IReadOnlyList<TrademarkConflictDto>> FindLocalConflictsAsync(
        string normalizedBrand,
        IReadOnlyList<string> meaningfulTokens,
        CancellationToken cancellationToken)
    {
        if (meaningfulTokens.Count == 0)
        {
            return [];
        }

        var primaryToken = meaningfulTokens[0];
        var broadQuery = dbContext.Trademarks.AsNoTracking()
            .Where(x => x.Name.ToLower().Contains(primaryToken));

        foreach (var token in meaningfulTokens.Skip(1).Take(2))
        {
            broadQuery = broadQuery.Concat(dbContext.Trademarks.AsNoTracking()
                .Where(x => x.Name.ToLower().Contains(token)));
        }

        var candidateIds = await broadQuery
            .Distinct()
            .OrderBy(x => x.Name)
            .Select(x => x.Id)
            .Take(300)
            .ToListAsync(cancellationToken);

        var candidates = await dbContext.Trademarks
            .AsNoTracking()
            .Where(x => candidateIds.Contains(x.Id))
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.ProcessNumber,
                x.Name,
                Status = x.Status == null ? null : x.Status.Description,
                NiceClasses = x.NiceClasses.OrderBy(c => c.Code).Select(c => c.Code).ToList(),
                OwnerNames = x.OwnerLinks.OrderBy(o => o.Owner.Name).Select(o => o.Owner.Name).ToList(),
                LegacyOwnerName = x.Owner == null ? null : x.Owner.Name,
                LastDispatchDate = x.Dispatches.OrderByDescending(d => d.PublishedAt).Select(d => (DateOnly?)d.PublishedAt).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return candidates
            .Select(x =>
            {
                var normalizedExisting = NormalizeBrand(x.Name);
                var score = ScoreSimilarity(normalizedBrand, normalizedExisting);
                var owners = x.OwnerNames.Count > 0
                    ? x.OwnerNames
                    : string.IsNullOrWhiteSpace(x.LegacyOwnerName) ? new List<string>() : new List<string> { x.LegacyOwnerName };

                return new TrademarkConflictDto(
                    x.Id,
                    x.ProcessNumber,
                    x.Name,
                    x.Status,
                    x.NiceClasses,
                    owners,
                    x.LastDispatchDate,
                    score,
                    BuildConflictReason(normalizedBrand, normalizedExisting, score));
            })
            .Where(x => x.SimilarityScore >= 35)
            .OrderByDescending(x => x.SimilarityScore)
            .ThenBy(x => x.Name)
            .Take(25)
            .ToList();
    }

    private static IReadOnlyList<NiceClassSuggestionDto> SuggestNiceClasses(string analysisText)
    {
        var suggestions = NiceClassRules
            .Select(rule =>
            {
                var matches = rule.Keywords
                    .Where(keyword => analysisText.Contains(NormalizeSearchText(keyword), StringComparison.OrdinalIgnoreCase))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                return new { Rule = rule, Matches = matches };
            })
            .Where(x => x.Matches.Count > 0)
            .OrderByDescending(x => x.Matches.Count)
            .Select(x => new NiceClassSuggestionDto(
                x.Rule.Code,
                x.Rule.Title,
                BuildClassReason(x.Rule.Code),
                x.Matches))
            .ToList();

        return suggestions.Count > 0
            ? suggestions
            : [new NiceClassSuggestionDto("35", "Publicidade, gestao comercial e comercio", "Classe comum para atividades comerciais; valide com a especificacao real do servico.", [])];
    }

    private static IReadOnlyList<WebPresenceCheckDto> BuildWebPresenceChecks(string normalizedBrand)
    {
        var encoded = Uri.EscapeDataString($"\"{normalizedBrand}\"");
        var dashed = normalizedBrand.ToLowerInvariant().Replace(" ", "-");

        return
        [
            new("Google Brasil", normalizedBrand, $"https://www.google.com/search?q={encoded}&hl=pt-BR&gl=BR&pws=0", "ManualCheckRequired", "Verificar resultados exatos no Brasil, nomes fantasia, dominios e uso comercial."),
            new("Google - dominios BR", normalizedBrand, $"https://www.google.com/search?q={encoded}+site%3A.com.br+OR+site%3A.br&hl=pt-BR&gl=BR&pws=0", "ManualCheckRequired", "Ajuda a encontrar sites brasileiros que usam a expressao como marca ou nome comercial."),
            new("Instagram - Brasil", normalizedBrand, $"https://www.google.com/search?q={encoded}+site%3Ainstagram.com+Brasil&hl=pt-BR&gl=BR&pws=0", "ManualCheckRequired", "Localizar perfis brasileiros sem automatizar login ou scraping."),
            new("Facebook - Brasil", normalizedBrand, $"https://www.google.com/search?q={encoded}+site%3Afacebook.com+Brasil&hl=pt-BR&gl=BR&pws=0", "ManualCheckRequired", "Verificar paginas, perfis e grupos brasileiros com uso comercial do nome."),
            new("Registro.br", normalizedBrand, $"https://registro.br/tecnologia/ferramentas/whois?search={Uri.EscapeDataString(dashed + ".com.br")}", "ManualCheckRequired", "Verificar dominio .com.br relacionado ao nome.")
        ];
    }

    private static string BuildSummary(string normalizedBrand, string riskLevel, int classesCount, int conflictsCount, string source)
    {
        var sourceText = source == "OnlineInpi"
            ? "A busca de conflitos priorizou o INPI online."
            : "A busca de conflitos usou o banco local como fallback.";

        return conflictsCount == 0
            ? $"Nenhum conflito forte foi encontrado para {normalizedBrand}. Foram sugeridas {classesCount} classes para analise inicial. {sourceText}"
            : $"{conflictsCount} possiveis conflitos foram encontrados para {normalizedBrand}. Risco preliminar: {riskLevel}. {sourceText}";
    }

    private static string ResolveRiskLevel(IReadOnlyList<TrademarkConflictDto> conflicts)
    {
        if (conflicts.Any(x => x.SimilarityScore >= 85))
        {
            return "High";
        }

        if (conflicts.Any(x => x.SimilarityScore >= 60))
        {
            return "Medium";
        }

        return conflicts.Count > 0 ? "Low" : "NoLocalConflictFound";
    }

    private static int ScoreSimilarity(string proposed, string existing)
    {
        if (proposed == existing)
        {
            return 100;
        }

        if (existing.Contains(proposed) || proposed.Contains(existing))
        {
            return 82;
        }

        var proposedTokens = ExtractMeaningfulTokens(proposed).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existingTokens = ExtractMeaningfulTokens(existing).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (proposedTokens.Count == 0 || existingTokens.Count == 0)
        {
            return 0;
        }

        var overlap = proposedTokens.Intersect(existingTokens, StringComparer.OrdinalIgnoreCase).Count();
        var tokenScore = (int)Math.Round((double)overlap / Math.Max(proposedTokens.Count, existingTokens.Count) * 70);
        var prefixBonus = proposedTokens.Any(p => existingTokens.Any(e => e.StartsWith(p) || p.StartsWith(e))) ? 15 : 0;
        return Math.Min(95, tokenScore + prefixBonus);
    }

    private static string BuildConflictReason(string proposed, string existing, int score)
    {
        if (score >= 100)
        {
            return "Nome normalizado identico.";
        }

        if (score >= 80)
        {
            return "Nome muito proximo ou contendo a expressao principal.";
        }

        if (score >= 60)
        {
            return "Compartilha termos distintivos relevantes.";
        }

        return $"Similaridade baixa a moderada com {existing}.";
    }

    private static string BuildClassReason(string code) => code switch
    {
        "09" => "Pode ser relevante quando houver software, conteudo digital baixavel ou midias digitais.",
        "35" => "Pode ser relevante para publicidade, gestao comercial, comercio, agencia ou marketplace.",
        "38" => "Pode ser relevante para transmissao, telecomunicacao, streaming ou canais digitais.",
        "41" => "Pode ser relevante para entretenimento, producao audiovisual, filmes, videos e conteudo cultural.",
        "42" => "Pode ser relevante quando a marca identificar plataforma digital, tecnologia, SaaS ou desenvolvimento de software.",
        _ => "Classe sugerida por palavras-chave; valide a especificacao antes do deposito."
    };

    private static IReadOnlyList<string> ExtractMeaningfulTokens(string value)
    {
        var ignored = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ltda", "me", "eireli", "sa", "s.a", "epp", "mei", "do", "da", "de", "dos", "das", "the", "and", "com", "br"
        };

        return NormalizeSearchText(value)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => x.Length >= 3 && !ignored.Contains(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string NormalizeBrand(string value)
    {
        var tokens = ExtractMeaningfulTokens(value);
        return tokens.Count == 0 ? NormalizeSearchText(value) : string.Join(' ', tokens);
    }

    private static string NormalizeSearchText(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(char.IsLetterOrDigit(character) ? char.ToLowerInvariant(character) : ' ');
        }

        return string.Join(' ', builder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private sealed record NiceClassRule(string Code, string Title, IReadOnlyList<string> Keywords);
    private sealed record ConflictSearchResult(IReadOnlyList<TrademarkConflictDto> Conflicts, string Source, string? Warning);
}
