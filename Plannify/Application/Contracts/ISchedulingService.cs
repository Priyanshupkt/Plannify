using Plannify.Services;

namespace Plannify.Application.Contracts;

/// <summary>
/// Interface for scheduling service
/// Generates optimized timetables using constraint satisfaction
/// </summary>
public interface ISchedulingService
{
    /// <summary>
    /// Generate an optimized timetable for the specified criteria
    /// </summary>
    Task<SchedulingResult> GenerateTimetableAsync(SchedulingRequest request);
}
