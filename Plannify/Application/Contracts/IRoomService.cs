using Plannify.Application.Common;
using Plannify.Application.DTOs;

namespace Plannify.Application.Contracts;

/// <summary>
/// Service interface for Room operations
/// All room business logic is delegated through this abstraction
/// </summary>
public interface IRoomService
{
    /// <summary>
    /// Get room by ID
    /// </summary>
    Task<Result<RoomDto>> GetByIdAsync(int id);

    /// <summary>
    /// Get all rooms
    /// </summary>
    Task<Result<IEnumerable<RoomDto>>> GetAllAsync();

    /// <summary>
    /// Get room summary list
    /// </summary>
    Task<Result<IEnumerable<RoomSummaryDto>>> GetSummaryListAsync();

    /// <summary>
    /// Create new room
    /// </summary>
    Task<Result<int>> CreateAsync(CreateRoomRequest request);

    /// <summary>
    /// Update existing room
    /// </summary>
    Task<Result> UpdateAsync(UpdateRoomRequest request);

    /// <summary>
    /// Delete room
    /// </summary>
    Task<Result> DeleteAsync(int id);
}
