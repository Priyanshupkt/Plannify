using Plannify.Application.Common;

namespace Plannify.Domain.Entities;

/// <summary>
/// Semester domain entity
/// Represents an academic semester/term
/// </summary>
public class Semester
{
    private Semester(int id, string name, int semesterNumber, int academicYearId, DateTime startDate, DateTime endDate)
    {
        Id = id;
        Name = name;
        SemesterNumber = semesterNumber;
        AcademicYearId = academicYearId;
        StartDate = startDate;
        EndDate = endDate;
    }

    public int Id { get; private set; }
    public string Name { get; private set; }
    public int SemesterNumber { get; private set; }
    public int AcademicYearId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Factory method to create a new Semester with business rule validation
    /// </summary>
    public static Result<Semester> Create(string name, int semesterNumber, int academicYearId, DateTime startDate, DateTime endDate)
    {
        // Validate name
        if (string.IsNullOrWhiteSpace(name))
            return Result<Semester>.Failure("Semester name is required");

        if (name.Length < 2 || name.Length > 100)
            return Result<Semester>.Failure("Semester name must be between 2 and 100 characters");

        // Validate semester number
        if (semesterNumber < 1 || semesterNumber > 8)
            return Result<Semester>.Failure("Semester number must be between 1 and 8");

        // Validate academic year ID
        if (academicYearId <= 0)
            return Result<Semester>.Failure("Valid academic year ID is required");

        // Validate dates
        if (startDate >= endDate)
            return Result<Semester>.Failure("Start date must be before end date");

        if (startDate < DateTime.Now.AddYears(-5) || endDate > DateTime.Now.AddYears(5))
            return Result<Semester>.Failure("Semester dates must be within reasonable range");

        return Result<Semester>.Success(new Semester(0, name, semesterNumber, academicYearId, startDate, endDate));
    }

    /// <summary>
    /// Update semester details with validation
    /// </summary>
    public Result Update(string name, int semesterNumber, DateTime startDate, DateTime endDate)
    {
        // Validate name
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure("Semester name is required");

        if (name.Length < 2 || name.Length > 100)
            return Result.Failure("Semester name must be between 2 and 100 characters");

        // Validate semester number
        if (semesterNumber < 1 || semesterNumber > 8)
            return Result.Failure("Semester number must be between 1 and 8");

        // Validate dates
        if (startDate >= endDate)
            return Result.Failure("Start date must be before end date");

        if (startDate < DateTime.Now.AddYears(-5) || endDate > DateTime.Now.AddYears(5))
            return Result.Failure("Semester dates must be within reasonable range");

        Name = name;
        SemesterNumber = semesterNumber;
        StartDate = startDate;
        EndDate = endDate;

        return Result.Success();
    }

    /// <summary>
    /// Check if semester is currently active
    /// </summary>
    public bool IsCurrent()
    {
        var now = DateTime.Now;
        return IsActive && now >= StartDate && now <= EndDate;
    }
}
