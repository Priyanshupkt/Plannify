using Plannify.Application.Common;
using Plannify.Application.DTOs;

namespace Plannify.Application.Contracts;

/// <summary>
/// Service interface for Subject operations
/// All subject business logic is delegated through this abstraction
/// </summary>
public interface ISubjectService
{
    /// <summary>
    /// Get subject by ID
    /// </summary>
    Task<Result<SubjectDto>> GetByIdAsync(int id);

    /// <summary>
    /// Get all subjects
    /// </summary>
    Task<Result<IEnumerable<SubjectDto>>> GetAllAsync();

    /// <summary>
    /// Get subjects by department
    /// </summary>
    Task<Result<IEnumerable<SubjectDto>>> GetByDepartmentAsync(int departmentId);

    /// <summary>
    /// Get subjects by semester
    /// </summary>
    Task<Result<IEnumerable<SubjectDto>>> GetBySemesterAsync(int semesterNumber);

    /// <summary>
    /// Get subject summary list
    /// </summary>
    Task<Result<IEnumerable<SubjectSummaryDto>>> GetSummaryListAsync();

    /// <summary>
    /// Create new subject
    /// </summary>
    Task<Result<int>> CreateAsync(CreateSubjectRequest request);

    /// <summary>
    /// Update existing subject
    /// </summary>
    Task<Result> UpdateAsync(UpdateSubjectRequest request);

    /// <summary>
    /// Delete subject
    /// </summary>
    Task<Result> DeleteAsync(int id);
}
