using IntelliINPI.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IntelliINPI.Application.Nit;

public sealed record NitAuditLogDto(
    Guid Id,
    Guid UserId,
    string UserEmail,
    Guid? UniversityId,
    string? UniversityName,
    string Module,
    string EntityName,
    Guid EntityId,
    string Action,
    string? PreviousValue,
    string? NewValue,
    string? IpAddress,
    DateTime CreatedAtUtc);

public sealed record ListNitAuditLogsQuery(string? EntityName = null, string? Action = null, Guid? UserId = null, DateTime? StartAtUtc = null, DateTime? EndAtUtc = null) : IRequest<IReadOnlyList<NitAuditLogDto>>;

public sealed class ListNitAuditLogsQueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<ListNitAuditLogsQuery, IReadOnlyList<NitAuditLogDto>>
{
    public async Task<IReadOnlyList<NitAuditLogDto>> Handle(ListNitAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var access = await NitAccessContext.LoadAsync(dbContext, currentUser.UserId, cancellationToken);
        var query = dbContext.AuditLogs.AsNoTracking().Where(x => x.Module == "NIT");

        if (!string.IsNullOrWhiteSpace(request.EntityName)) query = query.Where(x => x.EntityName == request.EntityName);
        if (!string.IsNullOrWhiteSpace(request.Action)) query = query.Where(x => x.Action == request.Action);
        if (request.UserId.HasValue) query = query.Where(x => x.UserId == request.UserId);
        if (request.StartAtUtc.HasValue) query = query.Where(x => x.CreatedAtUtc >= request.StartAtUtc);
        if (request.EndAtUtc.HasValue) query = query.Where(x => x.CreatedAtUtc <= request.EndAtUtc);

        if (!access.IsGlobalAdmin)
        {
            query = query.Where(x => x.UniversityId != null && access.UniversityIds.Contains(x.UniversityId.Value));
        }

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(200)
            .Select(x => new NitAuditLogDto(
                x.Id,
                x.UserId,
                x.User.Email,
                x.UniversityId,
                x.University == null ? null : x.University.Name,
                x.Module,
                x.EntityName,
                x.EntityId,
                x.Action,
                x.PreviousValue,
                x.NewValue,
                x.IpAddress,
                x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
