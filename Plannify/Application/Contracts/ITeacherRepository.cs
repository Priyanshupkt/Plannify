using Plannify.Domain.Entities;

namespace Plannify.Application.Contracts;

/// <summary>
/// Repository abstraction for Teacher entity
/// Extends generic repository with teacher-specific operations
/// </summary>
public interface ITeacherRepository : IGenericRepository<Teacher>
{
    /// <summary>
    /// Get teacher by employee code (unique constraint)
    /// </summary>
    Task<Teacher?> GetByEmployeeCodeAsync(string employeeCode);

    /// <summary>
    /// Get all teachers in a department with their workload
    /// </summary>
    Task<IEnumerable<Teacher>> GetByDepartmentAsync(int departmentId);

    /// <summary>
    /// Get all active teachers
    /// </summary>
    Task<IEnumerable<Teacher>> GetActiveAsync();

    /// <summary>
    /// Get teacher with all their timetable slots for a semester
    /// </summary>
    Task<Teacher?> GetWithTimetableSlotsAsync(int teacherId, int semesterId);

    /// <summary>
    /// Check if employee code already exists
    /// </summary>
    Task<bool> EmployeeCodeExistsAsync(string employeeCode, int? excludeTeacherId = null);

    /// <summary>
    /// Get total hours allocated to a teacher in a semester
    /// </summary>
    Task<decimal> GetAllocatedHoursAsync(int teacherId, int semesterId);
}
