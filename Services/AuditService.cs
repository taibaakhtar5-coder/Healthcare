using HealthcareCRM.Data;
using HealthcareCRM.Models;

namespace HealthcareCRM.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _db;

    public AuditService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(string userId, string userName, string action,
                               string entityName, string? entityId = null,
                               string? oldValues = null, string? newValues = null,
                               string? ipAddress = null)
    {
        var log = new AuditLog
        {
            UserId     = userId,
            UserName   = userName,
            Action     = action,
            EntityName = entityName,
            EntityId   = entityId,
            OldValues  = oldValues,
            NewValues  = newValues,
            IPAddress  = ipAddress,
            Timestamp  = DateTime.UtcNow
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }
}
