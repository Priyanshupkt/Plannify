using System.ComponentModel.DataAnnotations;

namespace Plannify.Models;

public class Room
{
    public int Id { get; set; }

    [Required]
    public string RoomNumber { get; set; } = string.Empty;

    [Required]
    public string BuildingName { get; set; } = string.Empty;

    public int Capacity { get; set; }

    [Required]
    public string RoomType { get; set; } = "Lecture";

    public bool IsActive { get; set; } = true;

    public virtual List<TimetableSlot> TimetableSlots { get; set; } = new();
}
