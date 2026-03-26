using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Plannify.Data;
using Plannify.Models;

namespace Plannify.Pages.Admin.Timetable;

[Authorize(Roles = "Admin,HOD")]
public class ByRoomModel : PageModel
{
    private readonly AppDbContext _context;

    public ByRoomModel(AppDbContext context)
    {
        _context = context;
    }

    public List<SelectListItem> Rooms { get; set; } = new();
    public List<SelectListItem> Semesters { get; set; } = new();

    public Room? CurrentRoom { get; set; }
    public Semester? CurrentSemester { get; set; }
    public Dictionary<string, Dictionary<string, TimetableSlot?>> Grid { get; set; } = new();
    public List<string> Days { get; set; } = new();
    public List<string> TimeRanges { get; set; } = new();

    public int TotalOccupiedPeriods { get; set; }
    public int TotalAvailablePeriods { get; set; }
    public double UtilizationPercentage { get; set; }
    public string PeakUsageDay { get; set; } = string.Empty;
    public Dictionary<string, int> PeriodsPerDay { get; set; } = new();

    public async Task OnGetAsync(int? roomId, int? semesterId)
    {
        await LoadDropdownsAsync();

        if (roomId.HasValue && semesterId.HasValue)
        {
            CurrentRoom = await _context.Rooms.FindAsync(roomId);
            CurrentSemester = await _context.Semesters.FindAsync(semesterId);

            if (CurrentRoom != null && CurrentSemester != null)
            {
                await BuildGridAsync(roomId.Value, semesterId.Value);
            }
        }
    }

    private async Task BuildGridAsync(int roomId, int semesterId)
    {
        var slots = await _context.TimetableSlots
            .Where(s => s.RoomId == roomId && s.SemesterId == semesterId)
            .Include(s => s.Subject)
            .Include(s => s.ClassBatch)
            .Include(s => s.Teacher)
            .ToListAsync();

        Days = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
        TotalAvailablePeriods = 36; // 6 days × 6 periods

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

        // Calculate utilization statistics
        TotalOccupiedPeriods = slots.Count;
        UtilizationPercentage = TotalAvailablePeriods > 0 ? (double)TotalOccupiedPeriods / TotalAvailablePeriods * 100 : 0;

        // Calculate periods per day to find peak
        PeriodsPerDay = new();
        int maxPeriods = 0;
        string peakDay = string.Empty;

        foreach (var day in Days)
        {
            int periodsThisDay = slots.Count(s => s.Day == day);
            PeriodsPerDay[day] = periodsThisDay;

            if (periodsThisDay > maxPeriods)
            {
                maxPeriods = periodsThisDay;
                peakDay = day;
            }
        }

        PeakUsageDay = maxPeriods > 0 ? peakDay : "No usage";
    }

    private async Task LoadDropdownsAsync()
    {
        Rooms = await _context.Rooms
            .Where(r => r.IsActive)
            .OrderBy(r => r.RoomNumber)
            .Select(r => new SelectListItem($"{r.RoomNumber} ({r.RoomType}, Cap: {r.Capacity})", r.Id.ToString()))
            .ToListAsync();

        Semesters = await _context.Semesters
            .OrderByDescending(s => s.IsActive)
            .ThenByDescending(s => s.StartDate)
            .Select(s => new SelectListItem($"{s.Name} {(s.IsActive ? "(Active)" : "")}", s.Id.ToString()))
            .ToListAsync();
    }

    public IActionResult OnPostPrintPdf(int roomId, int semesterId)
    {
        // Placeholder for PDF export
        return RedirectToPage(new { roomId, semesterId });
    }
}
