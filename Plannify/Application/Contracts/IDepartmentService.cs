using Plannify.Application.Common;
using Plannify.Application.DTOs;

namespace Plannify.Application.Contracts;

/// <summary>
/// Department service interface
/// Defines all department-related business operations
/// </summary>
public interface IDepartmentService
{
    /// <summary>
    /// Get department by ID
    /// </summary>
    Task<Result<DepartmentDto>> GetByIdAsync(int id);

    /// <summary>
    /// Get all departments with counts
    /// </summary>
    Task<Result<IEnumerable<DepartmentDto>>> GetAllAsync();

    /// <summary>
    /// Get summary list of departments (for dropdowns)
    /// </summary>
    Task<Result<IEnumerable<DepartmentSummaryDto>>> GetSummaryListAsync();

    /// <summary>
    /// Create a new department
    /// </summary>
    Task<Result<int>> CreateAsync(CreateDepartmentRequest request);

    /// <summary>
    /// Update existing department
    /// </summary>
    Task<Result> UpdateAsync(UpdateDepartmentRequest request);

    /// <summary>
    /// Delete department by ID
    /// </summary>
    Task<Result> DeleteAsync(int id);

    /// <summary>
    /// Check if department can be deleted (no dependencies)
    /// </summary>
    Task<Result<bool>> CanDeleteAsync(int id);
}

