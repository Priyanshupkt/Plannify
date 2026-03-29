using Plannify.Domain.Entities;

namespace Plannify.Application.Contracts;

/// <summary>
/// Repository contract for substitution records data access
/// </summary>
public interface ISubstitutionRepository
{
    /// <summary>
    /// Get substitution by ID
    /// </summary>
    Task<Substitution?> GetByIdAsync(int id);

    /// <summary>
    /// Get all substitutions
    /// </summary>
    Task<List<Substitution>> GetAllAsync();

    /// <summary>
    /// Get substitutions for a specific teacher (both original and substitute roles)
    /// </summary>
    Task<List<Substitution>> GetByTeacherAsync(int teacherId);

    /// <summary>
    /// Get substitutions where the teacher is the original (absent) teacher
    /// </summary>
    Task<List<Substitution>> GetByOriginalTeacherAsync(int teacherId);

    /// <summary>
    /// Get substitutions where the teacher is the substitute
    /// </summary>
    Task<List<Substitution>> GetBySubstituteTeacherAsync(int teacherId);

    /// <summary>
    /// Get substitutions for a specific timetable slot
    /// </summary>
    Task<List<Substitution>> GetByTimetableSlotAsync(int slotId);

    /// <summary>
    /// Get substitutions for a specific date
    /// </summary>
    Task<List<Substitution>> GetByDateAsync(DateOnly date);

    /// <summary>
    /// Get substitutions within a date range
    /// </summary>
    Task<List<Substitution>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate);

    /// <summary>
    /// Check if a substitution already exists for the given slot and date
    /// </summary>
    Task<bool> ExistsBySlotAndDateAsync(int slotId, DateOnly date);

    /// <summary>
    /// Check if a teacher has a substitution on a specific date
    /// </summary>
    Task<bool> TeacherHasSubstitutionOnDateAsync(int teacherId, DateOnly date);

    /// <summary>
    /// Get active substitutions (where the date is today or in the future)
    /// </summary>
    Task<List<Substitution>> GetActiveAsync();

    /// <summary>
    /// Get urgent substitutions (recent and happening soon)
    /// </summary>
    Task<List<Substitution>> GetUrgentAsync();

    /// <summary>
    /// Add a new substitution
    /// </summary>
    Task<int> AddAsync(Substitution substitution);

    /// <summary>
    /// Update an existing substitution
    /// </summary>
    Task UpdateAsync(Substitution substitution);

    /// <summary>
    /// Delete a substitution
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// Get count of substitutions
    /// </summary>
    Task<int> GetCountAsync();

    /// <summary>
    /// Get count of substitutions for a specific teacher
    /// </summary>
    Task<int> GetCountByTeacherAsync(int teacherId);
}
