using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Plannify.Application.Contracts;
using Plannify.Data;
using Plannify.Domain.Entities;
using Plannify.Services;

namespace Plannify.Pages.Admin.Timetable;

public class ByTeacherModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly PdfExportService _pdfService;
    private readonly ITeacherService _teacherService;
    private readonly ISemesterService _semesterService;

    public ByTeacherModel(AppDbContext context, PdfExportService pdfService, ITeacherService teacherService, ISemesterService semesterService)
    {
        _context = context;
        _pdfService = pdfService;
        _teacherService = teacherService;
        _semesterService = semesterService;
    }

    public List<SelectListItem> Teachers { get; set; } = new();
    public List<SelectListItem> Semesters { get; set; } = new();

    public Teacher? CurrentTeacher { get; set; }
    public Semester? CurrentSemester { get; set; }
    public Dictionary<string, Dictionary<string, TimetableSlot?>> Grid { get; set; } = new();
    public List<string> Days { get; set; } = new();
    public List<string> TimeRanges { get; set; } = new();

    public int TotalTeachingHours { get; set; }
    public int MaxWeeklyHours { get; set; }
    public int TheoryHours { get; set; }
    public int LabHours { get; set; }
    public int GapHours { get; set; }
    public List<string> DaysWithNoClasses { get; set; } = new();

    public async Task OnGetAsync(int? teacherId, int? semesterId)
    {
        await LoadDropdownsAsync();

        if (teacherId.HasValue && semesterId.HasValue)
        {
            CurrentTeacher = await _context.Teachers.FindAsync(teacherId);
            CurrentSemester = await _context.Semesters.FindAsync(semesterId);

            if (CurrentTeacher != null && CurrentSemester != null)
            {
                await BuildGridAsync(teacherId.Value, semesterId.Value);
            }
        }
    }

    private async Task BuildGridAsync(int teacherId, int semesterId)
    {
        var slots = await _context.TimetableSlots
            .Where(s => s.TeacherId == teacherId && s.SemesterId == semesterId)
            .Include(s => s.Subject)
            .Include(s => s.ClassBatch)
            .Include(s => s.Room)
            .ToListAsync();

        Days = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
        MaxWeeklyHours = CurrentTeacher?.MaxWeeklyHours ?? 18;

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

        // Calculate workload statistics
        TotalTeachingHours = 0;
        TheoryHours = 0;
        LabHours = 0;
        GapHours = 0;
        DaysWithNoClasses = new();

        foreach (var slot in slots)
        {
            int duration = (slot.EndTime.Hour - slot.StartTime.Hour);
            TotalTeachingHours += duration;

            if (slot.SlotType == "Theory")
                TheoryHours += duration;
            else if (slot.SlotType == "Lab")
                LabHours += duration;
            else if (slot.SlotType == "GAP")
                GapHours += duration;
        }

        // Identify days with no classes
        var daysWithSlots = slots.Select(s => s.Day).Distinct().ToHashSet();
        foreach (var day in Days)
        {
            if (!daysWithSlots.Contains(day))
                DaysWithNoClasses.Add(day);
        }
    }

    private async Task LoadDropdownsAsync()
    {
        Teachers = await _context.Teachers
            .Where(t => t.IsActive)
            .OrderBy(t => t.FullName)
            .Select(t => new SelectListItem($"{t.FullName} ({t.Initials})", t.Id.ToString()))
            .ToListAsync();

        Semesters = await _context.Semesters
            .OrderByDescending(s => s.IsActive)
            .ThenByDescending(s => s.StartDate)
            .Select(s => new SelectListItem($"{s.Name} {(s.IsActive ? "(Active)" : "")}", s.Id.ToString()))
            .ToListAsync();
    }

    public IActionResult OnPostPrintPdf(int teacherId, int semesterId)
    {
        // Placeholder for PDF export
        return RedirectToPage(new { teacherId, semesterId });
    }

    public async Task<IActionResult> OnPostExportPdfAsync(int teacherId, int semesterId)
    {
        var teacher = await _context.Teachers.FindAsync(teacherId);
        var semester = await _context.Semesters
            .Include(s => s.AcademicYear)
            .FirstOrDefaultAsync(s => s.Id == semesterId);

        if (teacher == null || semester == null)
            return NotFound();

        CurrentTeacher = teacher;
        CurrentSemester = semester;
        await BuildGridAsync(teacherId, semesterId);

        var fileName = $"Timetable_{teacher.FullName}_{semester.Name}_{DateTime.Now:yyyy-MM-dd}.pdf";
        var bytes = _pdfService.GenerateTeacherTimetablePdf(
            teacher.FullName,
            semester.Name,
            Days,
            TimeRanges,
            Grid);

        return File(bytes, "application/pdf", fileName);
    }
}
