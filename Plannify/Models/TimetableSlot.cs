namespace Plannify.Models;

public class TimetableSlot
{
    public int Id { get; set; }
    public required string Day { get; set; }
    public required string StartTime { get; set; }
    public required string EndTime { get; set; }
    public int TeacherId { get; set; }
    public int SubjectId { get; set; }
    public int ClassId { get; set; }
    public bool IsGap { get; set; }

    public Teacher? Teacher { get; set; }
    public Subject? Subject { get; set; }
    public Class? Class { get; set; }
}
