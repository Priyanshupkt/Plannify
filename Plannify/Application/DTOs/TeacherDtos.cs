namespace Plannify.Application.DTOs;

/// <summary>
/// Request DTO for creating a new teacher
/// Only includes fields needed for creation
/// </summary>
public class CreateTeacherRequest
{
    public string FullName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string Initials { get; set; } = string.Empty;
    public string Designation { get; set; } = "Assistant Professor";
    public int MaxWeeklyHours { get; set; } = 18;
}

/// <summary>
/// Request DTO for updating a teacher
/// </summary>
public class UpdateTeacherRequest
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public int MaxWeeklyHours { get; set; }
    public int DepartmentId { get; set; }
}

/// <summary>
/// Response DTO for teacher information
/// Contains only what the UI needs to display
/// </summary>
public class TeacherDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int MaxWeeklyHours { get; set; }
    public decimal CurrentWeeklyHours { get; set; }
    public bool CanAcceptMore { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Lightweight DTO for dropdowns/lists
/// </summary>
public class TeacherSummaryDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
