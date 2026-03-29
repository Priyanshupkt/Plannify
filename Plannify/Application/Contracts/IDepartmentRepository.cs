using DomainDepartment = Plannify.Domain.Entities.Department;

namespace Plannify.Application.Contracts;

/// <summary>
/// Repository abstraction for Department entity
/// Extends generic repository with department-specific operations
/// </summary>
public interface IDepartmentRepository : IGenericRepository<DomainDepartment>
{
    /// <summary>
    /// Get department by code (unique constraint)
    /// </summary>
    Task<DomainDepartment?> GetByCodeAsync(string code);

    /// <summary>
    /// Check if department code already exists
    /// </summary>
    Task<bool> CodeExistsAsync(string code, int? excludeDepartmentId = null);

    /// <summary>
    /// Get count of teachers in department
    /// </summary>
    Task<int> GetTeacherCountAsync(int departmentId);

    /// <summary>
    /// Get count of subjects in department
    /// </summary>
    Task<int> GetSubjectCountAsync(int departmentId);

    /// <summary>
    /// Get count of classes in department
    /// </summary>
    Task<int> GetClassCountAsync(int departmentId);
}
