using Plannify.Application.Common;
using Plannify.Application.DTOs;

namespace Plannify.Application.Contracts;

/// <summary>
/// Service contract for substitution business logic
/// </summary>
public interface ISubstitutionService
{
    /// <summary>
    /// Get substitution by ID
    /// </summary>
    Task<Result<SubstitutionDto>> GetByIdAsync(int id);

    /// <summary>
    /// Get all substitutions
    /// </summary>
    Task<Result<List<SubstitutionDto>>> GetAllAsync();

    /// <summary>
    /// Get substitutions for a specific teacher
    /// </summary>
    Task<Result<List<SubstitutionSummaryDto>>> GetByTeacherAsync(int teacherId);

    /// <summary>
    /// Get substitutions by date
    /// </summary>
    Task<Result<List<SubstitutionSummaryDto>>> GetByDateAsync(DateOnly date);

    /// <summary>
    /// Get substitutions within a date range
    /// </summary>
    Task<Result<List<SubstitutionSummaryDto>>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate);

    /// <summary>
    /// Create a new substitution
    /// </summary>
    Task<Result<SubstitutionDto>> CreateAsync(CreateSubstitutionRequest request);

    /// <summary>
    /// Update substitution details
    /// </summary>
    Task<Result<SubstitutionDto>> UpdateAsync(int id, UpdateSubstitutionRequest request);

    /// <summary>
    /// Change the substitute teacher
    /// </summary>
    Task<Result<SubstitutionDto>> ChangeSubstituteAsync(int id, ChangeSubstituteRequest request);

    /// <summary>
    /// Delete a substitution
    /// </summary>
    Task<Result<bool>> DeleteAsync(int id);

    /// <summary>
    /// Get active substitutions (today and future)
    /// </summary>
    Task<Result<List<SubstitutionSummaryDto>>> GetActiveAsync();

    /// <summary>
    /// Get urgent substitutions (recent and happening soon)
    /// </summary>
    Task<Result<List<SubstitutionSummaryDto>>> GetUrgentAsync();

    /// <summary>
    /// Check if a substitution exists for a slot on a date
    /// </summary>
    Task<Result<bool>> ExistsBySlotAndDateAsync(int slotId, DateOnly date);

    /// <summary>
    /// Check if a teacher has substitution on a date
    /// </summary>
    Task<Result<bool>> TeacherHasSubstitutionOnDateAsync(int teacherId, DateOnly date);

    /// <summary>
    /// Get substitutions for a specific timetable slot
    /// </summary>
    Task<Result<List<SubstitutionSummaryDto>>> GetByTimetableSlotAsync(int slotId);

    /// <summary>
    /// Get count of all substitutions
    /// </summary>
    Task<Result<int>> GetCountAsync();

    /// <summary>
    /// Get count of substitutions for a teacher
    /// </summary>
    Task<Result<int>> GetCountByTeacherAsync(int teacherId);

    /// <summary>
    /// Get substitutions where teacher is the original (absent) teacher
    /// </summary>
    Task<Result<List<SubstitutionSummaryDto>>> GetByOriginalTeacherAsync(int teacherId);

    /// <summary>
    /// Get substitutions where teacher is the substitute
    /// </summary>
    Task<Result<List<SubstitutionSummaryDto>>> GetBySubstituteTeacherAsync(int teacherId);
}
