using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Plannify.Data;
using Plannify.Models;
using Plannify.Services;

namespace Plannify.Pages.Admin.Rooms;

[Authorize(Roles = "SuperAdmin")]
public class IndexModel : PageModel
{
    private readonly AppDbContext _dbContext;
    private readonly AuditService _auditService;

    public IndexModel(AppDbContext dbContext, AuditService auditService)
    {
        _dbContext = dbContext;
        _auditService = auditService;
    }

    [BindProperty]
    public Room NewRoom { get; set; } = new();

    public List<Room> Rooms { get; set; } = new();
    public Dictionary<int, int> SlotCounts { get; set; } = new();

    public async Task OnGetAsync()
    {
        Rooms = await _dbContext.Rooms.OrderBy(r => r.RoomNumber).ToListAsync();

        foreach (var room in Rooms)
        {
            var count = await _dbContext.TimetableSlots.CountAsync(t => t.RoomId == room.Id);
            SlotCounts[room.Id] = count;
        }
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var roomExists = await _dbContext.Rooms.AnyAsync(r => r.RoomNumber == NewRoom.RoomNumber);
        if (roomExists)
        {
            TempData["Error"] = $"Room number '{NewRoom.RoomNumber}' already exists.";
            await OnGetAsync();
            return Page();
        }

        _dbContext.Rooms.Add(NewRoom);
        await _dbContext.SaveChangesAsync();

        await _auditService.LogAsync("CREATE", "Room", NewRoom.Id.ToString(),
            null, $"Number: {NewRoom.RoomNumber}, Building: {NewRoom.BuildingName}, Type: {NewRoom.RoomType}");

        TempData["Success"] = $"Room '{NewRoom.RoomNumber}' added successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync(int id, string roomNumber, string buildingName, int capacity, string roomType)
    {
        var room = await _dbContext.Rooms.FindAsync(id);
        if (room == null)
        {
            TempData["Error"] = "Room not found.";
            return RedirectToPage();
        }

        var oldValues = $"Number: {room.RoomNumber}, Building: {room.BuildingName}";

        room.RoomNumber = roomNumber;
        room.BuildingName = buildingName;
        room.Capacity = capacity;
        room.RoomType = roomType;

        _dbContext.Rooms.Update(room);
        await _dbContext.SaveChangesAsync();

        var newValues = $"Number: {room.RoomNumber}, Building: {room.BuildingName}";
        await _auditService.LogAsync("UPDATE", "Room", room.Id.ToString(), oldValues, newValues);

        TempData["Success"] = "Room updated successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var room = await _dbContext.Rooms.FindAsync(id);
        if (room == null)
        {
            TempData["Error"] = "Room not found.";
            return RedirectToPage();
        }

        var slotCount = await _dbContext.TimetableSlots.CountAsync(t => t.RoomId == id);
        if (slotCount > 0)
        {
            TempData["Error"] = $"Cannot delete room. It has {slotCount} timetable slots assigned.";
            await OnGetAsync();
            return Page();
        }

        var roomNumber = room.RoomNumber;
        _dbContext.Rooms.Remove(room);
        await _dbContext.SaveChangesAsync();

        await _auditService.LogAsync("DELETE", "Room", id.ToString(),
            $"Number: {roomNumber}, Building: {room.BuildingName}", null);

        TempData["Success"] = $"Room '{roomNumber}' deleted successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetCheckAvailabilityAsync(int roomId, string day, string startTime, string endTime)
    {
        var conflicts = await _dbContext.TimetableSlots
            .Where(t => t.RoomId == roomId && t.Day == day)
            .Where(t => TimeOnly.Parse(t.StartTime.ToString(@"hh\:mm")) < TimeOnly.Parse(endTime) &&
                        TimeOnly.Parse(t.EndTime.ToString(@"hh\:mm")) > TimeOnly.Parse(startTime))
            .ToListAsync();

        return new JsonResult(new { available = conflicts.Count == 0, conflicts = conflicts.Count });
    }
}
