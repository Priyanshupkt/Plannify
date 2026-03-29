using Plannify.Application.Common;

namespace Plannify.Domain.Entities;

/// <summary>
/// Clean domain entity for Department - no framework dependencies
/// Business logic and validation rules encapsulated here
/// </summary>
public class Department
{
    // Properties with private setters - only set through Create()
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Domain method: Create a new department with validation
    /// </summary>
    public static Result<Department> Create(string name, string code, string? shortName = null)
    {
        // Domain validation rules
        if (string.IsNullOrWhiteSpace(name))
            return Result<Department>.Failure("Department name is required");

        if (name.Length > 100)
            return Result<Department>.Failure("Department name must not exceed 100 characters");

        if (string.IsNullOrWhiteSpace(code))
            return Result<Department>.Failure("Department code is required");

        if (code.Length > 10)
            return Result<Department>.Failure("Department code must not exceed 10 characters");

        var department = new Department
        {
            Name = name.Trim(),
            Code = code.Trim().ToUpper(),
            ShortName = shortName?.Trim() ?? string.Empty
        };

        return Result<Department>.Success(department);
    }

    /// <summary>
    /// Domain method: Update department information
    /// </summary>
    public Result Update(string name, string code, string? shortName)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure("Department name is required");

        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure("Department code is required");

        Name = name.Trim();
        Code = code.Trim().ToUpper();
        ShortName = shortName?.Trim() ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}
