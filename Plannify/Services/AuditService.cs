using Plannify.Data;
using Plannify.Models;

namespace Plannify.Services;

public class AuditService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string action, string entityName, string entityId, string? oldValues = null, string? newValues = null)
    {
        var username = _httpContextAccessor.HttpContext?.User.Identity?.Name ?? "System";
        var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        var auditLog = new AuditLog
        {
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            PerformedBy = username,
            PerformedAt = DateTime.UtcNow,
            IPAddress = ipAddress
        };

        _dbContext.AuditLogs.Add(auditLog);
        await _dbContext.SaveChangesAsync();
    }
}
