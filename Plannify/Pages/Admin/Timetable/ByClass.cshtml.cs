using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Plannify.Data;
using Plannify.Models;
using Plannify.Services;

namespace Plannify.Pages.Admin.Timetable;

[Authorize(Roles = "Admin,HOD")]
public class ByClassModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly TimetableExportService _exportService;

    public ByClassModel(AppDbContext context, TimetableExportService exportService)
    {
        _context = context;
        _exportService = exportService;
    }

    public List<SelectListItem> ClassBatches { get; set; } = new();
    public List<SelectListItem> Semesters { get; set; } = new();

    public ClassBatch? CurrentClass { get; set; }
    public Semester? CurrentSemester { get; set; }
    public Dictionary<string, Dictionary<string, TimetableSlot?>> Grid { get; set; } = new();
    public List<string> Days { get; set; } = new();
    public List<string> TimeRanges { get; set; } = new();

    public Dictionary<string, int> PeriodsPerDay { get; set; } = new();
    public Dictionary<string, int> FreePeriods { get; set; } = new();
    public int TotalWeeklyPeriods { get; set; }

    public async Task OnGetAsync(int? classId, int? semesterId)
    {
        await LoadDropdownsAsync();

        if (classId.HasValue && semesterId.HasValue)
        {
            CurrentClass = await _context.ClassBatches.FindAsync(classId);
            CurrentSemester = await _context.Semesters.FindAsync(semesterId);

            if (CurrentClass != null && CurrentSemester != null)
            {
                await BuildGridAsync(classId.Value, semesterId.Value);
            }
        }
    }

    private async Task BuildGridAsync(int classId, int semesterId)
    {
        var slots = await _context.TimetableSlots
            .Where(s => s.ClassBatchId == classId && s.SemesterId == semesterId)
            .Include(s => s.Subject)
            .Include(s => s.Teacher)
            .Include(s => s.Room)
            .ToListAsync();

        Days = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

        // Get unique time ranges across all slots
        var timeSlots = slots
            .Select(s => (s.StartTime, s.EndTime))
            .Distinct()
            .OrderBy(t => t.StartTime)
            .Select(t => $"{t.StartTime:HH:mm}-{t.EndTime:HH:mm}")
            .ToList();

        TimeRanges = timeSlots;

        // Build grid dictionary
        Grid = new();
        foreach (var day in Days)
        {
            Grid[day] = new();
            foreach (var timeRange in TimeRanges)
            {
                Grid[day][timeRange] = null;
            }
        }

        // Fill grid with slots
        foreach (var slot in slots)
        {
            var timeRange = $"{slot.StartTime:HH:mm}-{slot.EndTime:HH:mm}";
            if (Grid.ContainsKey(slot.Day) && Grid[slot.Day].ContainsKey(timeRange))
            {
                Grid[slot.Day][timeRange] = slot;
            }
        }

        // Calculate statistics
        PeriodsPerDay = new();
        FreePeriods = new();
        int totalSlots = 0;

        foreach (var day in Days)
        {
            int periodsThisDay = Grid[day].Values.Count(s => s != null);
            int freeThisDay = Grid[day].Values.Count(s => s == null || s.SlotType == "GAP");

            PeriodsPerDay[day] = periodsThisDay;
            FreePeriods[day] = freeThisDay;
            totalSlots += periodsThisDay;
        }

        TotalWeeklyPeriods = totalSlots;
    }

    private async Task LoadDropdownsAsync()
    {
        ClassBatches = await _context.ClassBatches
            .OrderBy(c => c.BatchName)
            .Select(c => new SelectListItem($"{c.BatchName} (Sem {c.Semester})", c.Id.ToString()))
            .ToListAsync();

        Semesters = await _context.Semesters
            .OrderByDescending(s => s.IsActive)
            .ThenByDescending(s => s.StartDate)
            .Select(s => new SelectListItem($"{s.Name} {(s.IsActive ? "(Active)" : "")}", s.Id.ToString()))
            .ToListAsync();
    }

    public IActionResult OnPostPrintPdf(int classId, int semesterId)
    {
        // Placeholder for PDF export
        return RedirectToPage(new { classId, semesterId });
    }

    public async Task<IActionResult> OnPostExportPdfAsync(int classId, int semesterId)
    {
        var classBatch = await _context.ClassBatches.FindAsync(classId);
        var semester = await _context.Semesters
            .Include(s => s.AcademicYear)
            .FirstOrDefaultAsync(s => s.Id == semesterId);

        if (classBatch == null || semester == null)
            return NotFound();

        CurrentClass = classBatch;
        CurrentSemester = semester;
        await BuildGridAsync(classId, semesterId);

        var fileName = $"Timetable_{classBatch.BatchName}_{semester.Name}_{DateTime.Now:yyyy-MM-dd}.pdf";
        var bytes = _exportService.ExportClassTimetablePdf(
            classBatch.BatchName,
            semester.Name,
            semester.AcademicYear?.Name ?? "N/A",
            Grid,
            TimeRanges,
            Days);

        return File(bytes, "application/pdf", fileName);
    }

    public IActionResult OnPostExportExcel(int classId, int semesterId)
    {
        // Placeholder for Excel export
        return RedirectToPage(new { classId, semesterId });
    }

    public async Task<IActionResult> OnPostExportExcelAsync(int classId, int semesterId)
    {
        var classBatch = await _context.ClassBatches.FindAsync(classId);
        var semester = await _context.Semesters.FindAsync(semesterId);

        if (classBatch == null || semester == null)
            return NotFound();

        CurrentClass = classBatch;
        CurrentSemester = semester;
        await BuildGridAsync(classId, semesterId);

        var fileName = $"Timetable_{classBatch.BatchName}_{semester.Name}_{DateTime.Now:yyyy-MM-dd}.xlsx";
        var bytes = _exportService.ExportClassTimetableExcel(
            classBatch.BatchName,
            semester.Name,
            Grid,
            TimeRanges,
            Days);

        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
