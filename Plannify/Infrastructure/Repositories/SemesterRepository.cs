using Microsoft.EntityFrameworkCore;
using Plannify.Application.Contracts;
using Plannify.Data;
using DomainSemester = Plannify.Domain.Entities.Semester;

namespace Plannify.Infrastructure.Repositories;

/// <summary>
/// Semester repository implementation
/// Handles all data access for Semester entity
/// </summary>
public class SemesterRepository : GenericRepository<DomainSemester>, ISemesterRepository
{
    public SemesterRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<DomainSemester?> GetByNumberAndYearAsync(int semesterNumber, int academicYearId)
        => await _dbSet.FirstOrDefaultAsync(s => s.SemesterNumber == semesterNumber && s.AcademicYearId == academicYearId);

    public async Task<IEnumerable<DomainSemester>> GetByAcademicYearAsync(int academicYearId)
        => await _dbSet
            .Where(s => s.AcademicYearId == academicYearId)
            .OrderBy(s => s.SemesterNumber)
            .ToListAsync();

    public async Task<DomainSemester?> GetCurrentSemesterAsync()
        => await _dbSet
            .Where(s => s.IsActive && s.StartDate <= DateTime.Now && s.EndDate >= DateTime.Now)
            .OrderBy(s => s.StartDate)
            .FirstOrDefaultAsync();

    public async Task<bool> ExistsForYearAsync(int semesterNumber, int academicYearId, int? excludeSemesterId = null)
    {
        var query = _dbSet.Where(s => s.SemesterNumber == semesterNumber && s.AcademicYearId == academicYearId);

        if (excludeSemesterId.HasValue)
            query = query.Where(s => s.Id != excludeSemesterId.Value);

        return await query.AnyAsync();
    }
}
