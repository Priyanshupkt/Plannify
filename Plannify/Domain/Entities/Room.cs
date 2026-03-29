using Plannify.Application.Common;

namespace Plannify.Domain.Entities;

/// <summary>
/// Room domain entity
/// Represents a physical classroom or facility for timetable allocation
/// </summary>
public class Room
{
    private Room(int id, string roomNumber, string buildingName, int capacity, string roomType)
    {
        Id = id;
        RoomNumber = roomNumber;
        BuildingName = buildingName;
        Capacity = capacity;
        RoomType = roomType;
    }

    public int Id { get; private set; }
    public string RoomNumber { get; private set; }
    public string BuildingName { get; private set; }
    public int Capacity { get; private set; }
    public string RoomType { get; private set; }

    /// <summary>
    /// Factory method to create a new Room with business rule validation
    /// </summary>
    public static Result<Room> Create(string roomNumber, string buildingName, int capacity, string roomType = "Lecture")
    {
        // Validate room number
        if (string.IsNullOrWhiteSpace(roomNumber))
            return Result<Room>.Failure("Room number is required");

        if (roomNumber.Length < 1 || roomNumber.Length > 50)
            return Result<Room>.Failure("Room number must be between 1 and 50 characters");

        // Validate building name
        if (string.IsNullOrWhiteSpace(buildingName))
            return Result<Room>.Failure("Building name is required");

        if (buildingName.Length < 2 || buildingName.Length > 100)
            return Result<Room>.Failure("Building name must be between 2 and 100 characters");

        // Validate capacity
        if (capacity < 1)
            return Result<Room>.Failure("Room capacity must be at least 1");

        if (capacity > 500)
            return Result<Room>.Failure("Room capacity cannot exceed 500");

        // Validate room type
        if (string.IsNullOrWhiteSpace(roomType))
            return Result<Room>.Failure("Room type is required");

        return Result<Room>.Success(new Room(0, roomNumber, buildingName, capacity, roomType));
    }

    /// <summary>
    /// Update room details with validation
    /// </summary>
    public Result Update(string roomNumber, string buildingName, int capacity, string roomType)
    {
        // Validate room number
        if (string.IsNullOrWhiteSpace(roomNumber))
            return Result.Failure("Room number is required");

        if (roomNumber.Length < 1 || roomNumber.Length > 50)
            return Result.Failure("Room number must be between 1 and 50 characters");

        // Validate building name
        if (string.IsNullOrWhiteSpace(buildingName))
            return Result.Failure("Building name is required");

        if (buildingName.Length < 2 || buildingName.Length > 100)
            return Result.Failure("Building name must be between 2 and 100 characters");

        // Validate capacity
        if (capacity < 1)
            return Result.Failure("Room capacity must be at least 1");

        if (capacity > 500)
            return Result.Failure("Room capacity cannot exceed 500");

        // Validate room type
        if (string.IsNullOrWhiteSpace(roomType))
            return Result.Failure("Room type is required");

        RoomNumber = roomNumber;
        BuildingName = buildingName;
        Capacity = capacity;
        RoomType = roomType;

        return Result.Success();
    }
}
