using DomainSubject = Plannify.Domain.Entities.Subject;

namespace Plannify.Application.Contracts;

/// <summary>
/// Repository abstraction for Subject entity
/// Extends generic repository with subject-specific operations
/// </summary>
public interface ISubjectRepository : IGenericRepository<DomainSubject>
{
    /// <summary>
    /// Get subject by code and department (unique constraint)
    /// </summary>
    Task<DomainSubject?> GetByCodeAndDepartmentAsync(string code, int departmentId);

    /// <summary>
    /// Check if subject code already exists in department
    /// </summary>
    Task<bool> CodeExistsInDepartmentAsync(string code, int departmentId, int? excludeSubjectId = null);

    /// <summary>
    /// Get subjects by department ID
    /// </summary>
    Task<IEnumerable<DomainSubject>> GetByDepartmentAsync(int departmentId);

    /// <summary>
    /// Get subjects by semester
    /// </summary>
    Task<IEnumerable<DomainSubject>> GetBySemesterAsync(int semesterNumber);

    /// <summary>
    /// Get subjects by department and semester
    /// </summary>
    Task<IEnumerable<DomainSubject>> GetByDepartmentAndSemesterAsync(int departmentId, int semesterNumber);
}
