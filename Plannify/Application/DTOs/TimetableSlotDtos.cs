namespace Plannify.Application.DTOs;

/// <summary>
/// Request DTO for creating a timetable slot
/// </summary>
public class CreateTimetableSlotRequest
{
    public int SemesterId { get; set; }
    public string Day { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int ClassBatchId { get; set; }
    public int? TeacherId { get; set; }
    public int? SubjectId { get; set; }
    public int? RoomId { get; set; }
    public string SlotType { get; set; } = "Theory";
    public bool IsLabSession { get; set; }
    public string? LabGroupTag { get; set; }
}

/// <summary>
/// Request DTO for updating a timetable slot
/// </summary>
public class UpdateTimetableSlotRequest
{
    public int Id { get; set; }
    public string Day { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int? TeacherId { get; set; }
    public int? SubjectId { get; set; }
    public int? RoomId { get; set; }
    public string SlotType { get; set; } = "Theory";
    public bool IsLabSession { get; set; }
    public string? LabGroupTag { get; set; }
}

/// <summary>
/// Response DTO for timetable slot details
/// </summary>
public class TimetableSlotDto
{
    public int Id { get; set; }
    public int SemesterId { get; set; }
    public string Day { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int ClassBatchId { get; set; }
    public int? TeacherId { get; set; }
    public int? SubjectId { get; set; }
    public int? RoomId { get; set; }
    public string SlotType { get; set; } = string.Empty;
    public bool IsLabSession { get; set; }
    public string? LabGroupTag { get; set; }
    // Navigation properties for display
    public string? TeacherName { get; set; }
    public string? SubjectName { get; set; }
    public string? RoomNumber { get; set; }
    public string? BatchName { get; set; }
}

/// <summary>
/// Summary DTO for timetable slot list views
/// </summary>
public class TimetableSlotSummaryDto
{
    public int Id { get; set; }
    public string Day { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int ClassBatchId { get; set; }
    public int SemesterId { get; set; }
    public string SlotType { get; set; } = string.Empty;
}
