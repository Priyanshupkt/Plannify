using DomainTimetableSlot = Plannify.Domain.Entities.TimetableSlot;

namespace Plannify.Application.Contracts;

/// <summary>
/// Repository abstraction for TimetableSlot entity
/// Extends generic repository with timetable slot-specific operations
/// </summary>
public interface ITimetableSlotRepository : IGenericRepository<DomainTimetableSlot>
{
    /// <summary>
    /// Get all slots for a specific semester
    /// </summary>
    Task<IEnumerable<DomainTimetableSlot>> GetBySemesterAsync(int semesterId);

    /// <summary>
    /// Get all slots for a specific class batch
    /// </summary>
    Task<IEnumerable<DomainTimetableSlot>> GetByClassBatchAsync(int classBatchId);

    /// <summary>
    /// Get all slots for a specific teacher
    /// </summary>
    Task<IEnumerable<DomainTimetableSlot>> GetByTeacherAsync(int teacherId);

    /// <summary>
    /// Get all slots for a specific room
    /// </summary>
    Task<IEnumerable<DomainTimetableSlot>> GetByRoomAsync(int roomId);

    /// <summary>
    /// Get all slots for a specific subject
    /// </summary>
    Task<IEnumerable<DomainTimetableSlot>> GetBySubjectAsync(int subjectId);

    /// <summary>
    /// Get slots by day and semester
    /// </summary>
    Task<IEnumerable<DomainTimetableSlot>> GetByDayAndSemesterAsync(string day, int semesterId);

    /// <summary>
    /// Get slots for class batch on specific day
    /// </summary>
    Task<IEnumerable<DomainTimetableSlot>> GetByClassBatchAndDayAsync(int classBatchId, string day);

    /// <summary>
    /// Get slots for teacher on specific day
    /// </summary>
    Task<IEnumerable<DomainTimetableSlot>> GetByTeacherAndDayAsync(int teacherId, string day);

    /// <summary>
    /// Get slots for room on specific day
    /// </summary>
    Task<IEnumerable<DomainTimetableSlot>> GetByRoomAndDayAsync(int roomId, string day);

    /// <summary>
    /// Get all lab sessions for a class batch
    /// </summary>
    Task<IEnumerable<DomainTimetableSlot>> GetLabSessionsByClassBatchAsync(int classBatchId);

    /// <summary>
    /// Get all lab sessions for a specific lab group
    /// </summary>
    Task<IEnumerable<DomainTimetableSlot>> GetLabSessionsByGroupAsync(string labGroupTag);

    /// <summary>
    /// Check if room has slot at given time on given day
    /// </summary>
    Task<bool> IsRoomOccupiedAsync(int roomId, string day, TimeOnly startTime, TimeOnly endTime, int? excludeSlotId = null);

    /// <summary>
    /// Check if teacher has slot at given time on given day
    /// </summary>
    Task<bool> IsTeacherOccupiedAsync(int teacherId, string day, TimeOnly startTime, TimeOnly endTime, int? excludeSlotId = null);

    /// <summary>
    /// Check if class batch has slot at given time on given day
    /// </summary>
    Task<bool> IsClassOccupiedAsync(int classBatchId, string day, TimeOnly startTime, TimeOnly endTime, int? excludeSlotId = null);
}
