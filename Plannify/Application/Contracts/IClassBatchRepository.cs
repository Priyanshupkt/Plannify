using DomainClassBatch = Plannify.Domain.Entities.ClassBatch;

namespace Plannify.Application.Contracts;

/// <summary>
/// Repository abstraction for ClassBatch entity
/// Extends generic repository with class-specific operations
/// </summary>
public interface IClassBatchRepository : IGenericRepository<DomainClassBatch>
{
    /// <summary>
    /// Get all classes for a specific department
    /// </summary>
    Task<IEnumerable<DomainClassBatch>> GetByDepartmentAsync(int departmentId);

    /// <summary>
    /// Get all classes for a specific academic year
    /// </summary>
    Task<IEnumerable<DomainClassBatch>> GetByAcademicYearAsync(int academicYearId);

    /// <summary>
    /// Get all classes for a specific department and semester
    /// </summary>
    Task<IEnumerable<DomainClassBatch>> GetByDepartmentAndSemesterAsync(int departmentId, int semester);

    /// <summary>
    /// Get all classes assigned to a specific room
    /// </summary>
    Task<IEnumerable<DomainClassBatch>> GetByRoomAsync(int roomId);

    /// <summary>
    /// Check if batch name exists for department in academic year
    /// </summary>
    Task<bool> BatchNameExistsAsync(string batchName, int departmentId, int academicYearId, int? excludeClassId = null);
}
