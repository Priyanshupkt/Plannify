using Plannify.Application.Common;

namespace Plannify.Domain.Entities;

/// <summary>
/// Timetable domain entity (Aggregate Root)
/// Represents a complete timetable for a semester with all scheduled slots
/// Manages collection of TimetableSlots and enforces scheduling constraints
/// </summary>
public class Timetable
{
    private Timetable(
        int id,
        int semesterId,
        string name,
        DateTime createdAt,
        DateTime? finalizedAt = null,
        bool isFinalized = false)
    {
        Id = id;
        SemesterId = semesterId;
        Name = name;
        CreatedAt = createdAt;
        FinalizedAt = finalizedAt;
        IsFinalized = isFinalized;
        TimetableSlots = new List<TimetableSlot>();
    }

    public int Id { get; private set; }
    public int SemesterId { get; private set; }
    public string Name { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? FinalizedAt { get; private set; }
    public bool IsFinalized { get; private set; }

    // Navigation property for TimetableSlots collection
    public ICollection<TimetableSlot> TimetableSlots { get; private set; }

    /// <summary>
    /// Factory method to create a new Timetable with business rule validation
    /// </summary>
    public static Result<Timetable> Create(
        int semesterId,
        string name)
    {
        // Validate semester ID
        if (semesterId <= 0)
            return Result<Timetable>.Failure("Valid semester ID is required");

        // Validate name
        if (string.IsNullOrWhiteSpace(name))
            return Result<Timetable>.Failure("Timetable name is required");

        if (name.Length < 2 || name.Length > 100)
            return Result<Timetable>.Failure("Timetable name must be between 2 and 100 characters");

        var timetable = new Timetable(
            id: 0,
            semesterId: semesterId,
            name: name.Trim(),
            createdAt: DateTime.UtcNow);

        return Result<Timetable>.Success(timetable);
    }

    /// <summary>
    /// Update timetable name
    /// </summary>
    public (bool Success, string? Error) Update(string name)
    {
        if (IsFinalized)
            return (false, "Cannot update a finalized timetable");

        if (string.IsNullOrWhiteSpace(name))
            return (false, "Timetable name is required");

        if (name.Length < 2 || name.Length > 100)
            return (false, "Timetable name must be between 2 and 100 characters");

        Name = name.Trim();
        return (true, null);
    }

    /// <summary>
    /// Finalize the timetable (lock it against further modifications to slots)
    /// </summary>
    public (bool Success, string? Error) Finalize()
    {
        if (IsFinalized)
            return (false, "Timetable is already finalized");

        if (!TimetableSlots.Any())
            return (false, "Cannot finalize a timetable with no slots");

        IsFinalized = true;
        FinalizedAt = DateTime.UtcNow;
        return (true, null);
    }

    /// <summary>
    /// Unfinalizes the timetable to allow further modifications
    /// </summary>
    public (bool Success, string? Error) Unfinalize()
    {
        if (!IsFinalized)
            return (false, "Timetable is not finalized");

        IsFinalized = false;
        FinalizedAt = null;
        return (true, null);
    }

    /// <summary>
    /// Add a slot to the timetable
    /// </summary>
    public (bool Success, string? Error) AddSlot(TimetableSlot slot)
    {
        if (IsFinalized)
            return (false, "Cannot add slots to a finalized timetable");

        if (slot == null)
            return (false, "Slot cannot be null");

        if (slot.SemesterId != SemesterId)
            return (false, "Slot must belong to the same semester as the timetable");

        TimetableSlots.Add(slot);
        return (true, null);
    }

    /// <summary>
    /// Remove a slot from the timetable
    /// </summary>
    public (bool Success, string? Error) RemoveSlot(TimetableSlot slot)
    {
        if (IsFinalized)
            return (false, "Cannot remove slots from a finalized timetable");

        if (slot == null)
            return (false, "Slot cannot be null");

        if (!TimetableSlots.Contains(slot))
            return (false, "Slot is not part of this timetable");

        TimetableSlots.Remove(slot);
        return (true, null);
    }

    /// <summary>
    /// Get all slots for a specific class batch
    /// </summary>
    public IEnumerable<TimetableSlot> GetClassBatchSlots(int classBatchId)
    {
        return TimetableSlots.Where(s => s.ClassBatchId == classBatchId).OrderBy(s => s.Day).ThenBy(s => s.StartTime);
    }

    /// <summary>
    /// Get all slots for a specific teacher
    /// </summary>
    public IEnumerable<TimetableSlot> GetTeacherSlots(int teacherId)
    {
        return TimetableSlots.Where(s => s.TeacherId == teacherId).OrderBy(s => s.Day).ThenBy(s => s.StartTime);
    }

    /// <summary>
    /// Get all slots assigned to a specific room
    /// </summary>
    public IEnumerable<TimetableSlot> GetRoomSlots(int roomId)
    {
        return TimetableSlots.Where(s => s.RoomId == roomId).OrderBy(s => s.Day).ThenBy(s => s.StartTime);
    }

    /// <summary>
    /// Get slots for a specific day
    /// </summary>
    public IEnumerable<TimetableSlot> GetDaySlots(string day)
    {
        return TimetableSlots.Where(s => s.Day == day).OrderBy(s => s.StartTime);
    }

    /// <summary>
    /// Get total number of slots in the timetable
    /// </summary>
    public int GetSlotCount()
    {
        return TimetableSlots.Count;
    }

    /// <summary>
    /// Check if timetable has any unassigned class batches
    /// </summary>
    public bool HasUnassignedClassBatches(IEnumerable<int> allClassBatchIds)
    {
        var assignedBatches = TimetableSlots.Select(s => s.ClassBatchId).Distinct();
        return allClassBatchIds.Any(id => !assignedBatches.Contains(id));
    }

    /// <summary>
    /// Calculate coverage percentage for a class batch
    /// </summary>
    public decimal GetClassBatchCoveragePercentage(int classBatchId, int totalExpectedSlots)
    {
        if (totalExpectedSlots <= 0)
            return 0;

        var count = TimetableSlots.Count(s => s.ClassBatchId == classBatchId);
        return (decimal)count / totalExpectedSlots * 100;
    }

    /// <summary>
    /// Get statistics about the timetable
    /// </summary>
    public TimetableStatistics GetStatistics()
    {
        return new TimetableStatistics
        {
            TotalSlots = TimetableSlots.Count,
            TotalClassBatches = TimetableSlots.Select(s => s.ClassBatchId).Distinct().Count(),
            TotalTeachers = TimetableSlots.Where(s => s.TeacherId.HasValue).Select(s => s.TeacherId).Distinct().Count(),
            TotalRooms = TimetableSlots.Where(s => s.RoomId.HasValue).Select(s => s.RoomId).Distinct().Count(),
            TotalSubjects = TimetableSlots.Where(s => s.SubjectId.HasValue).Select(s => s.SubjectId).Distinct().Count(),
            LabSessions = TimetableSlots.Count(s => s.IsLabSession),
            UnassignedTeacherSlots = TimetableSlots.Count(s => !s.TeacherId.HasValue),
            UnassignedSubjectSlots = TimetableSlots.Count(s => !s.SubjectId.HasValue),
            UnassignedRoomSlots = TimetableSlots.Count(s => !s.RoomId.HasValue)
        };
    }
}

/// <summary>
/// DTO for timetable statistics
/// </summary>
public class TimetableStatistics
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

    public decimal GetCompletionPercentage()
    {
        if (TotalSlots == 0)
            return 0;

        int assignedSlots = TotalSlots - UnassignedTeacherSlots - UnassignedSubjectSlots - UnassignedRoomSlots;
        return (decimal)assignedSlots / TotalSlots * 100;
    }
}
