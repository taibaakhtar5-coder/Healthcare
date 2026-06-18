using System.ComponentModel.DataAnnotations;

namespace HealthcareCRM.Models;

public class AuditLog
{
    public int      Id         { get; set; }
    public string   UserId     { get; set; } = string.Empty;
    public string   UserName   { get; set; } = string.Empty;
    public string   Action     { get; set; } = string.Empty;
    public string   EntityName { get; set; } = string.Empty;
    public string?  EntityId   { get; set; }
    public string?  OldValues  { get; set; }
    public string?  NewValues  { get; set; }
    public string?  IPAddress  { get; set; }
    public DateTime Timestamp  { get; set; } = DateTime.UtcNow;

    // Navigation
    public ApplicationUser? User { get; set; }
}
