using DomainRoom = Plannify.Domain.Entities.Room;

namespace Plannify.Application.Contracts;

/// <summary>
/// Repository abstraction for Room entity
/// Extends generic repository with room-specific operations
/// </summary>
public interface IRoomRepository : IGenericRepository<DomainRoom>
{
    /// <summary>
    /// Get room by room number (unique constraint)
    /// </summary>
    Task<DomainRoom?> GetByRoomNumberAsync(string roomNumber);

    /// <summary>
    /// Check if room number already exists
    /// </summary>
    Task<bool> RoomNumberExistsAsync(string roomNumber, int? excludeRoomId = null);

    /// <summary>
    /// Get rooms by building location
    /// </summary>
    Task<IEnumerable<DomainRoom>> GetByBuildingAsync(string building);

    /// <summary>
    /// Get available rooms (capacity >= requested)
    /// </summary>
    Task<IEnumerable<DomainRoom>> GetAvailableRoomsAsync(int minCapacity);
}
