namespace Plannify.Services;

/// <summary>
/// Result object returned from conflict detection checks
/// </summary>
public class ConflictResult
{
    public bool HasConflict { get; set; }
    public int? ConflictingSlotId { get; set; }
    public string Message { get; set; } = string.Empty;
}
