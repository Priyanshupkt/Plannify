namespace Plannify.Application.DTOs;

/// <summary>
/// Request DTO for creating a new Timetable
/// </summary>
public class CreateTimetableRequest
{
    public int SemesterId { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for updating a Timetable
/// </summary>
public class UpdateTimetableRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Response DTO for Timetable with detailed information
/// </summary>
public class TimetableDto
{
    public int Id { get; set; }
    public int SemesterId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? FinalizedAt { get; set; }
    public bool IsFinalized { get; set; }
    public int SlotCount { get; set; }
    public string SemesterCode { get; set; } = string.Empty;
    public TimetableSlotSummaryDto[] Slots { get; set; } = Array.Empty<TimetableSlotSummaryDto>();
}

/// <summary>
/// Summary DTO for Timetable (minimal information for lists)
/// </summary>
public class TimetableSummaryDto
{
    public int Id { get; set; }
    public int SemesterId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsFinalized { get; set; }
    public int SlotCount { get; set; }
}

/// <summary>
/// DTO for timetable statistics
/// </summary>
public class TimetableStatisticsDto
{
    public int TotalSlots { get; set; }
    public int TotalClassBatches { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalRooms { get; set; }
    public int TotalSubjects { get; set; }
    public int LabSessions { get; set; }
    public int UnassignedTeacherSlots { get; set; }
    public int UnassignedSubjectSlots { get; set; }
    public int UnassignedRoomSlots { get; set; }
    public decimal CompletionPercentage { get; set; }
}
