namespace HealthcareCRM.Services;

public interface IAuditService
{
    Task LogAsync(string userId, string userName, string action,
                  string entityName, string? entityId = null,
                  string? oldValues = null, string? newValues = null,
                  string? ipAddress = null);
}
