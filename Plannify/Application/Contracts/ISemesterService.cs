using Plannify.Application.Common;
using Plannify.Application.DTOs;

namespace Plannify.Application.Contracts;

/// <summary>
/// Service interface for Semester operations
/// All semester business logic is delegated through this abstraction
/// </summary>
public interface ISemesterService
{
    /// <summary>
    /// Get semester by ID
    /// </summary>
    Task<Result<SemesterDto>> GetByIdAsync(int id);

    /// <summary>
    /// Get all semesters
    /// </summary>
    Task<Result<IEnumerable<SemesterDto>>> GetAllAsync();

    /// <summary>
    /// Get semesters by academic year
    /// </summary>
    Task<Result<IEnumerable<SemesterDto>>> GetByAcademicYearAsync(int academicYearId);

    /// <summary>
    /// Get current active semester
    /// </summary>
    Task<Result<SemesterDto>> GetCurrentSemesterAsync();

    /// <summary>
    /// Get semester summary list
    /// </summary>
    Task<Result<IEnumerable<SemesterSummaryDto>>> GetSummaryListAsync();

    /// <summary>
    /// Create new semester
    /// </summary>
    Task<Result<int>> CreateAsync(CreateSemesterRequest request);

    /// <summary>
    /// Update existing semester
    /// </summary>
    Task<Result> UpdateAsync(UpdateSemesterRequest request);

    /// <summary>
    /// Delete semester
    /// </summary>
    Task<Result> DeleteAsync(int id);
}
