using Plannify.Application.Common;

namespace Plannify.Domain.Entities;

/// <summary>
/// Subject domain entity
/// Represents a course/subject taught in the institution
/// </summary>
public class Subject
{
    // Parameterless constructor for EF Core
    public Subject()
    {
        Name = string.Empty;
        Code = string.Empty;
    }

    private Subject(int id, string name, string code, int departmentId, int semesterNumber, int credits, int maxClassesPerWeek)
    {
        Id = id;
        Name = name;
        Code = code;
        DepartmentId = departmentId;
        SemesterNumber = semesterNumber;
        Credits = credits;
        MaxClassesPerWeek = maxClassesPerWeek;
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public int DepartmentId { get; set; }
    public int SemesterNumber { get; set; }
    public int Credits { get; set; }
    public int MaxClassesPerWeek { get; set; }
    
    // Navigation property
    public string? SubjectType { get; set; } = "Theory"; // Theory, Lab, Practical, etc.
    public Department? Department { get; set; }
    public ICollection<TimetableSlot>? TimetableSlots { get; set; }

    /// <summary>
    /// Factory method to create a new Subject with business rule validation
    /// </summary>
    public static Result<Subject> Create(string name, string code, int departmentId, int semesterNumber, int credits, int maxClassesPerWeek = 5)
    {
        // Validate name
        if (string.IsNullOrWhiteSpace(name))
            return Result<Subject>.Failure("Subject name is required");

        if (name.Length < 2 || name.Length > 100)
            return Result<Subject>.Failure("Subject name must be between 2 and 100 characters");

        // Validate code
        if (string.IsNullOrWhiteSpace(code))
            return Result<Subject>.Failure("Subject code is required");

        if (code.Length < 1 || code.Length > 20)
            return Result<Subject>.Failure("Subject code must be between 1 and 20 characters");

        // Validate department ID
        if (departmentId <= 0)
            return Result<Subject>.Failure("Valid department ID is required");

        // Validate semester
        if (semesterNumber < 1 || semesterNumber > 8)
            return Result<Subject>.Failure("Semester number must be between 1 and 8");

        // Validate credits
        if (credits < 1 || credits > 10)
            return Result<Subject>.Failure("Credits must be between 1 and 10");

        // Validate max classes per week
        if (maxClassesPerWeek < 1 || maxClassesPerWeek > 10)
            return Result<Subject>.Failure("Max classes per week must be between 1 and 10");

        return Result<Subject>.Success(new Subject(0, name, code, departmentId, semesterNumber, credits, maxClassesPerWeek));
    }

    /// <summary>
    /// Update subject details with validation
    /// </summary>
    public Result Update(string name, string code, int departmentId, int semesterNumber, int credits, int maxClassesPerWeek)
    {
        // Validate name
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure("Subject name is required");

        if (name.Length < 2 || name.Length > 100)
            return Result.Failure("Subject name must be between 2 and 100 characters");

        // Validate code
        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure("Subject code is required");

        if (code.Length < 1 || code.Length > 20)
            return Result.Failure("Subject code must be between 1 and 20 characters");

        // Validate department ID
        if (departmentId <= 0)
            return Result.Failure("Valid department ID is required");

        // Validate semester
        if (semesterNumber < 1 || semesterNumber > 8)
            return Result.Failure("Semester number must be between 1 and 8");

        // Validate credits
        if (credits < 1 || credits > 10)
            return Result.Failure("Credits must be between 1 and 10");

        // Validate max classes per week
        if (maxClassesPerWeek < 1 || maxClassesPerWeek > 10)
            return Result.Failure("Max classes per week must be between 1 and 10");

        Name = name;
        Code = code;
        DepartmentId = departmentId;
        SemesterNumber = semesterNumber;
        Credits = credits;
        MaxClassesPerWeek = maxClassesPerWeek;

        return Result.Success();
    }
}
