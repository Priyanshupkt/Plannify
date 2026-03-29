using Microsoft.EntityFrameworkCore;
using Plannify.Application.Contracts;
using Plannify.Data;
using DomainRoom = Plannify.Domain.Entities.Room;

namespace Plannify.Infrastructure.Repositories;

/// <summary>
/// Room repository implementation
/// Handles all data access for Room entity
/// </summary>
public class RoomRepository : GenericRepository<DomainRoom>, IRoomRepository
{
    public RoomRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<DomainRoom?> GetByRoomNumberAsync(string roomNumber)
        => await _dbSet.FirstOrDefaultAsync(r => r.RoomNumber == roomNumber);

    public async Task<bool> RoomNumberExistsAsync(string roomNumber, int? excludeRoomId = null)
    {
        var query = _dbSet.Where(r => r.RoomNumber == roomNumber);

        if (excludeRoomId.HasValue)
            query = query.Where(r => r.Id != excludeRoomId.Value);

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<DomainRoom>> GetByBuildingAsync(string building)
        => await _dbSet
            .Where(r => r.BuildingName == building)
            .OrderBy(r => r.RoomNumber)
            .ToListAsync();

    public async Task<IEnumerable<DomainRoom>> GetAvailableRoomsAsync(int minCapacity)
        => await _dbSet
            .Where(r => r.Capacity >= minCapacity)
            .OrderBy(r => r.Capacity)
            .ToListAsync();
}
