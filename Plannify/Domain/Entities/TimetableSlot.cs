using Plannify.Application.Common;

namespace Plannify.Domain.Entities;

/// <summary>
/// TimetableSlot domain entity
/// Represents a single scheduled slot in the timetable
/// </summary>
public class TimetableSlot
{
    private TimetableSlot(
        int id,
        int semesterId,
        string day,
        TimeOnly startTime,
        TimeOnly endTime,
        int classBatchId,
        int? teacherId = null,
        int? subjectId = null,
        int? roomId = null,
        string slotType = "Theory",
        bool isLabSession = false,
        string? labGroupTag = null)
    {
        Id = id;
        SemesterId = semesterId;
        Day = day;
        StartTime = startTime;
        EndTime = endTime;
        ClassBatchId = classBatchId;
        TeacherId = teacherId;
        SubjectId = subjectId;
        RoomId = roomId;
        SlotType = slotType;
        IsLabSession = isLabSession;
        LabGroupTag = labGroupTag;
    }

    public int Id { get; private set; }
    public int SemesterId { get; private set; }
    public string Day { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public int ClassBatchId { get; private set; }
    public int? TeacherId { get; private set; }
    public int? SubjectId { get; private set; }
    public int? RoomId { get; private set; }
    public string SlotType { get; private set; }
    public bool IsLabSession { get; private set; }
    public string? LabGroupTag { get; private set; }

    private static readonly string[] ValidDays = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
    private static readonly string[] ValidSlotTypes = { "Theory", "Lab", "Practical", "Seminar", "Tutorial", "Project" };

    /// <summary>
    /// Factory method to create a new TimetableSlot with business rule validation
    /// </summary>
    public static Result<TimetableSlot> Create(
        int semesterId,
        string day,
        TimeOnly startTime,
        TimeOnly endTime,
        int classBatchId,
        int? teacherId = null,
        int? subjectId = null,
        int? roomId = null,
        string slotType = "Theory",
        bool isLabSession = false,
        string? labGroupTag = null)
    {
        // Validate semester ID
        if (semesterId <= 0)
            return Result<TimetableSlot>.Failure("Valid semester ID is required");

        // Validate day
        if (string.IsNullOrWhiteSpace(day))
            return Result<TimetableSlot>.Failure("Day is required");

        if (!ValidDays.Contains(day))
            return Result<TimetableSlot>.Failure($"Day must be one of: {string.Join(", ", ValidDays)}");

        // Validate times
        if (startTime >= endTime)
            return Result<TimetableSlot>.Failure("Start time must be before end time");

        // Validate slot duration (should be at least 30 minutes)
        var duration = endTime - startTime;
        if (duration.TotalMinutes < 30)
            return Result<TimetableSlot>.Failure("Slot duration must be at least 30 minutes");

        // Validate class batch ID
        if (classBatchId <= 0)
            return Result<TimetableSlot>.Failure("Valid class batch ID is required");

        // Validate teacher ID if provided
        if (teacherId.HasValue && teacherId <= 0)
            return Result<TimetableSlot>.Failure("Invalid teacher ID");

        // Validate subject ID if provided
        if (subjectId.HasValue && subjectId <= 0)
            return Result<TimetableSlot>.Failure("Invalid subject ID");

        // Validate room ID if provided
        if (roomId.HasValue && roomId <= 0)
            return Result<TimetableSlot>.Failure("Invalid room ID");

        // Validate slot type
        if (string.IsNullOrWhiteSpace(slotType))
            return Result<TimetableSlot>.Failure("Slot type is required");

        if (!ValidSlotTypes.Contains(slotType))
            return Result<TimetableSlot>.Failure($"Slot type must be one of: {string.Join(", ", ValidSlotTypes)}");

        // Validate lab group tag if lab session
        if (isLabSession && string.IsNullOrWhiteSpace(labGroupTag))
            return Result<TimetableSlot>.Failure("Lab group tag is required for lab sessions");

        // If subject is lab, should typically have a subject and room
        if (slotType == "Lab" && (!subjectId.HasValue || !roomId.HasValue))
            return Result<TimetableSlot>.Failure("Lab slots must have a subject and room assigned");

        return Result<TimetableSlot>.Success(
            new TimetableSlot(0, semesterId, day, startTime, endTime, classBatchId, teacherId, subjectId, roomId, slotType, isLabSession, labGroupTag));
    }

    /// <summary>
    /// Update timetable slot details with validation
    /// </summary>
    public Result Update(
        string day,
        TimeOnly startTime,
        TimeOnly endTime,
        int? teacherId = null,
        int? subjectId = null,
        int? roomId = null,
        string slotType = "Theory",
        bool isLabSession = false,
        string? labGroupTag = null)
    {
        // Validate day
        if (string.IsNullOrWhiteSpace(day))
            return Result.Failure("Day is required");

        if (!ValidDays.Contains(day))
            return Result.Failure($"Day must be one of: {string.Join(", ", ValidDays)}");

        // Validate times
        if (startTime >= endTime)
            return Result.Failure("Start time must be before end time");

        // Validate slot duration (should be at least 30 minutes)
        var duration = endTime - startTime;
        if (duration.TotalMinutes < 30)
            return Result.Failure("Slot duration must be at least 30 minutes");

        // Validate teacher ID if provided
        if (teacherId.HasValue && teacherId <= 0)
            return Result.Failure("Invalid teacher ID");

        // Validate subject ID if provided
        if (subjectId.HasValue && subjectId <= 0)
            return Result.Failure("Invalid subject ID");

        // Validate room ID if provided
        if (roomId.HasValue && roomId <= 0)
            return Result.Failure("Invalid room ID");

        // Validate slot type
        if (string.IsNullOrWhiteSpace(slotType))
            return Result.Failure("Slot type is required");

        if (!ValidSlotTypes.Contains(slotType))
            return Result.Failure($"Slot type must be one of: {string.Join(", ", ValidSlotTypes)}");

        // Validate lab group tag if lab session
        if (isLabSession && string.IsNullOrWhiteSpace(labGroupTag))
            return Result.Failure("Lab group tag is required for lab sessions");

        // If subject is lab, should typically have a subject and room
        if (slotType == "Lab" && (!subjectId.HasValue || !roomId.HasValue))
            return Result.Failure("Lab slots must have a subject and room assigned");

        Day = day;
        StartTime = startTime;
        EndTime = endTime;
        TeacherId = teacherId;
        SubjectId = subjectId;
        RoomId = roomId;
        SlotType = slotType;
        IsLabSession = isLabSession;
        LabGroupTag = labGroupTag;

        return Result.Success();
    }

    /// <summary>
    /// Assign teacher to this slot
    /// </summary>
    public Result AssignTeacher(int teacherId)
    {
        if (teacherId <= 0)
            return Result.Failure("Invalid teacher ID");

        TeacherId = teacherId;
        return Result.Success();
    }

    /// <summary>
    /// Remove teacher assignment
    /// </summary>
    public Result RemoveTeacher()
    {
        TeacherId = null;
        return Result.Success();
    }

    /// <summary>
    /// Assign subject to this slot
    /// </summary>
    public Result AssignSubject(int subjectId)
    {
        if (subjectId <= 0)
            return Result.Failure("Invalid subject ID");

        SubjectId = subjectId;
        return Result.Success();
    }

    /// <summary>
    /// Remove subject assignment
    /// </summary>
    public Result RemoveSubject()
    {
        SubjectId = null;
        return Result.Success();
    }

    /// <summary>
    /// Assign room to this slot
    /// </summary>
    public Result AssignRoom(int roomId)
    {
        if (roomId <= 0)
            return Result.Failure("Invalid room ID");

        RoomId = roomId;
        return Result.Success();
    }

    /// <summary>
    /// Remove room assignment
    /// </summary>
    public Result RemoveRoom()
    {
        RoomId = null;
        return Result.Success();
    }

    /// <summary>
    /// Check if slot overlaps with another time
    /// </summary>
    public bool OverlapsWith(TimeOnly otherStart, TimeOnly otherEnd)
    {
        return StartTime < otherEnd && EndTime > otherStart;
    }

    /// <summary>
    /// Get slot duration in minutes
    /// </summary>
    public int GetDurationMinutes()
    {
        return (int)(EndTime - StartTime).TotalMinutes;
    }
}
