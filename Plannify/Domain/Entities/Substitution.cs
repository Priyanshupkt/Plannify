namespace Plannify.Domain.Entities;

/// <summary>
/// Represents a teacher substitution for a planned timetable slot.
/// Handles cases where the original teacher is absent and needs a substitute.
/// This is a pure domain entity with factory method validation.
/// </summary>
public class Substitution
{
    /// <summary>
    /// Unique identifier for the substitution record
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Foreign key to the timetable slot being substituted
    /// </summary>
    public int TimetableSlotId { get; private set; }

    /// <summary>
    /// Foreign key to the original teacher (absent)
    /// </summary>
    public int OriginalTeacherId { get; private set; }

    /// <summary>
    /// Foreign key to the substitute teacher (available)
    /// </summary>
    public int SubstituteTeacherId { get; private set; }

    /// <summary>
    /// Date of the substitution (when the absence occurs)
    /// </summary>
    public DateOnly Date { get; private set; }

    /// <summary>
    /// Reason for the substitution (e.g., "Medical Leave", "Conference", etc.)
    /// </summary>
    public string Reason { get; private set; } = string.Empty;

    /// <summary>
    /// Identifier of the person who approved the substitution
    /// </summary>
    public string ApprovedBy { get; private set; } = string.Empty;

    /// <summary>
    /// Timestamp when the substitution was created
    /// </summary>
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Factory method to create a new substitution record with validation
    /// </summary>
    /// <param name="timetableSlotId">ID of the slot being substituted</param>
    /// <param name="originalTeacherId">ID of the absent teacher</param>
    /// <param name="substituteTeacherId">ID of the substitute teacher</param>
    /// <param name="date">Date of substitution</param>
    /// <param name="reason">Reason for substitution</param>
    /// <param name="approvedBy">User who approved the substitution</param>
    /// <returns>Tuple with (success boolean, error message if any)</returns>
    public static (bool Success, string? Error) Create(
        int timetableSlotId,
        int originalTeacherId,
        int substituteTeacherId,
        DateOnly date,
        string reason,
        string approvedBy)
    {
        // Validation: Timetable slot ID must be positive
        if (timetableSlotId <= 0)
            return (false, "Timetable slot ID must be positive");

        // Validation: Original teacher ID must be positive
        if (originalTeacherId <= 0)
            return (false, "Original teacher ID must be positive");

        // Validation: Substitute teacher ID must be positive
        if (substituteTeacherId <= 0)
            return (false, "Substitute teacher ID must be positive");

        // Validation: Cannot substitute with the same teacher
        if (originalTeacherId == substituteTeacherId)
            return (false, "Original teacher and substitute teacher cannot be the same");

        // Validation: Reason must not be empty
        if (string.IsNullOrWhiteSpace(reason))
            return (false, "Substitution reason is required");

        // Validation: Reason must be reasonable length
        if (reason.Length > 500)
            return (false, "Substitution reason must not exceed 500 characters");

        // Validation: ApprovedBy must not be empty
        if (string.IsNullOrWhiteSpace(approvedBy))
            return (false, "Approver identification is required");

        // Validation: Date should not be in the past (allowing today for same-day substitutions)
        if (date < DateOnly.FromDateTime(DateTime.Today))
            return (false, "Substitution date cannot be in the past");

        // Create new instance
        var substitution = new Substitution
        {
            TimetableSlotId = timetableSlotId,
            OriginalTeacherId = originalTeacherId,
            SubstituteTeacherId = substituteTeacherId,
            Date = date,
            Reason = reason.Trim(),
            ApprovedBy = approvedBy.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        return (true, null);
    }

    /// <summary>
    /// Update the substitution record (for cases where approval or reason needs to change)
    /// </summary>
    /// <param name="reason">Updated reason</param>
    /// <param name="approvedBy">Updated approver</param>
    /// <returns>Tuple with (success boolean, error message if any)</returns>
    public (bool Success, string? Error) Update(string reason, string approvedBy)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(reason))
            return (false, "Substitution reason is required");

        if (reason.Length > 500)
            return (false, "Substitution reason must not exceed 500 characters");

        if (string.IsNullOrWhiteSpace(approvedBy))
            return (false, "Approver identification is required");

        // Update
        Reason = reason.Trim();
        ApprovedBy = approvedBy.Trim();

        return (true, null);
    }

    /// <summary>
    /// Change the substitute teacher (for finding alternative substitutes)
    /// </summary>
    /// <param name="newSubstituteTeacherId">ID of the new substitute teacher</param>
    /// <returns>Tuple with (success boolean, error message if any)</returns>
    public (bool Success, string? Error) ChangeSubstitute(int newSubstituteTeacherId)
    {
        // Validation
        if (newSubstituteTeacherId <= 0)
            return (false, "New substitute teacher ID must be positive");

        if (newSubstituteTeacherId == OriginalTeacherId)
            return (false, "Substitute teacher cannot be the original teacher");

        if (newSubstituteTeacherId == SubstituteTeacherId)
            return (false, "New substitute teacher must be different from current substitute");

        // Update
        SubstituteTeacherId = newSubstituteTeacherId;

        return (true, null);
    }

    /// <summary>
    /// Check if this substitution is for the specified teacher (either role)
    /// </summary>
    /// <param name="teacherId">Teacher ID to check</param>
    /// <returns>True if teacher is involved in this substitution</returns>
    public bool InvolvesTeacher(int teacherId)
    {
        return OriginalTeacherId == teacherId || SubstituteTeacherId == teacherId;
    }

    /// <summary>
    /// Check if this substitution covers a specific timetable slot
    /// </summary>
    /// <param name="slotId">Slot ID to check</param>
    /// <returns>True if this substitution is for the specified slot</returns>
    public bool CoversSlot(int slotId)
    {
        return TimetableSlotId == slotId;
    }

    /// <summary>
    /// Check if this substitution is for a specific date
    /// </summary>
    /// <param name="checkDate">Date to check</param>
    /// <returns>True if substitution is for the specified date</returns>
    public bool IsForDate(DateOnly checkDate)
    {
        return Date == checkDate;
    }

    /// <summary>
    /// Get the duration since this substitution was created (for audit purposes)
    /// </summary>
    /// <returns>TimeSpan representing how long ago this was created</returns>
    public TimeSpan GetTimeSinceCreation()
    {
        return DateTime.UtcNow - CreatedAt;
    }

    /// <summary>
    /// Check if this substitution needs immediate attention (created recently but not yet acted upon)
    /// </summary>
    /// <returns>True if created within the last day and not too old</returns>
    public bool IsUrgent()
    {
        var hoursSinceCreation = GetTimeSinceCreation().TotalHours;
        var daysUntilSubstitution = (Date.ToDateTime(new TimeOnly()) - DateTime.Today).TotalDays;

        // Urgent if created recently AND substitution is happening soon
        return hoursSinceCreation < 24 && daysUntilSubstitution <= 7;
    }
}
