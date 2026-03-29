using DomainSemester = Plannify.Domain.Entities.Semester;

namespace Plannify.Application.Contracts;

/// <summary>
/// Repository abstraction for Semester entity
/// Extends generic repository with semester-specific operations
/// </summary>
public interface ISemesterRepository : IGenericRepository<DomainSemester>
{
    /// <summary>
    /// Get semester by number and academic year
    /// </summary>
    Task<DomainSemester?> GetByNumberAndYearAsync(int semesterNumber, int academicYearId);

    /// <summary>
    /// Get all semesters by academic year
    /// </summary>
    Task<IEnumerable<DomainSemester>> GetByAcademicYearAsync(int academicYearId);

    /// <summary>
    /// Get current/active semester
    /// </summary>
    Task<DomainSemester?> GetCurrentSemesterAsync();

    /// <summary>
    /// Check if semester exists for academic year
    /// </summary>
    Task<bool> ExistsForYearAsync(int semesterNumber, int academicYearId, int? excludeSemesterId = null);
}
