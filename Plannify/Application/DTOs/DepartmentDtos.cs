namespace Plannify.Application.DTOs;

/// <summary>
/// Request DTO for creating a new department
/// </summary>
public class CreateDepartmentRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? ShortName { get; set; }
}

/// <summary>
/// Request DTO for updating a department
/// </summary>
public class UpdateDepartmentRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? ShortName { get; set; }
}

/// <summary>
/// Response DTO for department information
/// Contains only what the UI needs to display
/// </summary>
public class DepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public int TeacherCount { get; set; }
    public int SubjectCount { get; set; }
    public int ClassCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Lightweight DTO for dropdowns
/// </summary>
public class DepartmentSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
