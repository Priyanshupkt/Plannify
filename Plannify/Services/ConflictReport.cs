namespace Plannify.Services;

/// <summary>
/// Detailed conflict report for all overlapping slots in a semester
/// </summary>
public class ConflictReport
{
    public string ConflictType { get; set; } = string.Empty;
    
    public string Day { get; set; } = string.Empty;
    
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    
    public int Slot1Id { get; set; }
    public string Slot1Description { get; set; } = string.Empty;
    
    public int Slot2Id { get; set; }
    public string Slot2Description { get; set; } = string.Empty;
    
    public string AffectedEntity { get; set; } = string.Empty;
}
