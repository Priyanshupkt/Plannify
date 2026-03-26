using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Plannify.Data;
using Plannify.Models;
using System.Text.Json.Serialization;

namespace Plannify.Pages.Teacher;

[Authorize(Roles = "Admin")]
public class MyWorkloadModel : PageModel
{
    private readonly AppDbContext _context;

    public MyWorkloadModel(AppDbContext context)
    {
        _context = context;
    }

    public Plannify.Models.Teacher? CurrentTeacher { get; set; }
    public Semester? ActiveSemester { get; set; }

    public int WeeklyTeachingHours { get; set; }
    public int MaxWeeklyHours { get; set; }
    public int TheoryHours { get; set; }
    public int LabHours { get; set; }
    public int GapHours { get; set; }
    public string BusiestDay { get; set; } = string.Empty;

    public List<DayWorkload> DayBreakdown { get; set; } = new();
    public List<SubjectWorkload> SubjectBreakdown { get; set; } = new();

    public class DayWorkload
    {
        public string Day { get; set; } = string.Empty;
        public int TeachingHours { get; set; }
        public int GapHours { get; set; }
        public int Subjects { get; set; }
        public int Classes { get; set; }
    }

    public class SubjectWorkload
    {
        public string SubjectName { get; set; } = string.Empty;
        public string Classes { get; set; } = string.Empty;
        public int SessionsPerWeek { get; set; }
        public int TotalHours { get; set; }
    }

    public async Task OnGetAsync()
    {
        var userName = User.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
            return;

        CurrentTeacher = await _context.Teachers
            .FirstOrDefaultAsync(t => t.EmployeeCode == userName || t.Email == userName);

        if (CurrentTeacher == null)
            return;

        ActiveSemester = await _context.Semesters
            .FirstOrDefaultAsync(s => s.IsActive);

        if (ActiveSemester == null)
            return;

        MaxWeeklyHours = CurrentTeacher.MaxWeeklyHours;

        var slots = await _context.TimetableSlots
            .Where(s => s.TeacherId == CurrentTeacher.Id && s.SemesterId == ActiveSemester.Id)
            .Include(s => s.Subject)
            .Include(s => s.ClassBatch)
            .ToListAsync();

        // Calculate workload statistics
        WeeklyTeachingHours = 0;
        TheoryHours = 0;
        LabHours = 0;
        GapHours = 0;

        foreach (var slot in slots)
        {
            int duration = (slot.EndTime.Hour - slot.StartTime.Hour);

            if (slot.SlotType == "Theory")
            {
                TheoryHours += duration;
                WeeklyTeachingHours += duration;
            }
            else if (slot.SlotType == "Lab")
            {
                LabHours += duration;
                WeeklyTeachingHours += duration;
            }
            else if (slot.SlotType == "GAP")
            {
                GapHours += duration;
            }
        }

        // Day breakdown
        var days = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
        var dayHours = new Dictionary<string, int>();

        foreach (var day in days)
        {
            var daySlots = slots.Where(s => s.Day == day).ToList();
            var dayTeachingHours = daySlots
                .Where(s => s.SlotType != "GAP")
                .Sum(s => s.EndTime.Hour - s.StartTime.Hour);

            var dayGapHours = daySlots
                .Where(s => s.SlotType == "GAP")
                .Sum(s => s.EndTime.Hour - s.StartTime.Hour);

            DayBreakdown.Add(new DayWorkload
            {
                Day = day,
                TeachingHours = dayTeachingHours,
                GapHours = dayGapHours,
                Subjects = daySlots.Select(s => s.SubjectId).Distinct().Count(),
                Classes = daySlots.Select(s => s.ClassBatchId).Distinct().Count()
            });

            if (dayTeachingHours > dayHours.GetValueOrDefault(BusiestDay, 0))
            {
                BusiestDay = day;
                dayHours[day] = dayTeachingHours;
            }
        }

        // Subject breakdown
        var subjectGroups = slots
            .Where(s => s.SubjectId.HasValue)
            .GroupBy(s => s.SubjectId)
            .ToList();

        foreach (var group in subjectGroups)
        {
            var subject = group.First().Subject;
            var classes = string.Join(", ", group.Select(s => s.ClassBatch?.BatchName).Distinct());
            var hours = group.Sum(s => s.EndTime.Hour - s.StartTime.Hour);

            SubjectBreakdown.Add(new SubjectWorkload
            {
                SubjectName = subject?.Name ?? "Unknown",
                Classes = classes,
                SessionsPerWeek = group.Count(),
                TotalHours = hours
            });
        }
    }
}
