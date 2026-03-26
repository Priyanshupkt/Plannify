using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Plannify.Data;
using Plannify.Models;

namespace Plannify.Pages.Teacher;

[Authorize(Roles = "Teacher")]
public class MyTimetableModel : PageModel
{
    private readonly AppDbContext _context;

    public MyTimetableModel(AppDbContext context)
    {
        _context = context;
    }

    public List<SelectListItem> Semesters { get; set; } = new();
    public Plannify.Models.Teacher? CurrentTeacher { get; set; }
    public Semester? CurrentSemester { get; set; }
    public Dictionary<string, Dictionary<string, TimetableSlot?>> Grid { get; set; } = new();
    public List<string> Days { get; set; } = new();
    public List<string> TimeRanges { get; set; } = new();

    public async Task OnGetAsync(int? semesterId)
    {
        var userName = User.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
            return;

        CurrentTeacher = await _context.Teachers
            .FirstOrDefaultAsync(t => t.EmployeeCode == userName || t.Email == userName);

        if (CurrentTeacher == null)
            return;

        await LoadSemestersAsync();

        if (!semesterId.HasValue)
        {
            var activeSemester = await _context.Semesters.FirstOrDefaultAsync(s => s.IsActive);
            semesterId = activeSemester?.Id;
        }

        if (semesterId.HasValue)
        {
            CurrentSemester = await _context.Semesters.FindAsync(semesterId);
            if (CurrentSemester != null)
                await BuildGridAsync(CurrentTeacher.Id, semesterId.Value);
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

        var timeSlots = slots
            .Select(s => (s.StartTime, s.EndTime))
            .Distinct()
            .OrderBy(t => t.StartTime)
            .Select(t => $"{t.StartTime:HH:mm}-{t.EndTime:HH:mm}")
            .ToList();

        TimeRanges = timeSlots;

        Grid = new();
        foreach (var day in Days)
        {
            Grid[day] = new();
            foreach (var timeRange in TimeRanges)
            {
                Grid[day][timeRange] = null;
            }
        }

        foreach (var slot in slots)
        {
            var timeRange = $"{slot.StartTime:HH:mm}-{slot.EndTime:HH:mm}";
            if (Grid.ContainsKey(slot.Day) && Grid[slot.Day].ContainsKey(timeRange))
            {
                Grid[slot.Day][timeRange] = slot;
            }
        }
    }

    private async Task LoadSemestersAsync()
    {
        Semesters = await _context.Semesters
            .OrderByDescending(s => s.IsActive)
            .ThenByDescending(s => s.StartDate)
            .Select(s => new SelectListItem($"{s.Name} {(s.IsActive ? "(Active)" : "")}", s.Id.ToString()))
            .ToListAsync();
    }
}
