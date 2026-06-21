using IntelliINPI.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Application.Imports;

public sealed record ImportStatusDto(Guid? LastJobId, string Status, string? Source, DateTime? StartedAtUtc, DateTime? FinishedAtUtc);
public sealed record GetImportStatusQuery : IRequest<ImportStatusDto>;

public sealed class GetImportStatusQueryHandler(IApplicationDbContext dbContext) : IRequestHandler<GetImportStatusQuery, ImportStatusDto>
{
    public async Task<ImportStatusDto> Handle(GetImportStatusQuery request, CancellationToken cancellationToken)
    {
        var job = await dbContext.ImportJobs
            .AsNoTracking()
            .OrderByDescending(x => x.StartedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return job is null
            ? new ImportStatusDto(null, "NotStarted", null, null, null)
            : new ImportStatusDto(job.Id, job.Status, job.Source, job.StartedAtUtc, job.FinishedAtUtc);
    }
}
