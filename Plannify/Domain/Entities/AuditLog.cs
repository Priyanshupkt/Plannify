using System.ComponentModel.DataAnnotations;

namespace Plannify.Domain.Entities;

/// <summary>
/// Domain entity for audit logging - tracks all changes to critical entities
/// </summary>
public class AuditLog
{
    public int Id { get; set; }

    [Required]
    public string Action { get; set; } = string.Empty;

    [Required]
    public string EntityName { get; set; } = string.Empty;

    public string EntityId { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
    public string? IPAddress { get; set; }
}
