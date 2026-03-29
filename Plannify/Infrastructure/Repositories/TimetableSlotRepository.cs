using Microsoft.EntityFrameworkCore;
using Plannify.Application.Contracts;
using Plannify.Data;
using DomainTimetableSlot = Plannify.Domain.Entities.TimetableSlot;

namespace Plannify.Infrastructure.Repositories;

/// <summary>
/// TimetableSlot repository implementation
/// Handles all data access for TimetableSlot entity
/// </summary>
public class TimetableSlotRepository : GenericRepository<DomainTimetableSlot>, ITimetableSlotRepository
{
    public TimetableSlotRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<DomainTimetableSlot>> GetBySemesterAsync(int semesterId)
        => await _dbSet
            .Where(t => t.SemesterId == semesterId)
            .OrderBy(t => t.Day)
            .ThenBy(t => t.StartTime)
            .ToListAsync();

    public async Task<IEnumerable<DomainTimetableSlot>> GetByClassBatchAsync(int classBatchId)
        => await _dbSet
            .Where(t => t.ClassBatchId == classBatchId)
            .OrderBy(t => t.Day)
            .ThenBy(t => t.StartTime)
            .ToListAsync();

    public async Task<IEnumerable<DomainTimetableSlot>> GetByTeacherAsync(int teacherId)
        => await _dbSet
            .Where(t => t.TeacherId == teacherId)
            .OrderBy(t => t.Day)
            .ThenBy(t => t.StartTime)
            .ToListAsync();

    public async Task<IEnumerable<DomainTimetableSlot>> GetByRoomAsync(int roomId)
        => await _dbSet
            .Where(t => t.RoomId == roomId)
            .OrderBy(t => t.Day)
            .ThenBy(t => t.StartTime)
            .ToListAsync();

    public async Task<IEnumerable<DomainTimetableSlot>> GetBySubjectAsync(int subjectId)
        => await _dbSet
            .Where(t => t.SubjectId == subjectId)
            .OrderBy(t => t.Day)
            .ThenBy(t => t.StartTime)
            .ToListAsync();

    public async Task<IEnumerable<DomainTimetableSlot>> GetByDayAndSemesterAsync(string day, int semesterId)
        => await _dbSet
            .Where(t => t.Day == day && t.SemesterId == semesterId)
            .OrderBy(t => t.StartTime)
            .ToListAsync();

    public async Task<IEnumerable<DomainTimetableSlot>> GetByClassBatchAndDayAsync(int classBatchId, string day)
        => await _dbSet
            .Where(t => t.ClassBatchId == classBatchId && t.Day == day)
            .OrderBy(t => t.StartTime)
            .ToListAsync();

    public async Task<IEnumerable<DomainTimetableSlot>> GetByTeacherAndDayAsync(int teacherId, string day)
        => await _dbSet
            .Where(t => t.TeacherId == teacherId && t.Day == day)
            .OrderBy(t => t.StartTime)
            .ToListAsync();

    public async Task<IEnumerable<DomainTimetableSlot>> GetByRoomAndDayAsync(int roomId, string day)
        => await _dbSet
            .Where(t => t.RoomId == roomId && t.Day == day)
            .OrderBy(t => t.StartTime)
            .ToListAsync();

    public async Task<IEnumerable<DomainTimetableSlot>> GetLabSessionsByClassBatchAsync(int classBatchId)
        => await _dbSet
            .Where(t => t.ClassBatchId == classBatchId && t.IsLabSession)
            .OrderBy(t => t.Day)
            .ThenBy(t => t.StartTime)
            .ToListAsync();

    public async Task<IEnumerable<DomainTimetableSlot>> GetLabSessionsByGroupAsync(string labGroupTag)
        => await _dbSet
            .Where(t => t.LabGroupTag == labGroupTag && t.IsLabSession)
            .OrderBy(t => t.Day)
            .ThenBy(t => t.StartTime)
            .ToListAsync();

    public async Task<bool> IsRoomOccupiedAsync(int roomId, string day, TimeOnly startTime, TimeOnly endTime, int? excludeSlotId = null)
    {
        var query = _dbSet.Where(t => 
            t.RoomId == roomId && 
            t.Day == day && 
            t.StartTime < endTime && 
            t.EndTime > startTime);

        if (excludeSlotId.HasValue)
            query = query.Where(t => t.Id != excludeSlotId.Value);

        return await query.AnyAsync();
    }

    public async Task<bool> IsTeacherOccupiedAsync(int teacherId, string day, TimeOnly startTime, TimeOnly endTime, int? excludeSlotId = null)
    {
        var query = _dbSet.Where(t => 
            t.TeacherId == teacherId && 
            t.Day == day && 
            t.StartTime < endTime && 
            t.EndTime > startTime);

        if (excludeSlotId.HasValue)
            query = query.Where(t => t.Id != excludeSlotId.Value);

        return await query.AnyAsync();
    }

    public async Task<bool> IsClassOccupiedAsync(int classBatchId, string day, TimeOnly startTime, TimeOnly endTime, int? excludeSlotId = null)
    {
        var query = _dbSet.Where(t => 
            t.ClassBatchId == classBatchId && 
            t.Day == day && 
            t.StartTime < endTime && 
            t.EndTime > startTime);

        if (excludeSlotId.HasValue)
            query = query.Where(t => t.Id != excludeSlotId.Value);

        return await query.AnyAsync();
    }
}
