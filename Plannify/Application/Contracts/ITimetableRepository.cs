using Plannify.Domain.Entities;

namespace Plannify.Application.Contracts;

/// <summary>
/// Repository contract for Timetable aggregate root operations
/// </summary>
public interface ITimetableRepository : IGenericRepository<Timetable>
{
    /// <summary>
    /// Get timetable by semester ID
    /// </summary>
    Task<IEnumerable<Timetable>> GetBySemesterAsync(int semesterId);

    /// <summary>
    /// Get a specific timetable with all its slots loaded
    /// </summary>
    Task<Timetable?> GetWithSlotsAsync(int timetableId);

    /// <summary>
    /// Check if a timetable with given name exists for a semester
    /// </summary>
    Task<bool> TimetableNameExistsAsync(string name, int semesterId, int? excludeId = null);

    /// <summary>
    /// Get all finalized timetables for a semester
    /// </summary>
    Task<IEnumerable<Timetable>> GetFinalizedBySemesterAsync(int semesterId);

    /// <summary>
    /// Get all active (non-finalized) timetables for a semester
    /// </summary>
    Task<IEnumerable<Timetable>> GetActiveBySemesterAsync(int semesterId);

    /// <summary>
    /// Get timetable by ID with all slots including navigation properties
    /// </summary>
    Task<Timetable?> GetWithSlotsAndNavigationAsync(int timetableId);

    /// <summary>
    /// Get timetables created within a date range
    /// </summary>
    Task<IEnumerable<Timetable>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get count of timetables for a semester
    /// </summary>
    Task<int> GetCountBySemesterAsync(int semesterId);
}
