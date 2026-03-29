using Microsoft.EntityFrameworkCore;
using Plannify.Application.Contracts;
using Plannify.Data;
using DomainClassBatch = Plannify.Domain.Entities.ClassBatch;

namespace Plannify.Infrastructure.Repositories;

/// <summary>
/// ClassBatch repository implementation
/// Handles all data access for ClassBatch entity
/// </summary>
public class ClassBatchRepository : GenericRepository<DomainClassBatch>, IClassBatchRepository
{
    public ClassBatchRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<DomainClassBatch>> GetByDepartmentAsync(int departmentId)
        => await _dbSet
            .Where(c => c.DepartmentId == departmentId)
            .OrderBy(c => c.Semester)
            .ThenBy(c => c.BatchName)
            .ToListAsync();

    public async Task<IEnumerable<DomainClassBatch>> GetByAcademicYearAsync(int academicYearId)
        => await _dbSet
            .Where(c => c.AcademicYearId == academicYearId)
            .OrderBy(c => c.DepartmentId)
            .ThenBy(c => c.Semester)
            .ToListAsync();

    public async Task<IEnumerable<DomainClassBatch>> GetByDepartmentAndSemesterAsync(int departmentId, int semester)
        => await _dbSet
            .Where(c => c.DepartmentId == departmentId && c.Semester == semester)
            .OrderBy(c => c.BatchName)
            .ToListAsync();

    public async Task<IEnumerable<DomainClassBatch>> GetByRoomAsync(int roomId)
        => await _dbSet
            .Where(c => c.RoomId == roomId)
            .OrderBy(c => c.BatchName)
            .ToListAsync();

    public async Task<bool> BatchNameExistsAsync(string batchName, int departmentId, int academicYearId, int? excludeClassId = null)
    {
        var query = _dbSet.Where(c => 
            c.BatchName == batchName && 
            c.DepartmentId == departmentId && 
            c.AcademicYearId == academicYearId);

        if (excludeClassId.HasValue)
            query = query.Where(c => c.Id != excludeClassId.Value);

        return await query.AnyAsync();
    }
}
