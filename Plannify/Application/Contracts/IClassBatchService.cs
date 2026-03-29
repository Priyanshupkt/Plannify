using Plannify.Application.Common;
using Plannify.Application.DTOs;

namespace Plannify.Application.Contracts;

/// <summary>
/// Service interface for ClassBatch operations
/// All class batch business logic is delegated through this abstraction
/// </summary>
public interface IClassBatchService
{
    /// <summary>
    /// Get class batch by ID
    /// </summary>
    Task<Result<ClassBatchDto>> GetByIdAsync(int id);

    /// <summary>
    /// Get all class batches
    /// </summary>
    Task<Result<IEnumerable<ClassBatchDto>>> GetAllAsync();

    /// <summary>
    /// Get class batches by department
    /// </summary>
    Task<Result<IEnumerable<ClassBatchDto>>> GetByDepartmentAsync(int departmentId);

    /// <summary>
    /// Get class batches by academic year
    /// </summary>
    Task<Result<IEnumerable<ClassBatchDto>>> GetByAcademicYearAsync(int academicYearId);

    /// <summary>
    /// Get class batches by department and semester
    /// </summary>
    Task<Result<IEnumerable<ClassBatchDto>>> GetByDepartmentAndSemesterAsync(int departmentId, int semester);

    /// <summary>
    /// Get class batches assigned to a room
    /// </summary>
    Task<Result<IEnumerable<ClassBatchDto>>> GetByRoomAsync(int roomId);

    /// <summary>
    /// Get class batch summary list
    /// </summary>
    Task<Result<IEnumerable<ClassBatchSummaryDto>>> GetSummaryListAsync();

    /// <summary>
    /// Create new class batch
    /// </summary>
    Task<Result<int>> CreateAsync(CreateClassBatchRequest request);

    /// <summary>
    /// Update existing class batch
    /// </summary>
    Task<Result> UpdateAsync(UpdateClassBatchRequest request);

    /// <summary>
    /// Delete class batch
    /// </summary>
    Task<Result> DeleteAsync(int id);

    /// <summary>
    /// Assign room to class batch
    /// </summary>
    Task<Result> AssignRoomAsync(int classId, int roomId);

    /// <summary>
    /// Remove room assignment from class batch
    /// </summary>
    Task<Result> RemoveRoomAsync(int classId);
}
