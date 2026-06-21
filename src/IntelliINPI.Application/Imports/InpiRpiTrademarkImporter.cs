using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using IntelliINPI.Application.Abstractions;
using IntelliINPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Application.Imports;

public sealed class InpiRpiTrademarkImporter(IApplicationDbContext dbContext, IInpiRpiClient rpiClient)
    : IInpiRpiTrademarkImporter
{
    private const int DispatchBatchSize = 500;

    public ImportSource Source => ImportSource.RpiXmlTxt;

    public Task<ImportTrademarksResult> ImportAsync(CancellationToken cancellationToken)
    {
        return ImportAsync(null, cancellationToken);
    }

    public async Task<ImportTrademarksResult> ImportAsync(int? rpiNumber, CancellationToken cancellationToken)
    {
        var job = new ImportJob
        {
            Id = Guid.NewGuid(),
            Source = "INPI RPI XML/TXT - Marcas",
            SourceType = Source,
            Status = "Running",
            StartedAtUtc = DateTime.UtcNow
        };

        dbContext.ImportJobs.Add(job);
        AddLog(job.Id, rpiNumber is null or 0
            ? "Importacao RPI de marcas iniciada usando ultima edicao disponivel."
            : $"Importacao RPI de marcas iniciada para edicao {rpiNumber}.");
        await dbContext.SaveChangesAsync(cancellationToken);

        var downloadedFiles = 0;
        var importedDispatches = 0;
        var duplicateDispatches = 0;
        var failedRows = 0;

        try
        {
            var download = await rpiClient.DownloadTrademarkFilesAsync(rpiNumber, LogDownloadAsync, cancellationToken);
            downloadedFiles = download.Files.Count;

            foreach (var file in download.Files)
            {
                AddLog(job.Id, $"RPI {download.RpiNumber}: arquivo baixado {file.Name}; URL={file.SourceUrl}; caminho={file.Path}.");
            }

            foreach (var file in download.Files.Where(IsProcessableFile))
            {
                var result = string.Equals(Path.GetExtension(file.Path), ".xml", StringComparison.OrdinalIgnoreCase)
                    ? await ImportXmlFileAsync(file, download.RpiNumber, job.Id, cancellationToken)
                    : await ImportTextFileAsync(file, download.RpiNumber, job.Id, cancellationToken);

                importedDispatches += result.ImportedDispatches;
                duplicateDispatches += result.DuplicateDispatches;
                failedRows += result.FailedRows;
            }

            var status = failedRows > 0 || duplicateDispatches > 0 || importedDispatches == 0
                ? "CompletedWithWarnings"
                : "Completed";
            var message = $"Importacao RPI {download.RpiNumber} finalizada. Despachos importados: {importedDispatches}. Duplicados: {duplicateDispatches}. Linhas falhas: {failedRows}.";
            var errorMessage = importedDispatches == 0 && duplicateDispatches == 0
                ? "Nenhum despacho de marca foi importado da RPI."
                : null;

            await MarkJobFinishedAsync(job.Id, status, message, cancellationToken);
            return new ImportTrademarksResult(job.Id, status, downloadedFiles, importedDispatches, failedRows, errorMessage)
            {
                SkippedRows = duplicateDispatches,
                DuplicateRows = duplicateDispatches
            };
        }
        catch (Exception exception)
        {
            var message = $"Falha na importacao RPI: {exception.Message}";
            await MarkJobFailedAsync(job.Id, message, cancellationToken);
            return new ImportTrademarksResult(job.Id, "Failed", downloadedFiles, importedDispatches, failedRows, message)
            {
                SkippedRows = duplicateDispatches,
                DuplicateRows = duplicateDispatches
            };
        }

        async Task LogDownloadAsync(string message, CancellationToken token)
        {
            AddLog(job.Id, message);
            await dbContext.SaveChangesAsync(token);
        }
    }

    private async Task<RpiFileImportResult> ImportXmlFileAsync(InpiRpiFile file, int rpiNumber, Guid jobId, CancellationToken cancellationToken)
    {
        var result = new RpiFileImportResult();
        var pendingDispatches = new List<TrademarkDispatch>(DispatchBatchSize);
        var seenDispatches = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using var stream = File.OpenRead(file.Path);
        using var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true, IgnoreComments = true, IgnoreWhitespace = true });

        DateOnly? rpiDate = null;
        while (await reader.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (reader.NodeType == XmlNodeType.Element && reader.Name == "revista")
            {
                rpiDate = ParseDate(reader.GetAttribute("data"));
            }

            if (reader.NodeType != XmlNodeType.Element || reader.Name != "processo")
            {
                continue;
            }

            result.ProcessesRead++;
            try
            {
                var processo = (XElement)await XNode.ReadFromAsync(reader, cancellationToken);
                var processResult = await QueueProcessDispatchesAsync(
                    processo,
                    rpiNumber,
                    rpiDate,
                    pendingDispatches,
                    seenDispatches,
                    cancellationToken);

                result.DuplicateDispatches += processResult.DuplicateDispatches;
                result.FailedRows += processResult.FailedRows;

                if (pendingDispatches.Count >= DispatchBatchSize)
                {
                    var batchResult = await SavePendingDispatchesAsync(pendingDispatches, jobId, rpiNumber, cancellationToken);
                    result.ImportedDispatches += batchResult.ImportedDispatches;
                    result.DuplicateDispatches += batchResult.DuplicateDispatches;
                    result.FailedRows += batchResult.FailedRows;
                }
            }
            catch (Exception exception)
            {
                result.ParseErrors++;
                result.FailedRows++;
                AddLog(jobId, $"{file.Name}: processo ignorado por erro de parsing: {exception.Message}");
            }
        }

        if (pendingDispatches.Count > 0)
        {
            var batchResult = await SavePendingDispatchesAsync(pendingDispatches, jobId, rpiNumber, cancellationToken);
            result.ImportedDispatches += batchResult.ImportedDispatches;
            result.DuplicateDispatches += batchResult.DuplicateDispatches;
            result.FailedRows += batchResult.FailedRows;
        }

        AddLog(jobId, $"{file.Name}: processos lidos={result.ProcessesRead}; despachos importados={result.ImportedDispatches}; duplicados={result.DuplicateDispatches}; linhas falhas={result.FailedRows}; erros parsing={result.ParseErrors}.");
        await dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    private Task<RpiFileImportResult> ImportTextFileAsync(InpiRpiFile file, int rpiNumber, Guid jobId, CancellationToken cancellationToken)
    {
        AddLog(jobId, $"{file.Name}: TXT simplificado detectado para RPI {rpiNumber}, mas o layout TXT de marcas ainda nao foi implementado. Arquivo mantido para processamento futuro.");
        return Task.FromResult(new RpiFileImportResult());
    }

    private async Task<RpiProcessImportResult> QueueProcessDispatchesAsync(
        XElement processo,
        int rpiNumber,
        DateOnly? rpiDate,
        List<TrademarkDispatch> pendingDispatches,
        HashSet<string> seenDispatches,
        CancellationToken cancellationToken)
    {
        var processNumber = (string?)processo.Attribute("numero");
        if (string.IsNullOrWhiteSpace(processNumber))
        {
            return new RpiProcessImportResult(0, 1);
        }

        var trademarkId = await FindOrCreateTrademarkIdAsync(processo, processNumber.Trim(), cancellationToken);
        var duplicateDispatches = 0;
        var failedRows = 0;

        foreach (var despacho in processo.Descendants("despacho"))
        {
            var code = (string?)despacho.Attribute("codigo");
            if (string.IsNullOrWhiteSpace(code))
            {
                failedRows++;
                continue;
            }

            var normalizedCode = TrimTo(code, 40);
            var key = $"{trademarkId:N}|{rpiNumber}|{normalizedCode}";
            if (!seenDispatches.Add(key))
            {
                duplicateDispatches++;
                continue;
            }

            var exists = await dbContext.TrademarkDispatches
                .AsNoTracking()
                .AnyAsync(
                    x => x.TrademarkId == trademarkId && x.RpiNumber == rpiNumber && x.Code == normalizedCode,
                    cancellationToken);

            if (exists)
            {
                duplicateDispatches++;
                continue;
            }

            pendingDispatches.Add(new TrademarkDispatch
            {
                Id = Guid.NewGuid(),
                TrademarkId = trademarkId,
                Code = normalizedCode,
                Description = TrimTo(BuildDispatchDescription(despacho), 1000),
                RpiNumber = rpiNumber,
                PublishedAt = rpiDate ?? DateOnly.FromDateTime(DateTime.UtcNow)
            });
        }

        return new RpiProcessImportResult(duplicateDispatches, failedRows);
    }

    private async Task<RpiBatchSaveResult> SavePendingDispatchesAsync(
        List<TrademarkDispatch> pendingDispatches,
        Guid jobId,
        int rpiNumber,
        CancellationToken cancellationToken)
    {
        if (pendingDispatches.Count == 0)
        {
            return new RpiBatchSaveResult();
        }

        var dispatches = pendingDispatches.ToList();
        pendingDispatches.Clear();

        dbContext.TrademarkDispatches.AddRange(dispatches);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            ClearChangeTracker();
            return new RpiBatchSaveResult { ImportedDispatches = dispatches.Count };
        }
        catch (DbUpdateConcurrencyException exception)
        {
            var message = $"RPI {rpiNumber}: DbUpdateConcurrencyException ao salvar lote de TrademarkDispatch. Entidades={DescribeEntries(exception)}. O lote sera reprocessado item a item.";
            ClearChangeTracker();
            AddLog(jobId, message);
            await dbContext.SaveChangesAsync(cancellationToken);
            return await SaveDispatchesIndividuallyAsync(dispatches, jobId, rpiNumber, cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            var message = $"RPI {rpiNumber}: DbUpdateException ao salvar lote de TrademarkDispatch: {exception.GetBaseException().Message}. O lote sera reprocessado item a item.";
            ClearChangeTracker();
            AddLog(jobId, message);
            await dbContext.SaveChangesAsync(cancellationToken);
            return await SaveDispatchesIndividuallyAsync(dispatches, jobId, rpiNumber, cancellationToken);
        }
    }

    private async Task<RpiBatchSaveResult> SaveDispatchesIndividuallyAsync(
        IReadOnlyList<TrademarkDispatch> dispatches,
        Guid jobId,
        int rpiNumber,
        CancellationToken cancellationToken)
    {
        var result = new RpiBatchSaveResult();

        foreach (var dispatch in dispatches)
        {
            var exists = await dbContext.TrademarkDispatches
                .AsNoTracking()
                .AnyAsync(
                    x => x.TrademarkId == dispatch.TrademarkId && x.RpiNumber == dispatch.RpiNumber && x.Code == dispatch.Code,
                    cancellationToken);

            if (exists)
            {
                result.DuplicateDispatches++;
                continue;
            }

            dbContext.TrademarkDispatches.Add(dispatch);
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                result.ImportedDispatches++;
            }
            catch (DbUpdateConcurrencyException exception)
            {
                ClearChangeTracker();
                AddLog(jobId, $"RPI {rpiNumber}: DbUpdateConcurrencyException em TrademarkDispatch {dispatch.TrademarkId}/{dispatch.RpiNumber}/{dispatch.Code}. Entidades={DescribeEntries(exception)}. Tratado como duplicidade recuperavel.");
                await dbContext.SaveChangesAsync(cancellationToken);
                result.DuplicateDispatches++;
            }
            catch (DbUpdateException exception) when (IsDuplicateException(exception))
            {
                result.DuplicateDispatches++;
            }
            catch (DbUpdateException exception)
            {
                ClearChangeTracker();
                AddLog(jobId, $"RPI {rpiNumber}: falha real ao salvar TrademarkDispatch {dispatch.TrademarkId}/{dispatch.RpiNumber}/{dispatch.Code}: {exception.GetBaseException().Message}");
                await dbContext.SaveChangesAsync(cancellationToken);
                result.FailedRows++;
            }
            finally
            {
                ClearChangeTracker();
            }
        }

        return result;
    }

    private async Task<Guid> FindOrCreateTrademarkIdAsync(XElement processo, string processNumber, CancellationToken cancellationToken)
    {
        var trademark = await dbContext.Trademarks
            .SingleOrDefaultAsync(x => x.ProcessNumber == processNumber, cancellationToken);

        if (trademark is null)
        {
            trademark = new Trademark
            {
                Id = Guid.NewGuid(),
                ProcessNumber = processNumber,
                Name = string.Empty,
                StatusId = await ResolveUnknownStatusIdAsync(cancellationToken),
                CreatedAtUtc = DateTime.UtcNow
            };

            dbContext.Trademarks.Add(trademark);
        }

        await ApplyRpiTrademarkMetadataAsync(trademark, processo, cancellationToken);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await SyncRpiOwnersAsync(trademark.Id, processo, cancellationToken);
            await SyncRpiNiceClassesAsync(trademark.Id, processo, cancellationToken);
            await SyncRpiViennaClassesAsync(trademark.Id, processo, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            ClearChangeTracker();
            return trademark.Id;
        }
        catch (DbUpdateException)
        {
            ClearChangeTracker();
            var existingId = await dbContext.Trademarks
                .AsNoTracking()
                .Where(x => x.ProcessNumber == processNumber)
                .Select(x => (Guid?)x.Id)
                .SingleOrDefaultAsync(cancellationToken);

            if (existingId.HasValue)
            {
                return existingId.Value;
            }

            throw;
        }
    }

    private async Task ApplyRpiTrademarkMetadataAsync(Trademark trademark, XElement processo, CancellationToken cancellationToken)
    {
        var name = TrimTo((string?)processo.Element("marca")?.Element("nome") ?? string.Empty, 200);
        if (!string.IsNullOrWhiteSpace(name))
        {
            trademark.Name = name;
        }

        trademark.FilingDate ??= ParseDate((string?)processo.Attribute("data-deposito"));
        trademark.RegistrationDate ??= ParseDate((string?)processo.Attribute("data-concessao"));
        trademark.ExpirationDate ??= ParseDate((string?)processo.Attribute("data-vigencia"));

        var marca = processo.Element("marca");
        var presentation = TrimTo((string?)marca?.Attribute("apresentacao") ?? string.Empty, 80);
        if (!string.IsNullOrWhiteSpace(presentation))
        {
            trademark.Presentation = presentation;
        }

        var nature = TrimTo((string?)marca?.Attribute("natureza") ?? string.Empty, 120);
        if (!string.IsNullOrWhiteSpace(nature))
        {
            trademark.Nature = nature;
        }

        var legalRepresentative = TrimTo((string?)processo.Element("procurador") ?? string.Empty, 200);
        if (!string.IsNullOrWhiteSpace(legalRepresentative))
        {
            trademark.LegalRepresentative = legalRepresentative;
        }

        var statusDescription = processo
            .Descendants("classe-nice")
            .Select(x => (string?)x.Element("status"))
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

        if (!string.IsNullOrWhiteSpace(statusDescription))
        {
            trademark.StatusId = await ResolveRpiStatusIdAsync(statusDescription, cancellationToken);
        }
    }

    private async Task SyncRpiOwnersAsync(Guid trademarkId, XElement processo, CancellationToken cancellationToken)
    {
        var ownerNames = processo
            .Descendants("titular")
            .Select(x => TrimTo((string?)x.Attribute("nome-razao-social") ?? string.Empty, 200))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        for (var index = 0; index < ownerNames.Count; index++)
        {
            var owner = await FindOrCreateOwnerAsync(ownerNames[index], cancellationToken);
            if (index == 0)
            {
                var trademark = await dbContext.Trademarks.SingleAsync(x => x.Id == trademarkId, cancellationToken);
                trademark.OwnerId ??= owner.Id;
            }

            var exists = await dbContext.TrademarkOwnerLinks
                .AsNoTracking()
                .AnyAsync(x => x.TrademarkId == trademarkId && x.OwnerId == owner.Id, cancellationToken);

            if (!exists)
            {
                dbContext.TrademarkOwnerLinks.Add(new TrademarkOwnerLink
                {
                    Id = Guid.NewGuid(),
                    TrademarkId = trademarkId,
                    OwnerId = owner.Id
                });
            }
        }
    }

    private async Task SyncRpiNiceClassesAsync(Guid trademarkId, XElement processo, CancellationToken cancellationToken)
    {
        foreach (var classe in processo.Descendants("classe-nice"))
        {
            var code = TrimTo((string?)classe.Attribute("codigo") ?? string.Empty, 20);
            if (string.IsNullOrWhiteSpace(code))
            {
                continue;
            }

            var normalizedCode = NormalizeNiceClassCode(code);
            var exists = await dbContext.TrademarkNiceClasses
                .AsNoTracking()
                .AnyAsync(x => x.TrademarkId == trademarkId && x.Code == normalizedCode, cancellationToken);

            if (exists)
            {
                continue;
            }

            dbContext.TrademarkNiceClasses.Add(new TrademarkNiceClass
            {
                Id = Guid.NewGuid(),
                TrademarkId = trademarkId,
                Code = normalizedCode,
                ClassNumber = int.TryParse(normalizedCode, out var classNumber) ? classNumber : 0,
                Specification = TrimTo((string?)classe.Element("especificacao") ?? string.Empty, 1000)
            });
        }
    }

    private async Task SyncRpiViennaClassesAsync(Guid trademarkId, XElement processo, CancellationToken cancellationToken)
    {
        foreach (var classe in processo.Descendants("classe-vienna"))
        {
            var code = TrimTo((string?)classe.Attribute("codigo") ?? string.Empty, 40);
            if (string.IsNullOrWhiteSpace(code))
            {
                continue;
            }

            var edition = TrimTo((string?)classe.Attribute("edicao") ?? string.Empty, 20);
            if (string.IsNullOrWhiteSpace(edition))
            {
                edition = "Nao informado";
            }

            var exists = await dbContext.TrademarkViennaClasses
                .AsNoTracking()
                .AnyAsync(x => x.TrademarkId == trademarkId && x.Edition == edition && x.Code == code, cancellationToken);

            if (exists)
            {
                continue;
            }

            dbContext.TrademarkViennaClasses.Add(new TrademarkViennaClass
            {
                Id = Guid.NewGuid(),
                TrademarkId = trademarkId,
                Edition = edition,
                Code = code
            });
        }
    }

    private async Task<TrademarkOwner> FindOrCreateOwnerAsync(string name, CancellationToken cancellationToken)
    {
        var normalizedName = TrimTo(name, 200);
        var owner = await dbContext.TrademarkOwners
            .Where(x => x.Name == normalizedName)
            .OrderBy(x => x.Document == null ? 0 : 1)
            .FirstOrDefaultAsync(cancellationToken);

        if (owner is not null)
        {
            return owner;
        }

        owner = new TrademarkOwner
        {
            Id = Guid.NewGuid(),
            Name = normalizedName
        };
        dbContext.TrademarkOwners.Add(owner);
        await dbContext.SaveChangesAsync(cancellationToken);
        return owner;
    }

    private async Task<Guid> ResolveRpiStatusIdAsync(string description, CancellationToken cancellationToken)
    {
        var normalizedDescription = TrimTo(description, 200);
        var codeSuffix = new string(normalizedDescription
            .ToUpperInvariant()
            .Where(x => char.IsLetterOrDigit(x))
            .Take(32)
            .ToArray());
        var code = string.IsNullOrWhiteSpace(codeSuffix) ? "RPI_STATUS" : $"RPI_{codeSuffix}";

        var existingId = await dbContext.TrademarkStatuses
            .AsNoTracking()
            .Where(x => x.Code == code)
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (existingId.HasValue)
        {
            return existingId.Value;
        }

        var status = new TrademarkStatus
        {
            Id = Guid.NewGuid(),
            Code = code,
            Description = normalizedDescription
        };

        dbContext.TrademarkStatuses.Add(status);
        await dbContext.SaveChangesAsync(cancellationToken);
        return status.Id;
    }

    private async Task<Guid> ResolveUnknownStatusIdAsync(CancellationToken cancellationToken)
    {
        const string code = "UNKNOWN";
        var existingId = await dbContext.TrademarkStatuses
            .AsNoTracking()
            .Where(x => x.Code == code)
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (existingId.HasValue)
        {
            return existingId.Value;
        }

        var status = new TrademarkStatus
        {
            Id = Guid.NewGuid(),
            Code = code,
            Description = "Status nao informado pela RPI"
        };

        dbContext.TrademarkStatuses.Add(status);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            ClearChangeTracker();
            return status.Id;
        }
        catch (DbUpdateException)
        {
            ClearChangeTracker();
            existingId = await dbContext.TrademarkStatuses
                .AsNoTracking()
                .Where(x => x.Code == code)
                .Select(x => (Guid?)x.Id)
                .SingleOrDefaultAsync(cancellationToken);

            if (existingId.HasValue)
            {
                return existingId.Value;
            }

            throw;
        }
    }

    private async Task MarkJobFinishedAsync(Guid jobId, string status, string message, CancellationToken cancellationToken)
    {
        var job = await dbContext.ImportJobs.SingleOrDefaultAsync(x => x.Id == jobId, cancellationToken);
        if (job is null)
        {
            return;
        }

        job.Status = status;
        job.FinishedAtUtc = DateTime.UtcNow;
        AddLog(jobId, message);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task MarkJobFailedAsync(Guid jobId, string message, CancellationToken cancellationToken)
    {
        ClearChangeTracker();

        var job = await dbContext.ImportJobs.SingleOrDefaultAsync(x => x.Id == jobId, cancellationToken);
        if (job is null)
        {
            return;
        }

        job.Status = "Failed";
        job.FinishedAtUtc = DateTime.UtcNow;
        AddLog(jobId, message);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private void AddLog(Guid jobId, string message)
    {
        dbContext.ImportJobLogs.Add(new ImportJobLog
        {
            Id = Guid.NewGuid(),
            ImportJobId = jobId,
            Message = TrimTo(message, 2000),
            CreatedAtUtc = DateTime.UtcNow
        });
    }

    private void ClearChangeTracker()
    {
        if (dbContext is DbContext efContext)
        {
            efContext.ChangeTracker.Clear();
        }
    }

    private static bool IsDuplicateException(DbUpdateException exception)
    {
        var message = exception.GetBaseException().Message;
        return message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase)
            || message.Contains("23505", StringComparison.OrdinalIgnoreCase)
            || message.Contains("IX_TrademarkDispatches_TrademarkId_RpiNumber_Code", StringComparison.OrdinalIgnoreCase);
    }

    private static string DescribeEntries(DbUpdateConcurrencyException exception)
    {
        var names = exception.Entries
            .Select(x => x.Metadata.ClrType.Name)
            .Distinct()
            .ToArray();

        return names.Length == 0 ? "desconhecida" : string.Join(",", names);
    }

    private static bool IsProcessableFile(InpiRpiFile file)
    {
        var extension = Path.GetExtension(file.Path);
        return string.Equals(extension, ".xml", StringComparison.OrdinalIgnoreCase)
            || string.Equals(extension, ".txt", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildDispatchDescription(XElement despacho)
    {
        var name = (string?)despacho.Attribute("nome");
        var complement = string.Join(" ", despacho.Descendants().Select(x => x.Value).Where(x => !string.IsNullOrWhiteSpace(x)));
        var value = string.IsNullOrWhiteSpace(complement) ? despacho.Value : complement;

        if (string.IsNullOrWhiteSpace(value))
        {
            return string.IsNullOrWhiteSpace(name) ? "Despacho RPI" : name;
        }

        return string.IsNullOrWhiteSpace(name) ? value : $"{name} - {value}";
    }

    private static DateOnly? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var formats = new[] { "dd/MM/yyyy", "yyyy-MM-dd", "dd-MM-yyyy", "yyyyMMdd" };
        return DateOnly.TryParseExact(value.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;
    }

    private static string NormalizeNiceClassCode(string value)
    {
        var digits = new string(value.Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(digits) ? value.Trim() : digits.PadLeft(2, '0');
    }

    private static string TrimTo(string value, int maxLength)
    {
        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    private sealed class RpiFileImportResult
    {
        public int ProcessesRead { get; set; }
        public int ImportedDispatches { get; set; }
        public int DuplicateDispatches { get; set; }
        public int FailedRows { get; set; }
        public int ParseErrors { get; set; }
    }

    private sealed class RpiBatchSaveResult
    {
        public int ImportedDispatches { get; set; }
        public int DuplicateDispatches { get; set; }
        public int FailedRows { get; set; }
    }

    private sealed record RpiProcessImportResult(int DuplicateDispatches, int FailedRows);
}
