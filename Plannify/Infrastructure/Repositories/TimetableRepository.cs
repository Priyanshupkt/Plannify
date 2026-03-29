using Microsoft.EntityFrameworkCore;
using Plannify.Application.Contracts;
using Plannify.Data;
using Plannify.Domain.Entities;

namespace Plannify.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Timetable aggregate root
/// </summary>
public class TimetableRepository : GenericRepository<Timetable>, ITimetableRepository
{
    public TimetableRepository(AppDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get timetable by semester ID
    /// </summary>
    public async Task<IEnumerable<Timetable>> GetBySemesterAsync(int semesterId)
    {
        return await _context.Set<Timetable>()
            .Where(t => t.SemesterId == semesterId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Get a specific timetable with all its slots loaded
    /// </summary>
    public async Task<Timetable?> GetWithSlotsAsync(int timetableId)
    {
        return await _context.Set<Timetable>()
            .Include(t => t.TimetableSlots)
            .FirstOrDefaultAsync(t => t.Id == timetableId);
    }

    /// <summary>
    /// Check if a timetable with given name exists for a semester
    /// </summary>
    public async Task<bool> TimetableNameExistsAsync(string name, int semesterId, int? excludeId = null)
    {
        var query = _context.Set<Timetable>()
            .Where(t => t.SemesterId == semesterId && t.Name == name);

        if (excludeId.HasValue)
            query = query.Where(t => t.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    /// <summary>
    /// Get all finalized timetables for a semester
    /// </summary>
    public async Task<IEnumerable<Timetable>> GetFinalizedBySemesterAsync(int semesterId)
    {
        return await _context.Set<Timetable>()
            .Where(t => t.SemesterId == semesterId && t.IsFinalized)
            .OrderByDescending(t => t.FinalizedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Get all active (non-finalized) timetables for a semester
    /// </summary>
    public async Task<IEnumerable<Timetable>> GetActiveBySemesterAsync(int semesterId)
    {
        return await _context.Set<Timetable>()
            .Where(t => t.SemesterId == semesterId && !t.IsFinalized)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Get timetable by ID with all slots including navigation properties
    /// </summary>
    public async Task<Timetable?> GetWithSlotsAndNavigationAsync(int timetableId)
    {
        return await _context.Set<Timetable>()
            .Include(t => t.TimetableSlots)
            .FirstOrDefaultAsync(t => t.Id == timetableId);
    }

    /// <summary>
    /// Get timetables created within a date range
    /// </summary>
    public async Task<IEnumerable<Timetable>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Set<Timetable>()
            .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Get count of timetables for a semester
    /// </summary>
    public async Task<int> GetCountBySemesterAsync(int semesterId)
    {
        return await _context.Set<Timetable>()
            .Where(t => t.SemesterId == semesterId)
            .CountAsync();
    }
}
