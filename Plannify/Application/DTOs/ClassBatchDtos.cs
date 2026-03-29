namespace Plannify.Application.DTOs;

/// <summary>
/// Request DTO for creating a class batch
/// </summary>
public class CreateClassBatchRequest
{
    public string BatchName { get; set; } = string.Empty;
    public int Strength { get; set; }
    public int Semester { get; set; }
    public int DepartmentId { get; set; }
    public int AcademicYearId { get; set; }
    public int? RoomId { get; set; }
}

/// <summary>
/// Request DTO for updating a class batch
/// </summary>
public class UpdateClassBatchRequest
{
    public int Id { get; set; }
    public string BatchName { get; set; } = string.Empty;
    public int Strength { get; set; }
    public int Semester { get; set; }
    public int DepartmentId { get; set; }
    public int AcademicYearId { get; set; }
    public int? RoomId { get; set; }
}

/// <summary>
/// Response DTO for class batch details
/// </summary>
public class ClassBatchDto
{
    public int Id { get; set; }
    public string BatchName { get; set; } = string.Empty;
    public int Strength { get; set; }
    public int Semester { get; set; }
    public int DepartmentId { get; set; }
    public int AcademicYearId { get; set; }
    public int? RoomId { get; set; }
    public bool IsActive { get; set; }
    public string? DepartmentName { get; set; }
    public string? RoomNumber { get; set; }
}

/// <summary>
/// Summary DTO for class batch list views
/// </summary>
public class ClassBatchSummaryDto
{
    public int Id { get; set; }
    public string BatchName { get; set; } = string.Empty;
    public int Semester { get; set; }
    public int Strength { get; set; }
    public int DepartmentId { get; set; }
}
