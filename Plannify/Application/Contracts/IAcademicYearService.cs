using Plannify.Application.Common;
using Plannify.Application.DTOs;

namespace Plannify.Application.Contracts;

/// <summary>
/// Service interface for AcademicYear operations
/// All academic year business logic is delegated through this abstraction
/// </summary>
public interface IAcademicYearService
{
    /// <summary>
    /// Get academic year by ID
    /// </summary>
    Task<Result<AcademicYearDto>> GetByIdAsync(int id);

    /// <summary>
    /// Get all academic years
    /// </summary>
    Task<Result<IEnumerable<AcademicYearDto>>> GetAllAsync();

    /// <summary>
    /// Get current active academic year
    /// </summary>
    Task<Result<AcademicYearDto>> GetCurrentAsync();

    /// <summary>
    /// Get all active academic years
    /// </summary>
    Task<Result<IEnumerable<AcademicYearDto>>> GetAllActiveAsync();

    /// <summary>
    /// Get academic year summary list
    /// </summary>
    Task<Result<IEnumerable<AcademicYearSummaryDto>>> GetSummaryListAsync();

    /// <summary>
    /// Create new academic year
    /// </summary>
    Task<Result<int>> CreateAsync(CreateAcademicYearRequest request);

    /// <summary>
    /// Update existing academic year
    /// </summary>
    Task<Result> UpdateAsync(UpdateAcademicYearRequest request);

    /// <summary>
    /// Delete academic year
    /// </summary>
    Task<Result> DeleteAsync(int id);

    /// <summary>
    /// Activate academic year
    /// </summary>
    Task<Result> ActivateAsync(int id);

    /// <summary>
    /// Deactivate academic year
    /// </summary>
    Task<Result> DeactivateAsync(int id);
}
