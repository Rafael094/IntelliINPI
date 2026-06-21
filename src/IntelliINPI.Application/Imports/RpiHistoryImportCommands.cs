using FluentValidation;
using IntelliINPI.Application.Abstractions;
using IntelliINPI.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Application.Imports;

public sealed record RunRpiHistoryImportRequest(
    int? StartYear,
    int? StartRpi,
    int EndRpi,
    int BatchSize,
    int DelaySecondsBetweenBatches);

public sealed record RunRpiHistoryImportCommand(
    int? StartYear,
    int? StartRpi,
    int EndRpi,
    int BatchSize,
    int DelaySecondsBetweenBatches) : IRequest<RpiHistoryImportStatusDto>;

public sealed record ResumeRpiHistoryImportCommand : IRequest<RpiHistoryImportStatusDto>;
public sealed record StopRpiHistoryImportCommand : IRequest<RpiHistoryImportStatusDto>;
public sealed record GetRpiHistoryImportStatusQuery : IRequest<RpiHistoryImportStatusDto?>;
public sealed record GetRpiHistoryImportErrorsQuery(Guid? RunId) : IRequest<IReadOnlyList<RpiHistoryImportErrorDto>>;

public sealed record RpiHistoryImportStatusDto(
    Guid RunId,
    string Status,
    int StartRpi,
    int EndRpi,
    int CurrentRpi,
    int TotalRpis,
    int SuccessfulRpis,
    int FailedRpis,
    int SkippedRpis,
    int TotalDispatchesImported,
    int DuplicateDispatches,
    string? LastErrorSummary,
    decimal Percentage,
    DateTime StartedAtUtc,
    DateTime? FinishedAtUtc,
    string? ErrorMessage);

public sealed record RpiHistoryImportErrorDto(
    int RpiNumber,
    string Status,
    string? ErrorMessage,
    string? ZipPath,
    int DispatchesImported,
    int FailedRows);

public sealed class RunRpiHistoryImportCommandValidator : AbstractValidator<RunRpiHistoryImportCommand>
{
    public RunRpiHistoryImportCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => x.StartRpi.HasValue || x.StartYear.HasValue)
            .WithMessage("Informe startRpi ou startYear.");

        RuleFor(x => x.StartYear)
            .InclusiveBetween(2010, 2100)
            .When(x => x.StartYear.HasValue);

        RuleFor(x => x.StartRpi)
            .GreaterThan(0)
            .When(x => x.StartRpi.HasValue);

        RuleFor(x => x.EndRpi)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.BatchSize)
            .InclusiveBetween(1, 200);

        RuleFor(x => x.DelaySecondsBetweenBatches)
            .InclusiveBetween(0, 3600);
    }
}

public sealed class RpiHistoryImportHandler(
    IApplicationDbContext dbContext,
    IInpiRpiTrademarkImporter importer,
    IInpiRpiClient rpiClient,
    IInpiRpiHistorySettings historySettings)
    : IRequestHandler<RunRpiHistoryImportCommand, RpiHistoryImportStatusDto>,
        IRequestHandler<ResumeRpiHistoryImportCommand, RpiHistoryImportStatusDto>,
        IRequestHandler<StopRpiHistoryImportCommand, RpiHistoryImportStatusDto>,
        IRequestHandler<GetRpiHistoryImportStatusQuery, RpiHistoryImportStatusDto?>,
        IRequestHandler<GetRpiHistoryImportErrorsQuery, IReadOnlyList<RpiHistoryImportErrorDto>>
{
    private const string Running = "Running";
    private const string Completed = "Completed";
    private const string CompletedWithWarnings = "CompletedWithWarnings";
    private const string Failed = "Failed";
    private const string StopRequested = "StopRequested";
    private const string Stopped = "Stopped";

    public async Task<RpiHistoryImportStatusDto> Handle(RunRpiHistoryImportCommand request, CancellationToken cancellationToken)
    {
        var startRpi = await ResolveStartRpiAsync(request.StartRpi, request.StartYear, cancellationToken);
        var endRpi = request.EndRpi == 0
            ? await rpiClient.GetLatestTrademarkRpiNumberAsync(cancellationToken)
            : request.EndRpi;

        if (endRpi < startRpi)
        {
            throw new InvalidOperationException("endRpi deve ser maior ou igual ao RPI inicial.");
        }

        var existingRunning = await dbContext.RpiHistoricalImportRuns
            .AnyAsync(x => x.Status == Running || x.Status == StopRequested, cancellationToken);

        if (existingRunning)
        {
            throw new InvalidOperationException("Ja existe uma importacao historica em execucao.");
        }

        var run = new RpiHistoricalImportRun
        {
            Id = Guid.NewGuid(),
            StartRpi = startRpi,
            EndRpi = endRpi,
            CurrentRpi = startRpi,
            BatchSize = request.BatchSize,
            Status = Running,
            StartedAtUtc = DateTime.UtcNow,
            TotalRpis = endRpi - startRpi + 1
        };

        dbContext.RpiHistoricalImportRuns.Add(run);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await ExecuteRunAsync(run.Id, request.DelaySecondsBetweenBatches, cancellationToken);
    }

    public async Task<RpiHistoryImportStatusDto> Handle(ResumeRpiHistoryImportCommand request, CancellationToken cancellationToken)
    {
        var run = await dbContext.RpiHistoricalImportRuns
            .OrderByDescending(x => x.StartedAtUtc)
            .FirstOrDefaultAsync(x => x.Status != Completed && x.Status != CompletedWithWarnings, cancellationToken);

        if (run is null)
        {
            throw new InvalidOperationException("Nenhuma importacao historica pendente foi encontrada para retomar.");
        }

        run.Status = Running;
        run.FinishedAtUtc = null;
        run.ErrorMessage = null;
        run.CurrentRpi = await ResolveResumeRpiAsync(run, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await ExecuteRunAsync(run.Id, 0, cancellationToken);
    }

    public async Task<RpiHistoryImportStatusDto> Handle(StopRpiHistoryImportCommand request, CancellationToken cancellationToken)
    {
        var run = await dbContext.RpiHistoricalImportRuns
            .OrderByDescending(x => x.StartedAtUtc)
            .FirstOrDefaultAsync(x => x.Status == Running, cancellationToken);

        if (run is null)
        {
            var latest = await GetLatestRunAsync(cancellationToken);
            if (latest is null)
            {
                throw new InvalidOperationException("Nenhuma importacao historica foi encontrada.");
            }

            return ToStatusDto(latest);
        }

        run.Status = StopRequested;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToStatusDto(run);
    }

    public async Task<RpiHistoryImportStatusDto?> Handle(GetRpiHistoryImportStatusQuery request, CancellationToken cancellationToken)
    {
        var run = await GetLatestRunAsync(cancellationToken);
        return run is null ? null : ToStatusDto(run);
    }

    public async Task<IReadOnlyList<RpiHistoryImportErrorDto>> Handle(GetRpiHistoryImportErrorsQuery request, CancellationToken cancellationToken)
    {
        var run = request.RunId.HasValue
            ? await dbContext.RpiHistoricalImportRuns.AsNoTracking().SingleOrDefaultAsync(x => x.Id == request.RunId.Value, cancellationToken)
            : await GetLatestRunAsync(cancellationToken);

        if (run is null)
        {
            return [];
        }

        return await dbContext.RpiImportCheckpoints
            .AsNoTracking()
            .Where(x => x.RpiNumber >= run.StartRpi && x.RpiNumber <= run.EndRpi && x.Status != Completed)
            .OrderBy(x => x.RpiNumber)
            .Select(x => new RpiHistoryImportErrorDto(
                x.RpiNumber,
                x.Status,
                x.ErrorMessage,
                x.ZipPath,
                x.DispatchesImported,
                x.FailedRows))
            .ToListAsync(cancellationToken);
    }

    private async Task<RpiHistoryImportStatusDto> ExecuteRunAsync(Guid runId, int delaySecondsBetweenBatches, CancellationToken cancellationToken)
    {
        var processedInCurrentBatch = 0;

        while (true)
        {
            var run = await dbContext.RpiHistoricalImportRuns.SingleAsync(x => x.Id == runId, cancellationToken);

            if (run.Status == StopRequested)
            {
                run.Status = Stopped;
                run.FinishedAtUtc = DateTime.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);
                return ToStatusDto(run);
            }

            if (run.CurrentRpi > run.EndRpi)
            {
                run.Status = ResolveFinalStatus(run);
                run.FinishedAtUtc = DateTime.UtcNow;
                run.LastErrorSummary = await BuildLastErrorSummaryAsync(run, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
                return ToStatusDto(run);
            }

            var rpiNumber = run.CurrentRpi;
            var completedCheckpoint = await dbContext.RpiImportCheckpoints
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.RpiNumber == rpiNumber && (x.Status == Completed || x.Status == CompletedWithWarnings),
                    cancellationToken);

            if (completedCheckpoint is not null)
            {
                run.SkippedRpis++;
                run.DuplicateDispatches += completedCheckpoint.DuplicateDispatches;
                run.CurrentRpi++;
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            else
            {
                await ImportSingleRpiAsync(run, rpiNumber, cancellationToken);
            }

            processedInCurrentBatch++;
            if (processedInCurrentBatch >= run.BatchSize && run.CurrentRpi <= run.EndRpi)
            {
                processedInCurrentBatch = 0;
                if (delaySecondsBetweenBatches > 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(delaySecondsBetweenBatches), cancellationToken);
                }
            }
        }
    }

    private async Task ImportSingleRpiAsync(RpiHistoricalImportRun run, int rpiNumber, CancellationToken cancellationToken)
    {
        var checkpoint = await dbContext.RpiImportCheckpoints.SingleOrDefaultAsync(x => x.RpiNumber == rpiNumber, cancellationToken);
        if (checkpoint is null)
        {
            checkpoint = new RpiImportCheckpoint
            {
                Id = Guid.NewGuid(),
                RpiNumber = rpiNumber
            };
            dbContext.RpiImportCheckpoints.Add(checkpoint);
        }

        checkpoint.Status = Running;
        checkpoint.StartedAtUtc = DateTime.UtcNow;
        checkpoint.FinishedAtUtc = null;
        checkpoint.DispatchesImported = 0;
        checkpoint.FailedRows = 0;
        checkpoint.DuplicateDispatches = 0;
        checkpoint.ErrorMessage = null;
        checkpoint.ZipPath = null;
        checkpoint.XmlOrTxtFilesCount = 0;

        run.CurrentRpi = rpiNumber;
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            var result = await importer.ImportAsync(rpiNumber, cancellationToken);
            var (zipPath, xmlOrTxtCount) = InspectLocalRpiFiles(rpiNumber);
            run = await dbContext.RpiHistoricalImportRuns.SingleAsync(x => x.Id == run.Id, cancellationToken);
            checkpoint = await dbContext.RpiImportCheckpoints.SingleAsync(x => x.RpiNumber == rpiNumber, cancellationToken);

            checkpoint.FinishedAtUtc = DateTime.UtcNow;
            checkpoint.DispatchesImported = result.ImportedRows;
            checkpoint.FailedRows = result.FailedRows;
            checkpoint.DuplicateDispatches = result.DuplicateRows > 0 ? result.DuplicateRows : result.FailedRows;
            checkpoint.ZipPath = zipPath;
            checkpoint.XmlOrTxtFilesCount = xmlOrTxtCount;

            if (string.Equals(result.Status, Failed, StringComparison.OrdinalIgnoreCase))
            {
                checkpoint.Status = Failed;
                checkpoint.ErrorMessage = result.ErrorMessage;
                run.FailedRpis++;
                run.LastErrorSummary = result.ErrorMessage;
            }
            else
            {
                checkpoint.Status = result.Status;
                run.SuccessfulRpis++;
                run.TotalDispatchesImported += result.ImportedRows;
                run.DuplicateDispatches += checkpoint.DuplicateDispatches;
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            run = await dbContext.RpiHistoricalImportRuns.SingleAsync(x => x.Id == run.Id, cancellationToken);
            checkpoint = await dbContext.RpiImportCheckpoints.SingleAsync(x => x.RpiNumber == rpiNumber, cancellationToken);
            checkpoint.Status = Failed;
            checkpoint.FinishedAtUtc = DateTime.UtcNow;
            checkpoint.ErrorMessage = exception.Message;
            run.FailedRpis++;
            run.LastErrorSummary = exception.Message;
        }
        finally
        {
            run.CurrentRpi = rpiNumber + 1;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private Task<int> ResolveStartRpiAsync(int? startRpi, int? startYear, CancellationToken cancellationToken)
    {
        if (startRpi.HasValue)
        {
            return Task.FromResult(startRpi.Value);
        }

        var resolved = historySettings.GetStartRpiForYear(startYear!.Value);
        if (resolved.HasValue)
        {
            return Task.FromResult(resolved.Value);
        }

        throw new InvalidOperationException($"Nao ha mapeamento configurado para o ano {startYear}.");
    }

    private async Task<int> ResolveResumeRpiAsync(RpiHistoricalImportRun run, CancellationToken cancellationToken)
    {
        var lastCompleted = await dbContext.RpiImportCheckpoints
            .AsNoTracking()
            .Where(x => x.RpiNumber >= run.StartRpi
                && x.RpiNumber <= run.EndRpi
                && (x.Status == Completed || x.Status == CompletedWithWarnings))
            .OrderByDescending(x => x.RpiNumber)
            .Select(x => (int?)x.RpiNumber)
            .FirstOrDefaultAsync(cancellationToken);

        return Math.Max(run.CurrentRpi, (lastCompleted ?? run.StartRpi - 1) + 1);
    }

    private (string? ZipPath, int XmlOrTxtFilesCount) InspectLocalRpiFiles(int rpiNumber)
    {
        var directory = Path.Combine(historySettings.RawDirectory, rpiNumber.ToString());
        if (!Directory.Exists(directory))
        {
            return (null, 0);
        }

        var zipPath = Directory.GetFiles(directory, "*.zip", SearchOption.TopDirectoryOnly).FirstOrDefault();
        var xmlOrTxtCount = Directory
            .GetFiles(directory, "*.*", SearchOption.TopDirectoryOnly)
            .Count(x =>
                string.Equals(Path.GetExtension(x), ".xml", StringComparison.OrdinalIgnoreCase)
                || string.Equals(Path.GetExtension(x), ".txt", StringComparison.OrdinalIgnoreCase));

        return (zipPath, xmlOrTxtCount);
    }

    private Task<RpiHistoricalImportRun?> GetLatestRunAsync(CancellationToken cancellationToken)
    {
        return dbContext.RpiHistoricalImportRuns
            .AsNoTracking()
            .OrderByDescending(x => x.StartedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static string ResolveFinalStatus(RpiHistoricalImportRun run)
    {
        if (run.FailedRpis > run.SuccessfulRpis)
        {
            return Failed;
        }

        if (run.FailedRpis > 0 || run.SkippedRpis > 0 || run.DuplicateDispatches > 0)
        {
            return CompletedWithWarnings;
        }

        return Completed;
    }

    private async Task<string?> BuildLastErrorSummaryAsync(RpiHistoricalImportRun run, CancellationToken cancellationToken)
    {
        var error = await dbContext.RpiImportCheckpoints
            .AsNoTracking()
            .Where(x => x.RpiNumber >= run.StartRpi && x.RpiNumber <= run.EndRpi && x.Status == Failed && x.ErrorMessage != null)
            .GroupBy(x => x.ErrorMessage!)
            .Select(x => new
            {
                ErrorMessage = x.Key,
                Count = x.Count(),
                FirstRpi = x.Min(c => c.RpiNumber),
                LastRpi = x.Max(c => c.RpiNumber)
            })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.FirstRpi)
            .FirstOrDefaultAsync(cancellationToken);

        return error is null
            ? null
            : $"{error.Count} RPI(s), {error.FirstRpi}-{error.LastRpi}: {error.ErrorMessage}";
    }

    private static RpiHistoryImportStatusDto ToStatusDto(RpiHistoricalImportRun run)
    {
        var processed = Math.Clamp(run.SuccessfulRpis + run.FailedRpis + run.SkippedRpis, 0, run.TotalRpis);
        var percentage = run.TotalRpis == 0
            ? 0
            : Math.Round(processed * 100m / run.TotalRpis, 2);

        return new RpiHistoryImportStatusDto(
            run.Id,
            run.Status,
            run.StartRpi,
            run.EndRpi,
            run.CurrentRpi,
            run.TotalRpis,
            run.SuccessfulRpis,
            run.FailedRpis,
            run.SkippedRpis,
            run.TotalDispatchesImported,
            run.DuplicateDispatches,
            run.LastErrorSummary,
            percentage,
            run.StartedAtUtc,
            run.FinishedAtUtc,
            run.ErrorMessage);
    }
}
