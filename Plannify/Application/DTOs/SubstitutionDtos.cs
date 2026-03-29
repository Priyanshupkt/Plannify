namespace Plannify.Application.DTOs;

/// <summary>
/// DTO for creating a new substitution record
/// </summary>
public class CreateSubstitutionRequest
{
    public int TimetableSlotId { get; set; }
    public int OriginalTeacherId { get; set; }
    public int SubstituteTeacherId { get; set; }
    public DateOnly Date { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating a substitution record
/// </summary>
public class UpdateSubstitutionRequest
{
    public string Reason { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
}

/// <summary>
/// DTO for changing the substitute teacher
/// </summary>
public class ChangeSubstituteRequest
{
    public int NewSubstituteTeacherId { get; set; }
}

/// <summary>
/// DTO for substitution response (full details)
/// </summary>
public class SubstitutionDto
{
    public int Id { get; set; }
    public int TimetableSlotId { get; set; }
    public int OriginalTeacherId { get; set; }
    public int SubstituteTeacherId { get; set; }
    public string? OriginalTeacherName { get; set; }
    public string? SubstituteTeacherName { get; set; }
    public DateOnly Date { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsUrgent { get; set; }
}

/// <summary>
/// DTO for substitution summary (minimal details for lists)
/// </summary>
public class SubstitutionSummaryDto
{
    public int Id { get; set; }
    public int TimetableSlotId { get; set; }
    public string? OriginalTeacherName { get; set; }
    public string? SubstituteTeacherName { get; set; }
    public DateOnly Date { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
