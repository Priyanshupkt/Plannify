using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Plannify.Data;
using Plannify.Models;
using Plannify.Services;
using System.Text.Json;

namespace Plannify.Pages.Admin.Substitutions;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly AuditService _auditService;

    public IndexModel(AppDbContext context, AuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    [BindProperty]
    public SubstitutionInput Input { get; set; } = new();

    public List<SelectListItem> Teachers { get; set; } = new();
    public List<SubstitutionRecord> SubstitutionHistory { get; set; } = new();

    public class SubstitutionInput
    {
        public DateOnly Date { get; set; }
        public int AbsentTeacherId { get; set; }
        public int TimetableSlotId { get; set; }
        public int SubstituteTeacherId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string ApprovedBy { get; set; } = string.Empty;
    }

    public async Task OnGetAsync()
    {
        Teachers = await _context.Teachers
            .Where(t => t.IsActive)
            .OrderBy(t => t.FullName)
            .Select(t => new SelectListItem($"{t.FullName} ({t.Initials})", t.Id.ToString()))
            .ToListAsync();

        await LoadHistoryAsync();
    }

    public async Task<IActionResult> OnGetAbsentTeacherSlotsAsync(int teacherId, string date)
    {
        if (!DateOnly.TryParse(date, out var dateOnly))
            return new JsonResult(new[] { "Invalid date format" });

        var dayOfWeek = dateOnly.DayOfWeek.ToString();

        var slots = await _context.TimetableSlots
            .Where(s => s.TeacherId == teacherId && s.Day == dayOfWeek && s.SlotType != "GAP")
            .Include(s => s.Subject)
            .Include(s => s.ClassBatch)
            .Include(s => s.Room)
            .Select(s => new
            {
                id = s.Id,
                subject = s.Subject!.Name,
                classBatch = s.ClassBatch!.BatchName,
                time = $"{s.StartTime:HH:mm}–{s.EndTime:HH:mm}",
                room = s.Room!.RoomNumber,
                startTime = s.StartTime.ToString("HH:mm"),
                endTime = s.EndTime.ToString("HH:mm"),
                day = s.Day
            })
            .ToListAsync();

        return new JsonResult(slots);
    }

    public async Task<IActionResult> OnGetAvailableSubstitutesAsync(int slotId, string date)
    {
        var slot = await _context.TimetableSlots
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == slotId);

        if (slot == null)
            return new JsonResult(new[] { "Slot not found" });

        // Find all teachers NOT having a conflicting slot at that time
        var busyTeachers = await _context.TimetableSlots
            .Where(s =>
                s.Day == slot.Day &&
                s.SlotType != "GAP" &&
                s.Id != slotId &&
                !(s.EndTime <= slot.StartTime || s.StartTime >= slot.EndTime))
            .Select(s => s.TeacherId)
            .ToListAsync();

        var availableTeachers = await _context.Teachers
            .Where(t => t.IsActive && !busyTeachers.Contains(t.Id))
            .Select(t => new
            {
                id = t.Id,
                name = t.FullName,
                initials = t.Initials,
                department = t.Department!.Name
            })
            .ToListAsync();

        return new JsonResult(availableTeachers);
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        // Verify substitute is free
        var slot = await _context.TimetableSlots
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == Input.TimetableSlotId);

        if (slot == null)
        {
            ModelState.AddModelError(string.Empty, "Slot not found.");
            await OnGetAsync();
            return Page();
        }

        var conflictingSlot = await _context.TimetableSlots
            .AsNoTracking()
            .FirstOrDefaultAsync(s =>
                s.TeacherId == Input.SubstituteTeacherId &&
                s.Day == slot.Day &&
                s.SlotType != "GAP" &&
                !(s.EndTime <= slot.StartTime || s.StartTime >= slot.EndTime));

        if (conflictingSlot != null)
        {
            ModelState.AddModelError(string.Empty, "Selected substitute is not available at this time.");
            await OnGetAsync();
            return Page();
        }

        // Check for existing substitution on same date/slot
        var existingSubstitution = await _context.SubstitutionRecords
            .AnyAsync(s => s.TimetableSlotId == Input.TimetableSlotId && s.Date == Input.Date);

        if (existingSubstitution)
        {
            ModelState.AddModelError(string.Empty, "A substitution already exists for this slot on this date.");
            await OnGetAsync();
            return Page();
        }

        // Create substitution record
        var substitution = new SubstitutionRecord
        {
            TimetableSlotId = Input.TimetableSlotId,
            OriginalTeacherId = Input.AbsentTeacherId,
            SubstituteTeacherId = Input.SubstituteTeacherId,
            Date = Input.Date,
            Reason = Input.Reason,
            CreatedAt = DateTime.UtcNow
        };

        _context.SubstitutionRecords.Add(substitution);
        await _context.SaveChangesAsync();

        // Audit log
        await _auditService.LogAsync(
            "CREATE",
            "SubstitutionRecord",
            substitution.Id.ToString(),
            null,
            JsonSerializer.Serialize(new
            {
                substitution.Date,
                Original = Input.AbsentTeacherId,
                Substitute = Input.SubstituteTeacherId,
                substitution.Reason
            }));

        TempData["Success"] = "Substitution recorded.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var substitution = await _context.SubstitutionRecords
            .Include(s => s.TimetableSlot)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (substitution == null)
            return NotFound();

        _context.SubstitutionRecords.Remove(substitution);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            "DELETE",
            "SubstitutionRecord",
            id.ToString(),
            JsonSerializer.Serialize(new { substitution.Date }),
            null);

        TempData["Success"] = "Substitution deleted successfully.";
        return RedirectToPage();
    }

    private async Task LoadHistoryAsync()
    {
        SubstitutionHistory = await _context.SubstitutionRecords
            .Include(s => s.OriginalTeacher)
            .Include(s => s.SubstituteTeacher)
            .Include(s => s.TimetableSlot)
            .ThenInclude(s => s.Subject)
            .Include(s => s.TimetableSlot)
            .ThenInclude(s => s.ClassBatch)
            .OrderByDescending(s => s.Date)
            .Take(50)
            .ToListAsync();
    }
}
