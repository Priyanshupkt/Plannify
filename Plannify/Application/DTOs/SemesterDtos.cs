namespace Plannify.Application.DTOs;

/// <summary>
/// Request DTO for creating a semester
/// </summary>
public class CreateSemesterRequest
{
    public string Name { get; set; } = string.Empty;
    public int SemesterNumber { get; set; }
    public int AcademicYearId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

/// <summary>
/// Request DTO for updating a semester
/// </summary>
public class UpdateSemesterRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SemesterNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

/// <summary>
/// Response DTO for semester details
/// </summary>
public class SemesterDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SemesterNumber { get; set; }
    public int AcademicYearId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsCurrent { get; set; }
}

/// <summary>
/// Summary DTO for semester list views
/// </summary>
public class SemesterSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SemesterNumber { get; set; }
    public int AcademicYearId { get; set; }
}
