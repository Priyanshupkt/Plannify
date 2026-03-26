using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Plannify.Data;
using Plannify.Models;

namespace Plannify.Pages.Teacher;

[Authorize(Roles = "Teacher")]
public class DashboardModel : PageModel
{
    private readonly AppDbContext _dbContext;

    public DashboardModel(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public int ThisWeekSlots { get; set; }
    public decimal TotalHours { get; set; }
    public int ClassCount { get; set; }
    public int PendingSubstitutions { get; set; }
    public string CurrentSemester { get; set; } = "Semester 1";
    public string CurrentAcademicYear { get; set; } = "2025-26";
    public string Department { get; set; } = "Not Assigned";
    public List<TimetableSlot> WeekSlots { get; set; } = new();

    public async Task OnGetAsync()
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);
        if (user?.TeacherId == null)
        {
            return;
        }

        var teacher = await _dbContext.Teachers
            .Include(t => t.Department)
            .FirstOrDefaultAsync(t => t.Id == user.TeacherId);

        if (teacher != null)
        {
            Department = teacher.Department?.Name ?? "Not Assigned";
        }

        // Get this week's slots
        var today = DateTime.Now;
        var dayOfWeek = (int)today.DayOfWeek;
        var startOfWeek = today.AddDays(-dayOfWeek);
        var endOfWeek = startOfWeek.AddDays(7);

        WeekSlots = await _dbContext.TimetableSlots
            .Where(s => s.TeacherId == user.TeacherId && s.SlotType != "GAP")
            .Include(s => s.Subject)
            .Include(s => s.ClassBatch)
            .Include(s => s.Room)
            .OrderBy(s => s.Day)
            .ThenBy(s => s.StartTime)
            .ToListAsync();

        ThisWeekSlots = WeekSlots.Count;
        ClassCount = await _dbContext.TimetableSlots
            .Where(s => s.TeacherId == user.TeacherId && s.SlotType != "GAP")
            .Select(s => s.ClassBatchId)
            .Distinct()
            .CountAsync();

        PendingSubstitutions = await _dbContext.SubstitutionRecords
            .Where(s => s.OriginalTeacherId == user.TeacherId)
            .CountAsync();

        // Calculate hours
        foreach (var slot in WeekSlots)
        {
            var duration = slot.EndTime.Hour - slot.StartTime.Hour
                + (slot.EndTime.Minute - slot.StartTime.Minute) / 60m;
            TotalHours += duration;
        }

        var currentYear = await _dbContext.AcademicYears
            .Where(ay => ay.IsActive)
            .FirstOrDefaultAsync();

        if (currentYear != null)
        {
            CurrentAcademicYear = currentYear.YearLabel;

            var currentSemester = await _dbContext.Semesters
                .Where(s => s.AcademicYearId == currentYear.Id && s.IsActive)
                .FirstOrDefaultAsync();

            if (currentSemester != null)
            {
                CurrentSemester = currentSemester.Name;
            }
        }
    }
}
