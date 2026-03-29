using Microsoft.EntityFrameworkCore;
using Plannify.Application.Contracts;
using Plannify.Data;
using DomainAcademicYear = Plannify.Domain.Entities.AcademicYear;

namespace Plannify.Infrastructure.Repositories;

/// <summary>
/// AcademicYear repository implementation
/// Handles all data access for AcademicYear entity
/// </summary>
public class AcademicYearRepository : GenericRepository<DomainAcademicYear>, IAcademicYearRepository
{
    public AcademicYearRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<DomainAcademicYear?> GetByYearLabelAsync(string yearLabel)
        => await _dbSet.FirstOrDefaultAsync(a => a.YearLabel == yearLabel);

    public async Task<DomainAcademicYear?> GetCurrentAcademicYearAsync()
        => await _dbSet
            .Where(a => a.IsActive && a.StartDate <= DateTime.Now && a.EndDate >= DateTime.Now)
            .OrderBy(a => a.StartDate)
            .FirstOrDefaultAsync();

    public async Task<bool> YearLabelExistsAsync(string yearLabel, int? excludeYearId = null)
    {
        var query = _dbSet.Where(a => a.YearLabel == yearLabel);

        if (excludeYearId.HasValue)
            query = query.Where(a => a.Id != excludeYearId.Value);

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<DomainAcademicYear>> GetAllActiveAsync()
        => await _dbSet
            .Where(a => a.IsActive)
            .OrderByDescending(a => a.StartDate)
            .ToListAsync();
}
