namespace Plannify.Services;

/// <summary>
/// Request DTO for generating an optimized timetable
/// </summary>
public class SchedulingRequest
{
    /// <summary>
    /// ID of the academic year
    /// </summary>
    public int AcademicYearId { get; set; }

    /// <summary>
    /// ID of the semester (optional, if null generate for all semesters in the year)
    /// </summary>
    public int? SemesterId { get; set; }

    /// <summary>
    /// Specific class ID to generate for (optional, if null generate for all classes)
    /// </summary>
    public int? ClassId { get; set; }

    /// <summary>
    /// Clear existing timetable before generation
    /// </summary>
    public bool ClearExisting { get; set; } = false;

    /// <summary>
    /// Starting hour for daily schedule (e.g., 9 for 9 AM)
    /// </summary>
    public int StartHour { get; set; } = 9;

    /// <summary>
    /// Ending hour for daily schedule (e.g., 17 for 5 PM)
    /// </summary>
    public int EndHour { get; set; } = 17;

    /// <summary>
    /// Slot duration in minutes
    /// </summary>
    public int SlotDurationMinutes { get; set; } = 60;
}

/// <summary>
/// Result DTO for timetable generation
/// </summary>
public class SchedulingResult
{
    /// <summary>
    /// Whether generation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Number of slots generated
    /// </summary>
    public int SlotsGenerated { get; set; }

    /// <summary>
    /// Number of conflicts detected (if any)
    /// </summary>
    public int ConflictsDetected { get; set; }

    /// <summary>
    /// Error or warning messages
    /// </summary>
    public List<string> Messages { get; set; } = new();

    /// <summary>
    /// Constraint violations found
    /// </summary>
    public List<ConstraintViolation> Violations { get; set; } = new();
}

/// <summary>
/// Represents a single constraint violation in the generated schedule
/// </summary>
public class ConstraintViolation
{
    public string Type { get; set; } = string.Empty; // "HardConstraint", "SoftConstraint"
    public string Description { get; set; } = string.Empty;
    public int? SlotId { get; set; }
    public string? Details { get; set; }
}

/// <summary>
/// Internal DTO representing an available time slot for assignment
/// </summary>
public class AvailableSlot
{
    public int SemesterId { get; set; }
    public int ClassBatchId { get; set; }
    public string Day { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int? RoomId { get; set; }
    public string SlotType { get; set; } = "Theory";

    /// <summary>
    /// Returns a unique key for this slot
    /// </summary>
    public string GetKey()
    {
        return $"{SemesterId}_{ClassBatchId}_{Day}_{StartTime:HHmm}_{EndTime:HHmm}_{RoomId}";
    }
}

/// <summary>
/// Internal DTO representing a class-subject assignment that needs scheduling
/// </summary>
public class ClassSubjectAssignment
{
    public int ClassBatchId { get; set; }
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string SubjectType { get; set; } = string.Empty;
    public List<int> AvailableTeacherIds { get; set; } = new();
    public int RequiredSessions { get; set; } = 1; // Per week
    public int HoursPerSession { get; set; } = 1;
    public int ClassStrength { get; set; }
}
