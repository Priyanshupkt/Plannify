using Plannify.Application.Common;
using Plannify.Application.DTOs;

namespace Plannify.Application.Contracts;

/// <summary>
/// Service interface for TimetableSlot operations
/// All timetable slot business logic is delegated through this abstraction
/// </summary>
public interface ITimetableSlotService
{
    /// <summary>
    /// Get timetable slot by ID
    /// </summary>
    Task<Result<TimetableSlotDto>> GetByIdAsync(int id);

    /// <summary>
    /// Get all timetable slots
    /// </summary>
    Task<Result<IEnumerable<TimetableSlotDto>>> GetAllAsync();

    /// <summary>
    /// Get slots by semester
    /// </summary>
    Task<Result<IEnumerable<TimetableSlotDto>>> GetBySemesterAsync(int semesterId);

    /// <summary>
    /// Get slots by class batch
    /// </summary>
    Task<Result<IEnumerable<TimetableSlotDto>>> GetByClassBatchAsync(int classBatchId);

    /// <summary>
    /// Get slots by teacher
    /// </summary>
    Task<Result<IEnumerable<TimetableSlotDto>>> GetByTeacherAsync(int teacherId);

    /// <summary>
    /// Get slots by room
    /// </summary>
    Task<Result<IEnumerable<TimetableSlotDto>>> GetByRoomAsync(int roomId);

    /// <summary>
    /// Get slots by subject
    /// </summary>
    Task<Result<IEnumerable<TimetableSlotDto>>> GetBySubjectAsync(int subjectId);

    /// <summary>
    /// Get class timetable for specific day
    /// </summary>
    Task<Result<IEnumerable<TimetableSlotDto>>> GetClassTimetableByDayAsync(int classBatchId, string day);

    /// <summary>
    /// Get teacher timetable for specific day
    /// </summary>
    Task<Result<IEnumerable<TimetableSlotDto>>> GetTeacherTimetableByDayAsync(int teacherId, string day);

    /// <summary>
    /// Get room schedule for specific day
    /// </summary>
    Task<Result<IEnumerable<TimetableSlotDto>>> GetRoomScheduleByDayAsync(int roomId, string day);

    /// <summary>
    /// Get lab sessions for class batch
    /// </summary>
    Task<Result<IEnumerable<TimetableSlotDto>>> GetLabSessionsByClassAsync(int classBatchId);

    /// <summary>
    /// Get timetable slot summary list
    /// </summary>
    Task<Result<IEnumerable<TimetableSlotSummaryDto>>> GetSummaryListAsync();

    /// <summary>
    /// Create new timetable slot
    /// </summary>
    Task<Result<int>> CreateAsync(CreateTimetableSlotRequest request);

    /// <summary>
    /// Update existing timetable slot
    /// </summary>
    Task<Result> UpdateAsync(UpdateTimetableSlotRequest request);

    /// <summary>
    /// Delete timetable slot
    /// </summary>
    Task<Result> DeleteAsync(int id);

    /// <summary>
    /// Assign teacher to slot (with conflict detection)
    /// </summary>
    Task<Result> AssignTeacherAsync(int slotId, int teacherId);

    /// <summary>
    /// Remove teacher assignment
    /// </summary>
    Task<Result> RemoveTeacherAsync(int slotId);

    /// <summary>
    /// Assign subject to slot
    /// </summary>
    Task<Result> AssignSubjectAsync(int slotId, int subjectId);

    /// <summary>
    /// Remove subject assignment
    /// </summary>
    Task<Result> RemoveSubjectAsync(int slotId);

    /// <summary>
    /// Assign room to slot (with conflict detection)
    /// </summary>
    Task<Result> AssignRoomAsync(int slotId, int roomId);

    /// <summary>
    /// Remove room assignment
    /// </summary>
    Task<Result> RemoveRoomAsync(int slotId);
}
