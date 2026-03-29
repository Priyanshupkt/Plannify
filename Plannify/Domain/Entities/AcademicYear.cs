using Plannify.Application.Common;

namespace Plannify.Domain.Entities;

/// <summary>
/// AcademicYear domain entity
/// Represents an academic year/calendar
/// </summary>
public class AcademicYear
{
    // Parameterless constructor for EF Core
    public AcademicYear()
    {
        YearLabel = string.Empty;
    }

    private AcademicYear(int id, string yearLabel, DateTime startDate, DateTime endDate)
    {
        Id = id;
        YearLabel = yearLabel;
        StartDate = startDate;
        EndDate = endDate;
    }

    public int Id { get; set; }
    public string YearLabel { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Factory method to create a new AcademicYear with business rule validation
    /// </summary>
    public static Result<AcademicYear> Create(string yearLabel, DateTime startDate, DateTime endDate)
    {
        // Validate year label
        if (string.IsNullOrWhiteSpace(yearLabel))
            return Result<AcademicYear>.Failure("Year label is required");

        if (yearLabel.Length < 2 || yearLabel.Length > 50)
            return Result<AcademicYear>.Failure("Year label must be between 2 and 50 characters");

        // Validate dates
        if (startDate >= endDate)
            return Result<AcademicYear>.Failure("Start date must be before end date");

        if (startDate < DateTime.Now.AddYears(-10) || endDate > DateTime.Now.AddYears(10))
            return Result<AcademicYear>.Failure("Academic year dates must be within reasonable range (±10 years)");

        // Year should typically span ~365 days (allow ±30 days)
        var daysDifference = (endDate - startDate).TotalDays;
        if (daysDifference < 335 || daysDifference > 395)
            return Result<AcademicYear>.Failure("Academic year should span approximately one year (±30 days)");

        return Result<AcademicYear>.Success(new AcademicYear(0, yearLabel, startDate, endDate));
    }

    /// <summary>
    /// Update academic year details with validation
    /// </summary>
    public Result Update(string yearLabel, DateTime startDate, DateTime endDate)
    {
        // Validate year label
        if (string.IsNullOrWhiteSpace(yearLabel))
            return Result.Failure("Year label is required");

        if (yearLabel.Length < 2 || yearLabel.Length > 50)
            return Result.Failure("Year label must be between 2 and 50 characters");

        // Validate dates
        if (startDate >= endDate)
            return Result.Failure("Start date must be before end date");

        if (startDate < DateTime.Now.AddYears(-10) || endDate > DateTime.Now.AddYears(10))
            return Result.Failure("Academic year dates must be within reasonable range (±10 years)");

        // Year should typically span ~365 days (allow ±30 days)
        var daysDifference = (endDate - startDate).TotalDays;
        if (daysDifference < 335 || daysDifference > 395)
            return Result.Failure("Academic year should span approximately one year (±30 days)");

        YearLabel = yearLabel;
        StartDate = startDate;
        EndDate = endDate;

        return Result.Success();
    }

    /// <summary>
    /// Set academic year as active
    /// </summary>
    public Result Activate()
    {
        IsActive = true;
        return Result.Success();
    }

    /// <summary>
    /// Deactivate academic year
    /// </summary>
    public Result Deactivate()
    {
        IsActive = false;
        return Result.Success();
    }

    /// <summary>
    /// Check if academic year is currently active (within date range)
    /// </summary>
    public bool IsCurrent()
    {
        var now = DateTime.Now;
        return IsActive && now >= StartDate && now <= EndDate;
    }
}
