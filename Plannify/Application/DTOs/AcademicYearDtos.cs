namespace Plannify.Application.DTOs;

/// <summary>
/// Request DTO for creating an academic year
/// </summary>
public class CreateAcademicYearRequest
{
    public string YearLabel { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

/// <summary>
/// Request DTO for updating an academic year
/// </summary>
public class UpdateAcademicYearRequest
{
    public int Id { get; set; }
    public string YearLabel { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

/// <summary>
/// Response DTO for academic year details
/// </summary>
public class AcademicYearDto
{
    public int Id { get; set; }
    public string YearLabel { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsCurrent { get; set; }
}

/// <summary>
/// Summary DTO for academic year list views
/// </summary>
public class AcademicYearSummaryDto
{
    public int Id { get; set; }
    public string YearLabel { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
}
