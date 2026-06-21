using IntelliINPI.Application.Abstractions;
using IntelliINPI.Domain.Entities;

namespace IntelliINPI.Application.Operational;

internal static class AuditLogHelper
{
    public static AuditLog Create(ICurrentUser currentUser, string module, string entityName, Guid entityId, string action, Guid? universityId = null)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = currentUser.UserId,
            UniversityId = universityId,
            Module = module,
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            IpAddress = currentUser.IpAddress,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
