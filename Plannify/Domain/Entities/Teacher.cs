using Plannify.Application.Common;

namespace Plannify.Domain.Entities;

/// <summary>
/// Clean domain entity for Teacher - no framework dependencies
/// Business logic and validation rules encapsulated here
/// </summary>
public class Teacher
{
    // Properties with private setters - only set through Create() or explicit methods
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string Designation { get; set; } = "Assistant Professor";
    public int MaxWeeklyHours { get; set; } = 18;
    public int DepartmentId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties - private to prevent direct modification
    public virtual Department? Department { get; set; }
    public virtual ICollection<TimetableSlot>? TimetableSlots { get; set; }

    /// <summary>
    /// Domain method: Check if teacher can accept more hours
    /// </summary>
    public bool CanAcceptMoreHours(decimal allocatedHours)
        => allocatedHours < MaxWeeklyHours;

    /// <summary>
    /// Domain method: Create a new teacher with validation
    /// </summary>
    public static Result<Teacher> Create(
        string fullName, 
        string employeeCode, 
        string email, 
        int departmentId, 
        string initials)
    {
        // Domain validation rules
        if (string.IsNullOrWhiteSpace(fullName))
            return Result<Teacher>.Failure("Full name is required");
        
        if (fullName.Length > 100)
            return Result<Teacher>.Failure("Full name must not exceed 100 characters");

        if (string.IsNullOrWhiteSpace(employeeCode))
            return Result<Teacher>.Failure("Employee code is required");
        
        if (employeeCode.Length > 20)
            return Result<Teacher>.Failure("Employee code must not exceed 20 characters");

        if (!string.IsNullOrEmpty(email) && !email.Contains("@"))
            return Result<Teacher>.Failure("Email format is invalid");

        if (departmentId <= 0)
            return Result<Teacher>.Failure("Valid department must be specified");

        var teacher = new Teacher
        {
            FullName = fullName.Trim(),
            EmployeeCode = employeeCode.Trim(),
            Email = email?.Trim() ?? string.Empty,
            DepartmentId = departmentId,
            Initials = initials?.Trim() ?? string.Empty
        };

        return Result<Teacher>.Success(teacher);
    }

    /// <summary>
    /// Domain method: Update teacher information
    /// </summary>
    public Result Update(string fullName, string email, string designation, int maxWeeklyHours, int departmentId)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return Result.Failure("Full name is required");

        if (maxWeeklyHours <= 0 || maxWeeklyHours > 40)
            return Result.Failure("Max weekly hours must be between 1 and 40");

        FullName = fullName.Trim();
        Email = email?.Trim() ?? string.Empty;
        Designation = designation;
        MaxWeeklyHours = maxWeeklyHours;
        DepartmentId = departmentId;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Domain method: Deactivate teacher
    /// </summary>
    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure("Teacher is already inactive");

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
