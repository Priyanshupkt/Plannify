using Plannify.Application.Common;
using Plannify.Application.DTOs;

namespace Plannify.Application.Contracts;

/// <summary>
/// Service contract for Timetable business logic orchestration
/// </summary>
public interface ITimetableService
{
    /// <summary>
    /// Get timetable by ID
    /// </summary>
    Task<Result<TimetableDto>> GetByIdAsync(int id);

    /// <summary>
    /// Get all timetables
    /// </summary>
    Task<Result<IEnumerable<TimetableSummaryDto>>> GetAllAsync();

    /// <summary>
    /// Get timetables by semester
    /// </summary>
    Task<Result<IEnumerable<TimetableSummaryDto>>> GetBySemesterAsync(int semesterId);

    /// <summary>
    /// Get finalized timetables for a semester
    /// </summary>
    Task<Result<IEnumerable<TimetableSummaryDto>>> GetFinalizedBySemesterAsync(int semesterId);

    /// <summary>
    /// Get active (non-finalized) timetables for a semester
    /// </summary>
    Task<Result<IEnumerable<TimetableSummaryDto>>> GetActiveBySemesterAsync(int semesterId);

    /// <summary>
    /// Create a new timetable
    /// </summary>
    Task<Result<TimetableDto>> CreateAsync(CreateTimetableRequest request);

    /// <summary>
    /// Update a timetable
    /// </summary>
    Task<Result<TimetableDto>> UpdateAsync(UpdateTimetableRequest request);

    /// <summary>
    /// Delete a timetable
    /// </summary>
    Task<Result<string>> DeleteAsync(int id);

    /// <summary>
    /// Finalize a timetable (lock against modifications)
    /// </summary>
    Task<Result<string>> FinalizeAsync(int id);

    /// <summary>
    /// Unfinalize a timetable (allow modifications)
    /// </summary>
    Task<Result<string>> UnfinalizeAsync(int id);

    /// <summary>
    /// Get timetable statistics
    /// </summary>
    Task<Result<TimetableStatisticsDto>> GetStatisticsAsync(int id);

    /// <summary>
    /// Get slots by class batch for a timetable
    /// </summary>
    Task<Result<IEnumerable<TimetableSlotSummaryDto>>> GetClassBatchSlotsAsync(int timetableId, int classBatchId);

    /// <summary>
    /// Get slots by teacher for a timetable
    /// </summary>
    Task<Result<IEnumerable<TimetableSlotSummaryDto>>> GetTeacherSlotsAsync(int timetableId, int teacherId);

    /// <summary>
    /// Get slots by room for a timetable
    /// </summary>
    Task<Result<IEnumerable<TimetableSlotSummaryDto>>> GetRoomSlotsAsync(int timetableId, int roomId);

    /// <summary>
    /// Get slots by day for a timetable
    /// </summary>
    Task<Result<IEnumerable<TimetableSlotSummaryDto>>> GetDaySlotsAsync(int timetableId, string day);

    /// <summary>
    /// Check if timetable name exists for a semester
    /// </summary>
    Task<bool> TimetableNameExistsAsync(string name, int semesterId, int? excludeId = null);

    /// <summary>
    /// Get full timetable with all slots and relationships
    /// </summary>
    Task<Result<TimetableDto>> GetFullTimetableAsync(int id);
}
