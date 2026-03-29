namespace Plannify.Application.DTOs;

/// <summary>
/// Request DTO for creating a room
/// </summary>
public class CreateRoomRequest
{
    public string RoomNumber { get; set; } = string.Empty;
    public string BuildingName { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string RoomType { get; set; } = "Lecture";
}

/// <summary>
/// Request DTO for updating a room
/// </summary>
public class UpdateRoomRequest
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string BuildingName { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string RoomType { get; set; } = "Lecture";
}

/// <summary>
/// Response DTO for room details
/// </summary>
public class RoomDto
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string BuildingName { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

/// <summary>
/// Summary DTO for room list views
/// </summary>
public class RoomSummaryDto
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string BuildingName { get; set; } = string.Empty;
    public int Capacity { get; set; }
}
