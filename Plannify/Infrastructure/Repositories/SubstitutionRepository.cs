using Microsoft.EntityFrameworkCore;
using Plannify.Application.Contracts;
using Plannify.Data;
using Plannify.Domain.Entities;
using Plannify.Models;

namespace Plannify.Infrastructure.Repositories;

/// <summary>
/// Implementation of ISubstitutionRepository for substitution data access
/// Maps between SubstitutionRecord (EF Core model) and Substitution (domain entity)
/// </summary>
public class SubstitutionRepository : ISubstitutionRepository
{
    private readonly AppDbContext _context;

    public SubstitutionRepository(AppDbContext context)
    {
        _context = context;
    }

    // Helper method to map SubstitutionRecord to Substitution domain entity
    private static Substitution MapToEntity(SubstitutionRecord record)
    {
        // Create using reflection since the constructor is not public
        var entity = (Substitution)Activator.CreateInstance(typeof(Substitution), nonPublic: true)!;
        
        // Use reflection to set private properties
        typeof(Substitution).GetProperty("Id")?.SetValue(entity, record.Id);
        typeof(Substitution).GetProperty("TimetableSlotId")?.SetValue(entity, record.TimetableSlotId);
        typeof(Substitution).GetProperty("OriginalTeacherId")?.SetValue(entity, record.OriginalTeacherId);
        typeof(Substitution).GetProperty("SubstituteTeacherId")?.SetValue(entity, record.SubstituteTeacherId);
        typeof(Substitution).GetProperty("Date")?.SetValue(entity, record.Date);
        typeof(Substitution).GetProperty("Reason")?.SetValue(entity, record.Reason);
        typeof(Substitution).GetProperty("ApprovedBy")?.SetValue(entity, record.ApprovedBy);
        typeof(Substitution).GetProperty("CreatedAt")?.SetValue(entity, record.CreatedAt);

        return entity;
    }

    // Helper method to map Substitution domain entity to SubstitutionRecord
    private static SubstitutionRecord MapToRecord(Substitution entity)
    {
        return new SubstitutionRecord
        {
            TimetableSlotId = entity.TimetableSlotId,
            OriginalTeacherId = entity.OriginalTeacherId,
            SubstituteTeacherId = entity.SubstituteTeacherId,
            Date = entity.Date,
            Reason = entity.Reason,
            ApprovedBy = entity.ApprovedBy,
            CreatedAt = entity.CreatedAt
        };
    }

    public async Task<Substitution?> GetByIdAsync(int id)
    {
        var record = await _context.SubstitutionRecords.FirstOrDefaultAsync(s => s.Id == id);
        return record != null ? MapToEntity(record) : null;
    }

    public async Task<List<Substitution>> GetAllAsync()
    {
        var records = await _context.SubstitutionRecords.ToListAsync();
        return records.Select(MapToEntity).ToList();
    }

    public async Task<List<Substitution>> GetByTeacherAsync(int teacherId)
    {
        var records = await _context.SubstitutionRecords
            .Where(s => s.OriginalTeacherId == teacherId || s.SubstituteTeacherId == teacherId)
            .ToListAsync();
        return records.Select(MapToEntity).ToList();
    }

    public async Task<List<Substitution>> GetByOriginalTeacherAsync(int teacherId)
    {
        var records = await _context.SubstitutionRecords
            .Where(s => s.OriginalTeacherId == teacherId)
            .ToListAsync();
        return records.Select(MapToEntity).ToList();
    }

    public async Task<List<Substitution>> GetBySubstituteTeacherAsync(int teacherId)
    {
        var records = await _context.SubstitutionRecords
            .Where(s => s.SubstituteTeacherId == teacherId)
            .ToListAsync();
        return records.Select(MapToEntity).ToList();
    }

    public async Task<List<Substitution>> GetByTimetableSlotAsync(int slotId)
    {
        var records = await _context.SubstitutionRecords
            .Where(s => s.TimetableSlotId == slotId)
            .ToListAsync();
        return records.Select(MapToEntity).ToList();
    }

    public async Task<List<Substitution>> GetByDateAsync(DateOnly date)
    {
        var records = await _context.SubstitutionRecords
            .Where(s => s.Date == date)
            .ToListAsync();
        return records.Select(MapToEntity).ToList();
    }

    public async Task<List<Substitution>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        var records = await _context.SubstitutionRecords
            .Where(s => s.Date >= startDate && s.Date <= endDate)
            .OrderBy(s => s.Date)
            .ToListAsync();
        return records.Select(MapToEntity).ToList();
    }

    public async Task<bool> ExistsBySlotAndDateAsync(int slotId, DateOnly date)
    {
        return await _context.SubstitutionRecords
            .AnyAsync(s => s.TimetableSlotId == slotId && s.Date == date);
    }

    public async Task<bool> TeacherHasSubstitutionOnDateAsync(int teacherId, DateOnly date)
    {
        return await _context.SubstitutionRecords
            .AnyAsync(s => (s.OriginalTeacherId == teacherId || s.SubstituteTeacherId == teacherId) 
                && s.Date == date);
    }

    public async Task<List<Substitution>> GetActiveAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var records = await _context.SubstitutionRecords
            .Where(s => s.Date >= today)
            .OrderBy(s => s.Date)
            .ToListAsync();
        return records.Select(MapToEntity).ToList();
    }

    public async Task<List<Substitution>> GetUrgentAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var oneWeekFromNow = today.AddDays(7);
        var oneDayAgo = DateTime.UtcNow.AddHours(-24);

        var records = await _context.SubstitutionRecords
            .Where(s => s.Date >= today && s.Date <= oneWeekFromNow && s.CreatedAt >= oneDayAgo)
            .OrderBy(s => s.Date)
            .ToListAsync();
        return records.Select(MapToEntity).ToList();
    }

    public async Task<int> AddAsync(Substitution substitution)
    {
        var record = MapToRecord(substitution);
        _context.SubstitutionRecords.Add(record);
        await _context.SaveChangesAsync();
        return record.Id;
    }

    public async Task UpdateAsync(Substitution substitution)
    {
        var record = await _context.SubstitutionRecords.FirstOrDefaultAsync(s => s.Id == substitution.Id);
        if (record != null)
        {
            record.Reason = substitution.Reason;
            record.ApprovedBy = substitution.ApprovedBy;
            record.SubstituteTeacherId = substitution.SubstituteTeacherId;
            _context.SubstitutionRecords.Update(record);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        var record = await _context.SubstitutionRecords.FirstOrDefaultAsync(s => s.Id == id);
        if (record != null)
        {
            _context.SubstitutionRecords.Remove(record);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.SubstitutionRecords.CountAsync();
    }

    public async Task<int> GetCountByTeacherAsync(int teacherId)
    {
        return await _context.SubstitutionRecords
            .Where(s => s.OriginalTeacherId == teacherId || s.SubstituteTeacherId == teacherId)
            .CountAsync();
    }
}

