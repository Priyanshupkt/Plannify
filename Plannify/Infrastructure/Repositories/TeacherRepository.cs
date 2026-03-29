using Microsoft.EntityFrameworkCore;
using Plannify.Application.Contracts;
using Plannify.Data;
using Plannify.Domain.Entities;

namespace Plannify.Infrastructure.Repositories;

/// <summary>
/// Teacher repository implementation
/// Handles all data access for Teacher entity
/// </summary>
public class TeacherRepository : GenericRepository<Teacher>, ITeacherRepository
{
    public TeacherRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Teacher?> GetByEmployeeCodeAsync(string employeeCode)
        => await _dbSet
            .Include(t => t.Department)
            .FirstOrDefaultAsync(t => t.EmployeeCode == employeeCode);

    public async Task<IEnumerable<Teacher>> GetByDepartmentAsync(int departmentId)
        => await _dbSet
            .Where(t => t.DepartmentId == departmentId)
            .Include(t => t.Department)
            .OrderBy(t => t.FullName)
            .ToListAsync();

    public async Task<IEnumerable<Teacher>> GetActiveAsync()
        => await _dbSet
            .Where(t => t.IsActive)
            .Include(t => t.Department)
            .OrderBy(t => t.FullName)
            .ToListAsync();

    public async Task<Teacher?> GetWithTimetableSlotsAsync(int teacherId, int semesterId)
        => await _dbSet
            .Where(t => t.Id == teacherId)
            .Include(t => t.TimetableSlots.Where(s => s.SemesterId == semesterId))
            .Include(t => t.Department)
            .FirstOrDefaultAsync();

    public async Task<bool> EmployeeCodeExistsAsync(string employeeCode, int? excludeTeacherId = null)
    {
        var query = _dbSet.Where(t => t.EmployeeCode == employeeCode);
        
        if (excludeTeacherId.HasValue)
            query = query.Where(t => t.Id != excludeTeacherId.Value);

        return await query.AnyAsync();
    }

    public async Task<decimal> GetAllocatedHoursAsync(int teacherId, int semesterId)
    {
        var slots = await _context.TimetableSlots
            .Where(s => s.TeacherId == teacherId && s.SemesterId == semesterId)
            .ToListAsync();

        decimal totalHours = 0;
        foreach (var slot in slots)
        {
            var hours = (slot.EndTime.Hour - slot.StartTime.Hour) +
                       ((slot.EndTime.Minute - slot.StartTime.Minute) / 60m);
            totalHours += hours;
        }

        return totalHours;
    }
}
