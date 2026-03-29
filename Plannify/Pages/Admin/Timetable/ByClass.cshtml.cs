using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Plannify.Application.Contracts;
using Plannify.Data;
using Plannify.Domain.Entities;
using Plannify.Services;

namespace Plannify.Pages.Admin.Timetable;

[Authorize]
public class ByClassModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly PdfExportService _pdfService;
    private readonly IClassBatchService _classBatchService;
    private readonly ISemesterService _semesterService;

    public ByClassModel(AppDbContext context, PdfExportService pdfService, IClassBatchService classBatchService, ISemesterService semesterService)
    {
        _context = context;
        _pdfService = pdfService;
        _classBatchService = classBatchService;
        _semesterService = semesterService;
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
    
    // Summary statistics
    public int TotalTeachingSlots { get; set; }
    public int TotalGapSlots { get; set; }
    public int TotalFreeSlots { get; set; }

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

        // Get unique time ranges across all slots — format as "HH:mm – HH:mm", sorted
        var timeSlots = slots
            .Select(s => (s.StartTime, s.EndTime))
            .Distinct()
            .OrderBy(t => t.StartTime)
            .Select(t => $"{t.StartTime:HH:mm} – {t.EndTime:HH:mm}")
            .ToList();

        TimeRanges = timeSlots;

        // Build grid dictionary: [Day][TimeRange] = slot or null
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
            var timeRange = $"{slot.StartTime:HH:mm} – {slot.EndTime:HH:mm}";
            if (Grid.ContainsKey(slot.Day) && Grid[slot.Day].ContainsKey(timeRange))
            {
                Grid[slot.Day][timeRange] = slot;
            }
        }

        // Calculate summary statistics
        TotalTeachingSlots = slots.Count(s => s.SlotType != "GAP");
        TotalGapSlots = slots.Count(s => s.SlotType == "GAP");
        int totalCells = Days.Count * TimeRanges.Count;
        TotalFreeSlots = totalCells - slots.Count;
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
        var classBatch = await _context.ClassBatches.FirstOrDefaultAsync(c => c.Id == classId);
        var semester = await _context.Semesters.FirstOrDefaultAsync(s => s.Id == semesterId);

        if (classBatch == null || semester == null)
            return NotFound();

        CurrentClass = classBatch;
        CurrentSemester = semester;
        await BuildGridAsync(classId, semesterId);

        var fileName = $"Timetable_{classBatch.BatchName}_{semester.Name}_{DateTime.Now:yyyy-MM-dd}.pdf";
        var bytes = _pdfService.GenerateClassTimetablePdf(
            classBatch.BatchName,
            semester.Name,
            Days,
            TimeRanges,
            Grid);

        return File(bytes, "application/pdf", fileName);
    }
}
