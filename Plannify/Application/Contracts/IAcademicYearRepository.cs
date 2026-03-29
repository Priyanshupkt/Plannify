using DomainAcademicYear = Plannify.Domain.Entities.AcademicYear;

namespace Plannify.Application.Contracts;

/// <summary>
/// Repository abstraction for AcademicYear entity
/// Extends generic repository with academic year-specific operations
/// </summary>
public interface IAcademicYearRepository : IGenericRepository<DomainAcademicYear>
{
    /// <summary>
    /// Get academic year by label
    /// </summary>
    Task<DomainAcademicYear?> GetByYearLabelAsync(string yearLabel);

    /// <summary>
    /// Get current active academic year
    /// </summary>
    Task<DomainAcademicYear?> GetCurrentAcademicYearAsync();

    /// <summary>
    /// Check if year label exists
    /// </summary>
    Task<bool> YearLabelExistsAsync(string yearLabel, int? excludeYearId = null);

    /// <summary>
    /// Get all active academic years
    /// </summary>
    Task<IEnumerable<DomainAcademicYear>> GetAllActiveAsync();
}
