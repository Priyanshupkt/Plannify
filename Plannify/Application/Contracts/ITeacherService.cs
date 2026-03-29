using Plannify.Application.Common;
using Plannify.Application.DTOs;

namespace Plannify.Application.Contracts;

/// <summary>
/// Application service interface for Teacher management
/// Defines all teacher-related business operations
/// </summary>
public interface ITeacherService
{
    /// <summary>
    /// Get teacher by ID with full details
    /// </summary>
    Task<Result<TeacherDto>> GetByIdAsync(int id);

    /// <summary>
    /// Get all teachers
    /// </summary>
    Task<Result<IEnumerable<TeacherDto>>> GetAllAsync();

    /// <summary>
    /// Get all active teachers
    /// </summary>
    Task<Result<IEnumerable<TeacherDto>>> GetActiveAsync();

    /// <summary>
    /// Get teachers by department
    /// </summary>
    Task<Result<IEnumerable<TeacherDto>>> GetByDepartmentAsync(int departmentId);

    /// <summary>
    /// Get summary (lightweight) list of teachers
    /// </summary>
    Task<Result<IEnumerable<TeacherSummaryDto>>> GetSummaryListAsync();

    /// <summary>
    /// Create a new teacher
    /// </summary>
    Task<Result<int>> CreateTeacherAsync(CreateTeacherRequest request);

    /// <summary>
    /// Update existing teacher
    /// </summary>
    Task<Result> UpdateTeacherAsync(UpdateTeacherRequest request);

    /// <summary>
    /// Delete teacher by ID
    /// </summary>
    Task<Result> DeleteTeacherAsync(int id);

    /// <summary>
    /// Deactivate teacher without deleting
    /// </summary>
    Task<Result> DeactivateTeacherAsync(int id);

    /// <summary>
    /// Check if teacher can accept more hours
    /// </summary>
    Task<Result<bool>> CanAcceptMoreHoursAsync(int teacherId, int semesterId, decimal proposedHours);

    /// <summary>
    /// Get teacher's allocated hours for a semester
    /// </summary>
    Task<Result<decimal>> GetAllocatedHoursAsync(int teacherId, int semesterId);
}
