using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using IntelliINPI.Application.Abstractions;
using IntelliINPI.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Application.Imports;

public sealed record ImportTrademarksCommand : IRequest<ImportTrademarksResult>;
public sealed record ImportTrademarksResult(Guid JobId, string Status, int DownloadedFiles, int ImportedRows, int FailedRows, string? ErrorMessage = null)
{
    public int SkippedRows { get; init; }
    public int DuplicateRows { get; init; }
}

public sealed class ImportTrademarksCommandHandler(IInpiOpenDataTrademarkImporter openDataImporter)
    : IRequestHandler<ImportTrademarksCommand, ImportTrademarksResult>
{
    public Task<ImportTrademarksResult> Handle(ImportTrademarksCommand request, CancellationToken cancellationToken)
    {
        return openDataImporter.ImportAsync(cancellationToken);
    }
}

public sealed class InpiOpenDataTrademarkImporter(IApplicationDbContext dbContext, IInpiOpenDataClient openDataClient)
    : IInpiOpenDataTrademarkImporter
{
    private static readonly string[] BibliographicNames = ["bibliograficos", "dados"];
    private static readonly string[] OwnerNames = ["depositantes", "titulares", "owners"];
    private static readonly string[] NiceClassNames = ["nice"];
    private static readonly string[] DispatchNames = ["despachos"];

    public ImportSource Source => ImportSource.OpenDataCsv;

    public async Task<ImportTrademarksResult> ImportAsync(CancellationToken cancellationToken)
    {
        var job = new ImportJob
        {
            Id = Guid.NewGuid(),
            Source = "INPI Dados Abertos CSV - Marcas",
            SourceType = Source,
            Status = "Running",
            StartedAtUtc = DateTime.UtcNow
        };

        dbContext.ImportJobs.Add(job);
        AddLog(job.Id, "Importacao de marcas iniciada.");
        await dbContext.SaveChangesAsync(cancellationToken);

        var downloadedFiles = 0;
        var importedRows = 0;
        var failedRows = 0;

        try
        {
            var files = await openDataClient.DownloadTrademarkFilesAsync(cancellationToken);
            downloadedFiles = files.Count;
            AddLog(job.Id, $"{files.Count} arquivo(s) baixado(s).");

            var bibliographicFile = FindFile(files, BibliographicNames);
            if (bibliographicFile is not null)
            {
                var result = await ImportBibliographicFileAsync(bibliographicFile, job.Id, cancellationToken);
                importedRows += result.ImportedRows;
                failedRows += result.FailedRows;
            }
            else
            {
                AddLog(job.Id, "Arquivo de dados bibliograficos nao encontrado entre os arquivos baixados.");
            }

            foreach (var ownerFile in files.Where(x => Matches(x.Name, OwnerNames)))
            {
                var result = await ImportOwnersFileAsync(ownerFile, job.Id, cancellationToken);
                importedRows += result.ImportedRows;
                failedRows += result.FailedRows;
            }

            foreach (var niceFile in files.Where(x => Matches(x.Name, NiceClassNames)))
            {
                var result = await ImportNiceClassesFileAsync(niceFile, job.Id, cancellationToken);
                importedRows += result.ImportedRows;
                failedRows += result.FailedRows;
            }

            foreach (var dispatchFile in files.Where(x => Matches(x.Name, DispatchNames)))
            {
                var result = await ImportDispatchesFileAsync(dispatchFile, job.Id, cancellationToken);
                importedRows += result.ImportedRows;
                failedRows += result.FailedRows;
            }

            var finalStatus = importedRows == 0 ? "Failed" : failedRows == 0 ? "Completed" : "CompletedWithWarnings";
            var finalMessage = importedRows == 0
                ? $"Importacao finalizada sem linhas importadas. Verifique se os arquivos baixados contem dados alem do cabecalho. Linhas ignoradas: {failedRows}."
                : $"Importacao finalizada. Linhas importadas: {importedRows}. Linhas ignoradas: {failedRows}.";

            await MarkJobFinishedAsync(
                job.Id,
                finalStatus,
                finalMessage,
                cancellationToken);

            var errorMessage = importedRows == 0 ? "Nenhuma marca foi importada dos arquivos baixados." : null;
            return new ImportTrademarksResult(job.Id, finalStatus, downloadedFiles, importedRows, failedRows, errorMessage);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            var entityNames = DescribeConcurrencyEntries(exception);
            var errorMessage = $"Falha de concorrencia ao salvar a importacao. Entidade(s): {entityNames}.";
            await MarkJobFailedAsync(job.Id, errorMessage, cancellationToken);
            return new ImportTrademarksResult(job.Id, "Failed", downloadedFiles, importedRows, failedRows, errorMessage);
        }
        catch (Exception exception)
        {
            var errorMessage = $"Falha na importacao: {exception.Message}";
            await MarkJobFailedAsync(job.Id, errorMessage, cancellationToken);
            return new ImportTrademarksResult(job.Id, "Failed", downloadedFiles, importedRows, failedRows, errorMessage);
        }
    }

    private async Task<FileImportResult> ImportBibliographicFileAsync(InpiOpenDataFile file, Guid jobId, CancellationToken cancellationToken)
    {
        var result = new FileImportResult();
        var layout = await InspectFileAsync(file.Path, cancellationToken);
        AddFileStartLog(jobId, file, layout);

        if (!layout.IsValid)
        {
            result.InvalidFileReason = layout.InvalidReason;
            AddLog(jobId, $"{file.Name}: arquivo ignorado. Motivo: {layout.InvalidReason}");
            await dbContext.SaveChangesAsync(cancellationToken);
            return result;
        }

        await foreach (var row in ReadRowsAsync(file.Path, cancellationToken))
        {
            result.LinesRead++;
            try
            {
                var processNumber = Get(row, "processo", "numero processo", "numero do processo", "numero_processo", "numero inpi", "numero_inpi", "processnumber");
                var name = Get(row, "marca", "nome marca", "nome da marca", "nome", "denominacao", "elemento nominativo", "elemento_nominativo");

                if (string.IsNullOrWhiteSpace(processNumber))
                {
                    result.FailedRows++;
                    result.MissingProcessNumberRows++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    result.FailedRows++;
                    result.MissingRequiredFieldRows++;
                    continue;
                }

                var trademark = await FindTrademarkAsync(processNumber, cancellationToken);
                if (trademark is null)
                {
                    trademark = new Trademark
                    {
                        Id = Guid.NewGuid(),
                        ProcessNumber = processNumber,
                        CreatedAtUtc = DateTime.UtcNow
                    };
                    dbContext.Trademarks.Add(trademark);
                }

                trademark.Name = TrimTo(name, 200);
                trademark.FilingDate = ParseDate(Get(row, "data deposito", "data_deposito", "deposito", "filing date"));
                trademark.RegistrationDate = ParseDate(Get(row, "data concessao", "data registro", "data_registro", "registration date"));

                var statusCode = Get(row, "codigo situacao", "codigo_situacao", "status codigo", "status");
                var statusDescription = Get(row, "situacao", "descricao situacao", "descricao_situacao", "status descricao", "status");
                trademark.StatusId = await ResolveStatusIdAsync(statusCode, statusDescription, cancellationToken);

                result.ImportedRows++;
                await SaveBatchAsync(result.ImportedRows, cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            catch (Exception exception)
            {
                result.FailedRows++;
                AddLog(jobId, $"Linha bibliografica ignorada em {file.Name}: {exception.Message}");
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        AddFileEndLog(jobId, file, result);
        await dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    private async Task<FileImportResult> ImportOwnersFileAsync(InpiOpenDataFile file, Guid jobId, CancellationToken cancellationToken)
    {
        var result = new FileImportResult();
        var layout = await InspectFileAsync(file.Path, cancellationToken);
        AddFileStartLog(jobId, file, layout);

        if (!layout.IsValid)
        {
            result.InvalidFileReason = layout.InvalidReason;
            AddLog(jobId, $"{file.Name}: arquivo ignorado. Motivo: {layout.InvalidReason}");
            await dbContext.SaveChangesAsync(cancellationToken);
            return result;
        }

        await foreach (var row in ReadRowsAsync(file.Path, cancellationToken))
        {
            result.LinesRead++;
            try
            {
                var processNumber = Get(row, "processo", "numero processo", "numero do processo", "numero_processo", "numero inpi", "numero_inpi");
                var ownerName = Get(row, "depositante", "titular", "nome", "nome titular", "nome depositante", "owner");
                var document = Get(row, "cpf cnpj", "documento", "cnpj", "cpf", "cnpj cpf titular", "cnpj_cpf_titular");

                if (string.IsNullOrWhiteSpace(processNumber))
                {
                    result.FailedRows++;
                    result.MissingProcessNumberRows++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(ownerName))
                {
                    result.FailedRows++;
                    result.MissingRequiredFieldRows++;
                    continue;
                }

                var trademark = await FindTrademarkAsync(processNumber, cancellationToken);
                if (trademark is null)
                {
                    result.FailedRows++;
                    result.MissingTrademarkRows++;
                    continue;
                }

                var owner = await FindOrCreateOwnerAsync(ownerName, document, cancellationToken);
                trademark.OwnerId = owner.Id;
                if (!await dbContext.TrademarkOwnerLinks.AnyAsync(
                    x => x.TrademarkId == trademark.Id && x.OwnerId == owner.Id,
                    cancellationToken))
                {
                    dbContext.TrademarkOwnerLinks.Add(new TrademarkOwnerLink
                    {
                        Id = Guid.NewGuid(),
                        TrademarkId = trademark.Id,
                        OwnerId = owner.Id
                    });
                }

                result.ImportedRows++;
                await SaveBatchAsync(result.ImportedRows, cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            catch (Exception exception)
            {
                result.FailedRows++;
                AddLog(jobId, $"Linha de titular ignorada em {file.Name}: {exception.Message}");
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        AddFileEndLog(jobId, file, result);
        await dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    private async Task<FileImportResult> ImportNiceClassesFileAsync(InpiOpenDataFile file, Guid jobId, CancellationToken cancellationToken)
    {
        var result = new FileImportResult();
        var layout = await InspectFileAsync(file.Path, cancellationToken);
        AddFileStartLog(jobId, file, layout);

        if (!layout.IsValid)
        {
            result.InvalidFileReason = layout.InvalidReason;
            AddLog(jobId, $"{file.Name}: arquivo ignorado. Motivo: {layout.InvalidReason}");
            await dbContext.SaveChangesAsync(cancellationToken);
            return result;
        }

        await foreach (var row in ReadRowsAsync(file.Path, cancellationToken))
        {
            result.LinesRead++;
            try
            {
                var processNumber = Get(row, "processo", "numero processo", "numero do processo", "numero_processo", "numero inpi", "numero_inpi");
                var code = Get(row, "classe nice", "classe_nice", "classe", "codigo classe", "codigo_classe", "nice");
                var specification = Get(row, "especificacao", "descricao", "produtos servicos", "produtos e servicos");

                if (string.IsNullOrWhiteSpace(processNumber))
                {
                    result.FailedRows++;
                    result.MissingProcessNumberRows++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(code))
                {
                    result.FailedRows++;
                    result.MissingRequiredFieldRows++;
                    continue;
                }

                var trademark = await FindTrademarkWithNiceClassesAsync(processNumber, cancellationToken);
                if (trademark is null)
                {
                    result.FailedRows++;
                    result.MissingTrademarkRows++;
                    continue;
                }

                var normalizedCode = NormalizeNiceClassCode(code);
                if (!trademark.NiceClasses.Any(x => x.Code == normalizedCode))
                {
                    trademark.NiceClasses.Add(new TrademarkNiceClass
                    {
                        Id = Guid.NewGuid(),
                        TrademarkId = trademark.Id,
                        Code = normalizedCode,
                        ClassNumber = int.TryParse(normalizedCode, out var classNumber) ? classNumber : 0,
                        Specification = string.IsNullOrWhiteSpace(specification) ? null : TrimTo(specification, 1000)
                    });
                }

                result.ImportedRows++;
                await SaveBatchAsync(result.ImportedRows, cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            catch (Exception exception)
            {
                result.FailedRows++;
                AddLog(jobId, $"Linha de classe Nice ignorada em {file.Name}: {exception.Message}");
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        AddFileEndLog(jobId, file, result);
        await dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    private async Task<FileImportResult> ImportDispatchesFileAsync(InpiOpenDataFile file, Guid jobId, CancellationToken cancellationToken)
    {
        var result = new FileImportResult();
        var layout = await InspectFileAsync(file.Path, cancellationToken);
        AddFileStartLog(jobId, file, layout);

        if (!layout.IsValid)
        {
            result.InvalidFileReason = layout.InvalidReason;
            AddLog(jobId, $"{file.Name}: arquivo ignorado. Motivo: {layout.InvalidReason}");
            await dbContext.SaveChangesAsync(cancellationToken);
            return result;
        }

        await foreach (var row in ReadRowsAsync(file.Path, cancellationToken))
        {
            result.LinesRead++;
            try
            {
                var processNumber = Get(row, "processo", "numero processo", "numero do processo", "numero_processo", "numero inpi", "numero_inpi");
                var code = Get(row, "codigo despacho", "codigo_despacho", "despacho", "codigo");
                var description = Get(row, "despacho", "descricao", "descricao despacho", "descricao_despacho", "texto", "complemento despacho", "complemento_despacho");
                var publishedAt = ParseDate(Get(row, "data rpi", "data publicacao", "data_publicacao", "published at"))
                    ?? DateOnly.FromDateTime(DateTime.UtcNow);

                if (string.IsNullOrWhiteSpace(processNumber))
                {
                    result.FailedRows++;
                    result.MissingProcessNumberRows++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(code))
                {
                    result.FailedRows++;
                    result.MissingRequiredFieldRows++;
                    continue;
                }

                var trademark = await FindTrademarkWithDispatchesAsync(processNumber, cancellationToken);
                if (trademark is null)
                {
                    result.FailedRows++;
                    result.MissingTrademarkRows++;
                    continue;
                }

                if (!trademark.Dispatches.Any(x => x.Code == code && x.PublishedAt == publishedAt))
                {
                    trademark.Dispatches.Add(new TrademarkDispatch
                    {
                        Id = Guid.NewGuid(),
                        TrademarkId = trademark.Id,
                        Code = TrimTo(code, 40),
                        Description = string.IsNullOrWhiteSpace(description) ? TrimTo(code, 1000) : TrimTo(description, 1000),
                        RpiNumber = null,
                        PublishedAt = publishedAt
                    });
                }

                result.ImportedRows++;
                await SaveBatchAsync(result.ImportedRows, cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            catch (Exception exception)
            {
                result.FailedRows++;
                AddLog(jobId, $"Linha de despacho ignorada em {file.Name}: {exception.Message}");
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        AddFileEndLog(jobId, file, result);
        await dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    private async Task<Guid?> ResolveStatusIdAsync(string? code, string? description, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        var normalizedCode = TrimTo(string.IsNullOrWhiteSpace(code) ? description! : code, 40);
        var normalizedDescription = TrimTo(string.IsNullOrWhiteSpace(description) ? normalizedCode : description!, 200);
        var status = dbContext.TrademarkStatuses.Local.FirstOrDefault(x => x.Code == normalizedCode)
            ?? await dbContext.TrademarkStatuses.SingleOrDefaultAsync(x => x.Code == normalizedCode, cancellationToken);

        if (status is null)
        {
            status = new TrademarkStatus
            {
                Id = Guid.NewGuid(),
                Code = normalizedCode,
                Description = normalizedDescription
            };
            dbContext.TrademarkStatuses.Add(status);
        }
        else
        {
            status.Description = normalizedDescription;
        }

        return status.Id;
    }

    private async Task<TrademarkOwner> FindOrCreateOwnerAsync(string name, string? document, CancellationToken cancellationToken)
    {
        var normalizedName = TrimTo(name, 200);
        var normalizedDocument = string.IsNullOrWhiteSpace(document) ? null : TrimTo(document, 40);
        var owner = dbContext.TrademarkOwners.Local.FirstOrDefault(x => x.Name == normalizedName && x.Document == normalizedDocument)
            ?? await dbContext.TrademarkOwners.SingleOrDefaultAsync(
                x => x.Name == normalizedName && x.Document == normalizedDocument,
                cancellationToken);

        if (owner is not null)
        {
            return owner;
        }

        owner = new TrademarkOwner
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            Document = normalizedDocument
        };

        dbContext.TrademarkOwners.Add(owner);
        return owner;
    }

    private async Task<Trademark?> FindTrademarkAsync(string processNumber, CancellationToken cancellationToken)
    {
        return dbContext.Trademarks.Local.FirstOrDefault(x => x.ProcessNumber == processNumber)
            ?? await dbContext.Trademarks.SingleOrDefaultAsync(x => x.ProcessNumber == processNumber, cancellationToken);
    }

    private async Task<Trademark?> FindTrademarkWithNiceClassesAsync(string processNumber, CancellationToken cancellationToken)
    {
        return await dbContext.Trademarks
            .Include(x => x.NiceClasses)
            .SingleOrDefaultAsync(x => x.ProcessNumber == processNumber, cancellationToken)
            ?? dbContext.Trademarks.Local.FirstOrDefault(x => x.ProcessNumber == processNumber);
    }

    private async Task<Trademark?> FindTrademarkWithDispatchesAsync(string processNumber, CancellationToken cancellationToken)
    {
        return await dbContext.Trademarks
            .Include(x => x.Dispatches)
            .SingleOrDefaultAsync(x => x.ProcessNumber == processNumber, cancellationToken)
            ?? dbContext.Trademarks.Local.FirstOrDefault(x => x.ProcessNumber == processNumber);
    }

    private async Task MarkJobFinishedAsync(Guid jobId, string status, string message, CancellationToken cancellationToken)
    {
        var job = await LoadTrackedJobAsync(jobId, cancellationToken);
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
        DetachTrackedEntries();

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

    private async Task<ImportJob?> LoadTrackedJobAsync(Guid jobId, CancellationToken cancellationToken)
    {
        if (dbContext is DbContext efContext)
        {
            var localJob = dbContext.ImportJobs.Local.FirstOrDefault(x => x.Id == jobId);
            if (localJob is not null)
            {
                await efContext.Entry(localJob).ReloadAsync(cancellationToken);
                return localJob;
            }
        }

        return await dbContext.ImportJobs.SingleOrDefaultAsync(x => x.Id == jobId, cancellationToken);
    }

    private void DetachTrackedEntries()
    {
        if (dbContext is not DbContext efContext)
        {
            return;
        }

        foreach (var entry in efContext.ChangeTracker.Entries().ToList())
        {
            entry.State = EntityState.Detached;
        }
    }

    private static string DescribeConcurrencyEntries(DbUpdateConcurrencyException exception)
    {
        var entityNames = exception.Entries
            .Select(entry => entry.Entity.GetType().Name)
            .Distinct()
            .ToArray();

        return entityNames.Length == 0 ? "desconhecida" : string.Join(", ", entityNames);
    }

    private static async IAsyncEnumerable<IReadOnlyDictionary<string, string>> ReadRowsAsync(
        string path,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(path, DetectEncoding(path), detectEncodingFromByteOrderMarks: true);
        var headerLine = await reader.ReadLineAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            yield break;
        }

        var separator = DetectSeparator(headerLine);
        var headers = SplitLine(headerLine, separator).Select(NormalizeHeader).ToArray();
        var lineNumber = 1;

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);
            lineNumber++;

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var values = SplitLine(line, separator);
            var row = new Dictionary<string, string>();
            for (var i = 0; i < headers.Length && i < values.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(headers[i]))
                {
                    row[headers[i]] = values[i].Trim();
                }
            }

            row["__line"] = lineNumber.ToString(CultureInfo.InvariantCulture);
            yield return row;
        }
    }

    private static char DetectSeparator(string headerLine)
    {
        var separators = new[] { ';', ',', '\t', '|' };
        return separators
            .Select(separator => new { Separator = separator, Count = SplitLine(headerLine, separator).Count })
            .OrderByDescending(x => x.Count)
            .First().Separator;
    }

    private static async Task<FileLayoutInfo> InspectFileAsync(string path, CancellationToken cancellationToken)
    {
        var fileInfo = new FileInfo(path);
        if (!fileInfo.Exists)
        {
            return new FileLayoutInfo(false, "Arquivo nao encontrado.", null, null, 0, []);
        }

        if (!IsSupportedTextFile(fileInfo.Extension))
        {
            return new FileLayoutInfo(false, $"Extensao nao suportada: {fileInfo.Extension}.", null, null, 0, []);
        }

        if (fileInfo.Length == 0)
        {
            return new FileLayoutInfo(false, "Arquivo vazio.", null, null, 0, []);
        }

        var encoding = DetectEncoding(path);
        using var reader = new StreamReader(path, encoding, detectEncodingFromByteOrderMarks: true);
        var headerLine = await reader.ReadLineAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(headerLine))
        {
            return new FileLayoutInfo(false, "Cabecalho ausente.", encoding.WebName, null, 0, []);
        }

        var separator = DetectSeparator(headerLine);
        var headers = SplitLine(headerLine, separator).Select(NormalizeHeader).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        if (headers.Length <= 1)
        {
            return new FileLayoutInfo(false, "Cabecalho nao parece CSV/TXT delimitado.", encoding.WebName, separator, 0, headers);
        }

        var dataRows = 0;
        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(line))
            {
                dataRows++;
            }
        }

        return new FileLayoutInfo(true, null, encoding.WebName, separator, dataRows, headers);
    }

    private static Encoding DetectEncoding(string path)
    {
        var bom = new byte[4];
        using (var stream = File.OpenRead(path))
        {
            _ = stream.Read(bom, 0, Math.Min(4, (int)stream.Length));
        }

        if (bom is [0xEF, 0xBB, 0xBF, _])
        {
            return Encoding.UTF8;
        }

        if (bom is [0xFF, 0xFE, _, _])
        {
            return Encoding.Unicode;
        }

        if (bom is [0xFE, 0xFF, _, _])
        {
            return Encoding.BigEndianUnicode;
        }

        return Encoding.Latin1;
    }

    private static bool IsSupportedTextFile(string extension)
    {
        return string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase)
            || string.Equals(extension, ".txt", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(extension);
    }

    private static List<string> SplitLine(string line, char separator)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        foreach (var character in line)
        {
            if (character == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (character == separator && !inQuotes)
            {
                values.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(character);
        }

        values.Add(current.ToString());
        return values;
    }

    private static string? Get(IReadOnlyDictionary<string, string> row, params string[] aliases)
    {
        foreach (var alias in aliases.Select(NormalizeHeader))
        {
            if (row.TryGetValue(alias, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }

    private static string NormalizeHeader(string value)
    {
        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.IsLetterOrDigit(character) ? character : ' ');
            }
        }

        return Regex.Replace(builder.ToString(), "\\s+", " ").Trim();
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
        return string.IsNullOrWhiteSpace(digits) ? TrimTo(value, 20) : digits.PadLeft(2, '0');
    }

    private static InpiOpenDataFile? FindFile(IReadOnlyList<InpiOpenDataFile> files, string[] terms)
    {
        return files.FirstOrDefault(x => Matches(x.Name, terms));
    }

    private static bool Matches(string name, string[] terms)
    {
        var normalizedName = name.ToLowerInvariant();
        return terms.All(normalizedName.Contains);
    }

    private async Task SaveBatchAsync(int importedRows, CancellationToken cancellationToken)
    {
        if (importedRows % 500 == 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static string TrimTo(string value, int maxLength)
    {
        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
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

    private void AddFileStartLog(Guid jobId, InpiOpenDataFile file, FileLayoutInfo layout)
    {
        var separator = layout.Separator is null ? "n/a" : layout.Separator == '\t' ? "tab" : layout.Separator.ToString();
        var headers = layout.Headers.Count == 0 ? "nenhum" : string.Join(", ", layout.Headers);
        var status = layout.IsValid ? "valido" : $"invalido ({layout.InvalidReason})";
        AddLog(jobId, $"{file.Name}: arquivo {status}; encoding={layout.Encoding ?? "n/a"}; separador={separator}; linhas lidas={layout.DataRows}; cabecalhos detectados=[{headers}].");
    }

    private void AddFileEndLog(Guid jobId, InpiOpenDataFile file, FileImportResult result)
    {
        var reasons = new List<string>();
        if (result.MissingProcessNumberRows > 0)
        {
            reasons.Add($"sem numero do processo: {result.MissingProcessNumberRows}");
        }

        if (result.MissingRequiredFieldRows > 0)
        {
            reasons.Add($"campo obrigatorio ausente: {result.MissingRequiredFieldRows}");
        }

        if (result.MissingTrademarkRows > 0)
        {
            reasons.Add($"marca nao encontrada para complementar: {result.MissingTrademarkRows}");
        }

        if (!string.IsNullOrWhiteSpace(result.InvalidFileReason))
        {
            reasons.Add($"arquivo invalido: {result.InvalidFileReason}");
        }

        var ignoredReasons = reasons.Count == 0 ? "nenhum" : string.Join("; ", reasons);
        AddLog(jobId, $"{file.Name}: linhas lidas={result.LinesRead}; importadas={result.ImportedRows}; ignoradas={result.FailedRows}; motivos=[{ignoredReasons}].");
    }

    private sealed class FileImportResult
    {
        public int LinesRead { get; set; }
        public int ImportedRows { get; set; }
        public int FailedRows { get; set; }
        public int MissingProcessNumberRows { get; set; }
        public int MissingRequiredFieldRows { get; set; }
        public int MissingTrademarkRows { get; set; }
        public string? InvalidFileReason { get; set; }
    }

    private sealed record FileLayoutInfo(
        bool IsValid,
        string? InvalidReason,
        string? Encoding,
        char? Separator,
        int DataRows,
        IReadOnlyList<string> Headers);
}
