namespace Plannify.Application.DTOs;

/// <summary>
/// Request DTO for creating a subject
/// </summary>
public class CreateSubjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public int SemesterNumber { get; set; }
    public int Credits { get; set; }
    public int MaxClassesPerWeek { get; set; } = 5;
}

/// <summary>
/// Request DTO for updating a subject
/// </summary>
public class UpdateSubjectRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public int SemesterNumber { get; set; }
    public int Credits { get; set; }
    public int MaxClassesPerWeek { get; set; }
}

/// <summary>
/// Response DTO for subject details
/// </summary>
public class SubjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int SemesterNumber { get; set; }
    public int Credits { get; set; }
    public int MaxClassesPerWeek { get; set; }
    public int AllocatedClassesPerWeek { get; set; }
}

/// <summary>
/// Summary DTO for subject list views
/// </summary>
public class SubjectSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public int SemesterNumber { get; set; }
    public int Credits { get; set; }
}
