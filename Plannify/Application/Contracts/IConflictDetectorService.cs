using Plannify.Services;

namespace Plannify.Application.Contracts;

/// <summary>
/// Interface for conflict detection service
/// Detects scheduling conflicts for teachers, rooms, and classes
/// </summary>
public interface IConflictDetectorService
{
    /// <summary>
    /// Checks for time conflicts with a specific teacher's schedule
    /// </summary>
    Task<ConflictResult> CheckTeacherConflictAsync(
        int teacherId, string day, TimeOnly startTime, TimeOnly endTime,
        int semesterId, int? excludeSlotId = null);

    /// <summary>
    /// Checks for time conflicts with a specific room's schedule
    /// </summary>
    Task<ConflictResult> CheckRoomConflictAsync(
        int roomId, string day, TimeOnly startTime, TimeOnly endTime,
        int semesterId, int? excludeSlotId = null);

    /// <summary>
    /// Checks for time conflicts with a specific class's schedule
    /// </summary>
    Task<ConflictResult> CheckClassConflictAsync(
        int classBatchId, string day, TimeOnly startTime, TimeOnly endTime,
        int semesterId, int? excludeSlotId = null);

    /// <summary>
    /// Finds ALL overlapping slots in a semester grouped by conflict type
    /// </summary>
    Task<List<ConflictReport>> GetAllConflictsAsync(int semesterId);

    /// <summary>
    /// Finds available time slots for a teacher on a specific day
    /// </summary>
    Task<List<(TimeOnly Start, TimeOnly End)>> GetAvailableTeacherSlotsAsync(
        int teacherId, string day, int semesterId, int durationMinutes = 60);

    /// <summary>
    /// Finds available time slots for a room on a specific day
    /// </summary>
    Task<List<(TimeOnly Start, TimeOnly End)>> GetAvailableRoomSlotsAsync(
        int roomId, string day, int semesterId, int durationMinutes = 60);

    /// <summary>
    /// Finds available time slots for a class on a specific day
    /// </summary>
    Task<List<(TimeOnly Start, TimeOnly End)>> GetAvailableClassSlotsAsync(
        int classBatchId, string day, int semesterId, int durationMinutes = 60);

    /// <summary>
    /// Suggests alternative days/times if current slot conflicts
    /// Returns up to maxSuggestions with preferred day prioritized
    /// </summary>
    Task<List<TimeslotSuggestion>> SuggestAlternativeSlotsAsync(
        int teacherId, int? roomId, int classBatchId, TimeOnly requestedStart, TimeOnly requestedEnd,
        string preferredDay, int semesterId, int maxSuggestions = 3);
}
