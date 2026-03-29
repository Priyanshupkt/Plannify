using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Plannify.Application.Contracts;
using Plannify.Data;
using Plannify.Models;
using Plannify.Services;
using System.Text.Json;

namespace Plannify.Pages.Admin.Timetable;

public class CreateModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly IConflictDetectorService _conflictDetector;
    private readonly AuditService _auditService;

    public CreateModel(AppDbContext context, IConflictDetectorService conflictDetector, AuditService auditService)
    {
        _context = context;
        _conflictDetector = conflictDetector;
        _auditService = auditService;
    }

    [BindProperty]
    public TimetableSlotInput Input { get; set; } = new();

    public List<SelectListItem> Semesters { get; set; } = new();
    public List<SelectListItem> ClassBatches { get; set; } = new();
    public List<SelectListItem> Days { get; set; } = new();
    public List<SelectListItem> SlotTypes { get; set; } = new();
    public List<SelectListItem> Teachers { get; set; } = new();
    public List<SelectListItem> Subjects { get; set; } = new();
    public List<SelectListItem> Rooms { get; set; } = new();

    public class TimetableSlotInput
    {
        public int SemesterId { get; set; }
        public int ClassBatchId { get; set; }
        public string Day { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string SlotType { get; set; } = "Theory";
        public int? TeacherId { get; set; }
        public int? SubjectId { get; set; }
        public int? RoomId { get; set; }
        public bool IsMultiPeriodLab { get; set; }
        public string? LabGroupTag { get; set; }
        public bool IgnoreConflicts { get; set; }
    }

    public async Task OnGetAsync(int? classId, int? semesterId)
    {
        await LoadDropdownDataAsync();

        if (classId.HasValue)
            Input.ClassBatchId = classId.Value;

        if (semesterId.HasValue)
            Input.SemesterId = semesterId.Value;
        else
        {
            var activeSemester = await _context.Semesters.FirstOrDefaultAsync(s => s.IsActive);
            if (activeSemester != null)
                Input.SemesterId = activeSemester.Id;
        }
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
            return await RedisplayFormAsync();

        // Validate time inputs
        if (!TimeOnly.TryParse(Input.StartTime, out var startTime) ||
            !TimeOnly.TryParse(Input.EndTime, out var endTime))
        {
            ModelState.AddModelError(string.Empty, "Invalid time format.");
            return await RedisplayFormAsync();
        }

        if (endTime <= startTime)
        {
            ModelState.AddModelError(string.Empty, "End time must be after start time.");
            return await RedisplayFormAsync();
        }

        var classBatch = await _context.ClassBatches.FindAsync(Input.ClassBatchId);
        if (classBatch == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid class selected.");
            return await RedisplayFormAsync();
        }

        // Check conflicts if not GAP
        var conflicts = new List<string>();
        if (Input.SlotType != "GAP")
        {
            if (Input.TeacherId.HasValue)
            {
                var teacherConflict = await _conflictDetector.CheckTeacherConflictAsync(
                    Input.TeacherId.Value, Input.Day, startTime, endTime, Input.SemesterId);
                if (teacherConflict.HasConflict)
                    conflicts.Add(teacherConflict.Message);
            }

            if (Input.RoomId.HasValue)
            {
                var roomConflict = await _conflictDetector.CheckRoomConflictAsync(
                    Input.RoomId.Value, Input.Day, startTime, endTime, Input.SemesterId);
                if (roomConflict.HasConflict)
                    conflicts.Add(roomConflict.Message);
            }

            var classConflict = await _conflictDetector.CheckClassConflictAsync(
                Input.ClassBatchId, Input.Day, startTime, endTime, Input.SemesterId);
            if (classConflict.HasConflict)
                conflicts.Add(classConflict.Message);
        }

        // If conflicts found and user didn't acknowledge
        if (conflicts.Count > 0 && !Input.IgnoreConflicts)
        {
            TempData["Conflicts"] = JsonSerializer.Serialize(conflicts);
            return await RedisplayFormAsync();
        }

        // Create slot
        var slot = new TimetableSlot
        {
            SemesterId = Input.SemesterId,
            ClassBatchId = Input.ClassBatchId,
            Day = Input.Day,
            StartTime = startTime,
            EndTime = endTime,
            SlotType = Input.SlotType,
            IsLabSession = Input.SlotType == "Lab",
            LabGroupTag = Input.IsMultiPeriodLab ? Input.LabGroupTag : null,
            TeacherId = Input.SlotType == "GAP" ? null : Input.TeacherId,
            SubjectId = Input.SlotType == "GAP" ? null : Input.SubjectId,
            RoomId = Input.SlotType == "GAP" ? null : Input.RoomId,
            CreatedBy = User.Identity?.Name ?? "System"
        };

        _context.TimetableSlots.Add(slot);
        await _context.SaveChangesAsync();

        // Audit log
        var note = Input.IgnoreConflicts && conflicts.Count > 0
            ? $"Saved with conflicts: {string.Join(", ", conflicts)}"
            : "Slot created successfully";

        await _auditService.LogAsync(
            "CREATE",
            "TimetableSlot",
            slot.Id.ToString(),
            null,
            JsonSerializer.Serialize(new
            {
                slot.Day,
                slot.StartTime,
                slot.EndTime,
                slot.SlotType,
                ClassId = Input.ClassBatchId,
                slot.TeacherId,
                slot.SubjectId,
                slot.RoomId,
                note
            }));

        TempData["Success"] = "Slot added successfully.";
        return RedirectToPage(new { classId = Input.ClassBatchId, semesterId = Input.SemesterId });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var slot = await _context.TimetableSlots
            .Include(s => s.ClassBatch)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (slot == null)
            return NotFound();

        // Check for substitution records
        var hasSubstitutions = await _context.SubstitutionRecords
            .AnyAsync(s => s.TimetableSlotId == id);

        if (hasSubstitutions)
            TempData["Warning"] = "This slot has associated substitution records. Proceed with caution.";

        _context.TimetableSlots.Remove(slot);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            "DELETE",
            "TimetableSlot",
            id.ToString(),
            JsonSerializer.Serialize(new { slot.Day, slot.StartTime, slot.EndTime, slot.SlotType }),
            null);

        TempData["Success"] = "Slot deleted successfully.";
        return RedirectToPage(new { classId = slot.ClassBatchId, semesterId = slot.SemesterId });
    }

    public async Task<IActionResult> OnGetClassGridAsync(int classId, int semesterId)
    {
        var slots = await _context.TimetableSlots
            .Where(s => s.ClassBatchId == classId && s.SemesterId == semesterId)
            .Include(s => s.Subject)
            .Include(s => s.Teacher)
            .Include(s => s.Room)
            .OrderBy(s => GetDayOrder(s.Day))
            .ThenBy(s => s.StartTime)
            .ToListAsync();

        var gridData = slots.Select(s => new
        {
            day = s.Day,
            startTime = s.StartTime.ToString("HH:mm"),
            endTime = s.EndTime.ToString("HH:mm"),
            subject = s.Subject?.Name ?? string.Empty,
            teacher = s.Teacher?.Initials ?? string.Empty,
            room = s.Room?.RoomNumber ?? string.Empty,
            slotType = s.SlotType,
            slotId = s.Id
        }).ToList();

        return new JsonResult(gridData);
    }

    public async Task<IActionResult> OnGetCheckConflictsAsync(
        int teacherId, int roomId, int classId, string day, string start, string end, int semesterId)
    {
        if (!TimeOnly.TryParse(start, out var startTime) || !TimeOnly.TryParse(end, out var endTime))
            return new JsonResult(new[] { "Invalid time format" });

        var conflicts = new List<string>();

        var teacherConflict = await _conflictDetector.CheckTeacherConflictAsync(
            teacherId, day, startTime, endTime, semesterId);
        if (teacherConflict.HasConflict)
            conflicts.Add(teacherConflict.Message);

        if (roomId > 0)
        {
            var roomConflict = await _conflictDetector.CheckRoomConflictAsync(
                roomId, day, startTime, endTime, semesterId);
            if (roomConflict.HasConflict)
                conflicts.Add(roomConflict.Message);
        }

        var classConflict = await _conflictDetector.CheckClassConflictAsync(
            classId, day, startTime, endTime, semesterId);
        if (classConflict.HasConflict)
            conflicts.Add(classConflict.Message);

        return new JsonResult(conflicts);
    }

    /// <summary>
    /// AJAX endpoint to get suggested time slots when conflict detected
    /// Called from JavaScript when user selects conflicted time
    /// </summary>
    public async Task<JsonResult> OnGetSuggestionsAsync(int semesterId, int teacherId, 
        int classBatchId, int? roomId, string day, string startTime, string endTime)
    {
        if (!TimeOnly.TryParse(startTime, out var start) || 
            !TimeOnly.TryParse(endTime, out var end) || end <= start)
        {
            return new JsonResult(new { success = false, message = "Invalid time format" });
        }

        try
        {
            var suggestions = await _conflictDetector.SuggestAlternativeSlotsAsync(
                teacherId, roomId, classBatchId, start, end, day, semesterId, maxSuggestions: 5);

            return new JsonResult(new 
            { 
                success = true,
                suggestions = suggestions.Select(s => new
                {
                    day = s.Day,
                    startTime = s.StartTime.ToString("HH:mm"),
                    endTime = s.EndTime.ToString("HH:mm"),
                    reason = s.Reason
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    private async Task<IActionResult> RedisplayFormAsync()
    {
        await LoadDropdownDataAsync();
        return Page();
    }

    private async Task LoadDropdownDataAsync()
    {
        // Load semesters
        Semesters = await _context.Semesters
            .Select(s => new SelectListItem(s.Name, s.Id.ToString()))
            .ToListAsync();

        // Load class batches
        ClassBatches = await _context.ClassBatches
            .Select(c => new SelectListItem($"{c.BatchName} (Sem {c.Semester})", c.Id.ToString()))
            .ToListAsync();

        // Load days
        Days = new List<SelectListItem>
        {
            new("Monday", "Monday"),
            new("Tuesday", "Tuesday"),
            new("Wednesday", "Wednesday"),
            new("Thursday", "Thursday"),
            new("Friday", "Friday"),
            new("Saturday", "Saturday")
        };

        // Load slot types
        SlotTypes = new List<SelectListItem>
        {
            new("Theory", "Theory"),
            new("Lab", "Lab"),
            new("GAP", "GAP"),
            new("Elective", "Elective")
        };

        // Load teachers
        Teachers = await _context.Teachers
            .Where(t => t.IsActive)
            .Select(t => new SelectListItem(
                $"{t.FullName} ({t.Initials})",
                t.Id.ToString()))
            .ToListAsync();

        // Load subjects
        Subjects = await _context.Subjects
            .Select(s => new SelectListItem($"{s.Name} ({s.Code})", s.Id.ToString()))
            .ToListAsync();

        // Load rooms
        Rooms = await _context.Rooms
            .Where(r => r.IsActive)
            .Select(r => new SelectListItem(
                $"{r.RoomNumber} ({r.RoomType}, Cap: {r.Capacity})",
                r.Id.ToString()))
            .ToListAsync();
    }

    private int GetDayOrder(string day) => day switch
    {
        "Monday" => 1,
        "Tuesday" => 2,
        "Wednesday" => 3,
        "Thursday" => 4,
        "Friday" => 5,
        "Saturday" => 6,
        _ => 7
    };
}
