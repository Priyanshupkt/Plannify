using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Plannify.Application.Contracts;
using Plannify.Application.DTOs;

namespace Plannify.Pages.Admin.Rooms;

/// <summary>
/// Refactored Rooms page model
/// ✅ No DbContext injection
/// ✅ No data access code
/// ✅ Only presentation concerns
/// ✅ Delegates to application service
/// </summary>
[Authorize]
public class IndexModel : PageModel
{
    private readonly IRoomService _roomService;

    // ✅ Only DTOs, no entities
    public List<RoomDto> Rooms { get; set; } = new();
    public Dictionary<int, int> SlotCounts { get; set; } = new();

    [BindProperty]
    public CreateRoomRequest NewRoom { get; set; } = new();

    [BindProperty]
    public UpdateRoomRequest EditRoom { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public IndexModel(IRoomService roomService)
    {
        _roomService = roomService;
    }

    /// <summary>
    /// Load all rooms
    /// </summary>
    public async Task OnGetAsync()
    {
        var result = await _roomService.GetAllAsync();
        if (result.IsSuccess)
        {
            Rooms = result.Value!.ToList();
        }
        else
        {
            ErrorMessage = result.ErrorMessage;
            Rooms = new();
        }
    }

    /// <summary>
    /// Create new room
    /// </summary>
    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var result = await _roomService.CreateAsync(NewRoom);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            await OnGetAsync();
            return Page();
        }

        TempData["Success"] = $"Room '{NewRoom.RoomNumber}' added successfully.";
        return RedirectToPage();
    }

    /// <summary>
    /// Update existing room
    /// </summary>
    public async Task<IActionResult> OnPostUpdateAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var result = await _roomService.UpdateAsync(EditRoom);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            await OnGetAsync();
            return Page();
        }

        TempData["Success"] = $"Room '{EditRoom.RoomNumber}' updated successfully.";
        return RedirectToPage();
    }

    /// <summary>
    /// Delete room
    /// </summary>
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await _roomService.DeleteAsync(id);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToPage();
        }

        TempData["Success"] = "Room deleted successfully.";
        return RedirectToPage();
    }
}
