using Microsoft.EntityFrameworkCore;
using Plannify.Application.Contracts;
using Plannify.Data;
using DomainSubject = Plannify.Domain.Entities.Subject;

namespace Plannify.Infrastructure.Repositories;

/// <summary>
/// Subject repository implementation
/// Handles all data access for Subject entity
/// </summary>
public class SubjectRepository : GenericRepository<DomainSubject>, ISubjectRepository
{
    private readonly AppDbContext _appContext;

    public SubjectRepository(AppDbContext context) : base(context)
    {
        _appContext = context;
    }

    public async Task<DomainSubject?> GetByCodeAndDepartmentAsync(string code, int departmentId)
        => await _dbSet.FirstOrDefaultAsync(s => s.Code == code && s.DepartmentId == departmentId);

    public async Task<bool> CodeExistsInDepartmentAsync(string code, int departmentId, int? excludeSubjectId = null)
    {
        var query = _dbSet.Where(s => s.Code == code && s.DepartmentId == departmentId);

        if (excludeSubjectId.HasValue)
            query = query.Where(s => s.Id != excludeSubjectId.Value);

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<DomainSubject>> GetByDepartmentAsync(int departmentId)
        => await _dbSet
            .Where(s => s.DepartmentId == departmentId)
            .OrderBy(s => s.Code)
            .ToListAsync();

    public async Task<IEnumerable<DomainSubject>> GetBySemesterAsync(int semesterNumber)
        => await _dbSet
            .Where(s => s.SemesterNumber == semesterNumber)
            .OrderBy(s => s.Code)
            .ToListAsync();

    public async Task<IEnumerable<DomainSubject>> GetByDepartmentAndSemesterAsync(int departmentId, int semesterNumber)
        => await _dbSet
            .Where(s => s.DepartmentId == departmentId && s.SemesterNumber == semesterNumber)
            .OrderBy(s => s.Code)
            .ToListAsync();
}
