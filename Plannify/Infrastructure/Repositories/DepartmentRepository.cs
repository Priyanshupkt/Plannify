using Microsoft.EntityFrameworkCore;
using Plannify.Application.Contracts;
using Plannify.Data;
using DomainDepartment = Plannify.Domain.Entities.Department;

namespace Plannify.Infrastructure.Repositories;

/// <summary>
/// Department repository implementation
/// Handles all data access for Department entity
/// </summary>
public class DepartmentRepository : GenericRepository<DomainDepartment>, IDepartmentRepository
{
    private readonly AppDbContext _appContext;

    public DepartmentRepository(AppDbContext context) : base(context)
    {
        _appContext = context;
    }

    public async Task<DomainDepartment?> GetByCodeAsync(string code)
        => await _dbSet.FirstOrDefaultAsync(d => d.Code == code);

    public async Task<bool> CodeExistsAsync(string code, int? excludeDepartmentId = null)
    {
        var query = _dbSet.Where(d => d.Code == code);

        if (excludeDepartmentId.HasValue)
            query = query.Where(d => d.Id != excludeDepartmentId.Value);

        return await query.AnyAsync();
    }

    public async Task<int> GetTeacherCountAsync(int departmentId)
        => await _appContext.Teachers.CountAsync(t => t.DepartmentId == departmentId);

    public async Task<int> GetSubjectCountAsync(int departmentId)
        => await _appContext.Subjects.CountAsync(s => s.DepartmentId == departmentId);

    public async Task<int> GetClassCountAsync(int departmentId)
        => await _appContext.ClassBatches.CountAsync(c => c.DepartmentId == departmentId);
}
