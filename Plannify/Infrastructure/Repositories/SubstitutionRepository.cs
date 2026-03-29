using Microsoft.EntityFrameworkCore;
using Plannify.Application.Contracts;
using Plannify.Data;
using Plannify.Domain.Entities;

namespace Plannify.Infrastructure.Repositories;

/// <summary>
/// Implementation of ISubstitutionRepository for substitution data access
/// Maps between Substitution (EF Core model) and Substitution (domain entity)
/// </summary>
public class SubstitutionRepository : ISubstitutionRepository
{
    private readonly AppDbContext _context;

    public SubstitutionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Substitution?> GetByIdAsync(int id)
    {
        var record = await _context.Substitutions.FirstOrDefaultAsync(s => s.Id == id);
        return record != null ? record : null;
    }

    public async Task<List<Substitution>> GetAllAsync()
    {
        var records = await _context.Substitutions.ToListAsync();
        return records.ToList();
    }

    public async Task<List<Substitution>> GetByTeacherAsync(int teacherId)
    {
        var records = await _context.Substitutions
            .Where(s => s.OriginalTeacherId == teacherId || s.SubstituteTeacherId == teacherId)
            .ToListAsync();
        return records.ToList();
    }

    public async Task<List<Substitution>> GetByOriginalTeacherAsync(int teacherId)
    {
        var records = await _context.Substitutions
            .Where(s => s.OriginalTeacherId == teacherId)
            .ToListAsync();
        return records.ToList();
    }

    public async Task<List<Substitution>> GetBySubstituteTeacherAsync(int teacherId)
    {
        var records = await _context.Substitutions
            .Where(s => s.SubstituteTeacherId == teacherId)
            .ToListAsync();
        return records.ToList();
    }

    public async Task<List<Substitution>> GetByTimetableSlotAsync(int slotId)
    {
        var records = await _context.Substitutions
            .Where(s => s.TimetableSlotId == slotId)
            .ToListAsync();
        return records.ToList();
    }

    public async Task<List<Substitution>> GetByDateAsync(DateOnly date)
    {
        var records = await _context.Substitutions
            .Where(s => s.Date == date)
            .ToListAsync();
        return records.ToList();
    }

    public async Task<List<Substitution>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        var records = await _context.Substitutions
            .Where(s => s.Date >= startDate && s.Date <= endDate)
            .OrderBy(s => s.Date)
            .ToListAsync();
        return records.ToList();
    }

    public async Task<bool> ExistsBySlotAndDateAsync(int slotId, DateOnly date)
    {
        return await _context.Substitutions
            .AnyAsync(s => s.TimetableSlotId == slotId && s.Date == date);
    }

    public async Task<bool> TeacherHasSubstitutionOnDateAsync(int teacherId, DateOnly date)
    {
        return await _context.Substitutions
            .AnyAsync(s => (s.OriginalTeacherId == teacherId || s.SubstituteTeacherId == teacherId) 
                && s.Date == date);
    }

    public async Task<List<Substitution>> GetActiveAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var records = await _context.Substitutions
            .Where(s => s.Date >= today)
            .OrderBy(s => s.Date)
            .ToListAsync();
        return records.ToList();
    }

    public async Task<List<Substitution>> GetUrgentAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var oneWeekFromNow = today.AddDays(7);
        var oneDayAgo = DateTime.UtcNow.AddHours(-24);

        var records = await _context.Substitutions
            .Where(s => s.Date >= today && s.Date <= oneWeekFromNow && s.CreatedAt >= oneDayAgo)
            .OrderBy(s => s.Date)
            .ToListAsync();
        return records.ToList();
    }

    public async Task<int> AddAsync(Substitution substitution)
    {
        _context.Substitutions.Add(substitution);
        await _context.SaveChangesAsync();
        return substitution.Id;
    }

    public async Task UpdateAsync(Substitution substitution)
    {
        _context.Substitutions.Update(substitution);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var record = await _context.Substitutions.FirstOrDefaultAsync(s => s.Id == id);
        if (record != null)
        {
            _context.Substitutions.Remove(record);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.Substitutions.CountAsync();
    }

    public async Task<int> GetCountByTeacherAsync(int teacherId)
    {
        return await _context.Substitutions
            .Where(s => s.OriginalTeacherId == teacherId || s.SubstituteTeacherId == teacherId)
            .CountAsync();
    }
}

