using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Plannify.Application.Contracts;
using Plannify.Data;
using Plannify.Models;
using Plannify.Services;
using System.Text.Json;

namespace Plannify.Pages.Admin;

public class DashboardModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly IConflictDetectorService _conflictDetector;
    private readonly ISubstitutionService _substitutionService;

    public DashboardModel(
        AppDbContext context,
        IConflictDetectorService conflictDetector,
        ISubstitutionService substitutionService)
    {
        _context = context;
        _conflictDetector = conflictDetector;
        _substitutionService = substitutionService;
    }

    // Statistics
    public int TotalTeachers { get; set; }
    public int TotalClasses { get; set; }
    public int TotalSubjects { get; set; }
    public int ActiveRooms { get; set; }
    public int TotalSlots { get; set; }
    public int ConflictCount { get; set; }
    public int PendingSubstitutions { get; set; }
    public int OverloadedTeachersCount { get; set; }

    // Charts data
    public string TeacherWorkloadChartData { get; set; } = "[]";
    public string RoomUtilizationChartData { get; set; } = "[]";

    // Alerts
    public List<string> Alerts { get; set; } = new();
    public List<AuditLog> RecentActivity { get; set; } = new();
    public List<TeacherOverload> OverloadedTeachers { get; set; } = new();

    public class TeacherOverload
    {
        public string TeacherName { get; set; } = string.Empty;
        public int WeeklyHours { get; set; }
        public int MaxHours { get; set; }
    }

    public async Task OnGetAsync()
    {
        // Load statistics
        TotalTeachers = await _context.Teachers.CountAsync(t => t.IsActive);
        TotalClasses = await _context.ClassBatches.CountAsync();
        TotalSubjects = await _context.Subjects.CountAsync();
        ActiveRooms = await _context.Rooms.CountAsync(r => r.IsActive);

        var activeSemester = await _context.Semesters.FirstOrDefaultAsync(s => s.IsActive);
        if (activeSemester != null)
        {
            TotalSlots = await _context.TimetableSlots.CountAsync(s => s.SemesterId == activeSemester.Id);
            ConflictCount = (await _conflictDetector.GetAllConflictsAsync(activeSemester.Id)).Count;
        }

        // Get pending substitutions using the service
        var activeSubstitutions = await _substitutionService.GetActiveAsync();
        if (activeSubstitutions.IsSuccess)
        {
            // Count substitutions from the past week to today
            var oneWeekAgo = DateOnly.FromDateTime(DateTime.Now.AddDays(-7));
            PendingSubstitutions = activeSubstitutions.Value
                ?.Where(s => s.Date >= oneWeekAgo)
                .Count() ?? 0;
        }

        // Load teacher workload for chart
        await LoadTeacherWorkloadChartAsync();

        // Load room utilization for chart
        await LoadRoomUtilizationChartAsync();

        // Load recent activity
        RecentActivity = await _context.AuditLogs
            .OrderByDescending(a => a.PerformedAt)
            .Take(10)
            .ToListAsync();

        // Find overloaded teachers
        var allTeachers = await _context.Teachers
            .Where(t => t.IsActive)
            .ToListAsync();

        if (activeSemester != null)
        {
            foreach (var teacher in allTeachers)
            {
                var slots = await _context.TimetableSlots
                    .Where(s => s.TeacherId == teacher.Id && s.SemesterId == activeSemester.Id && s.SlotType != "GAP")
                    .ToListAsync();

                int weeklyHours = slots.Sum(s => s.EndTime.Hour - s.StartTime.Hour);

                if (weeklyHours > teacher.MaxWeeklyHours)
                {
                    OverloadedTeachers.Add(new TeacherOverload
                    {
                        TeacherName = teacher.FullName,
                        WeeklyHours = weeklyHours,
                        MaxHours = teacher.MaxWeeklyHours
                    });
                }
            }
        }

        OverloadedTeachersCount = OverloadedTeachers.Count;

        // Generate alerts
        if (OverloadedTeachersCount > 0)
            Alerts.Add($"⚠ {OverloadedTeachersCount} teacher(s) are overloaded");

        if (ConflictCount > 0)
            Alerts.Add($"⚠ {ConflictCount} conflict(s) detected in current semester");

        if (activeSemester == null)
            Alerts.Add("⚠ No active semester configured");
    }

    private async Task LoadTeacherWorkloadChartAsync()
    {
        var activeSemester = await _context.Semesters.FirstOrDefaultAsync(s => s.IsActive);
        if (activeSemester == null)
            return;

        var teacherWorkloads = new List<object>();

        var teachers = await _context.Teachers
            .Where(t => t.IsActive)
            .OrderByDescending(t => t.TimetableSlots.Count(s => s.SemesterId == activeSemester.Id))
            .Take(10)
            .ToListAsync();

        foreach (var teacher in teachers)
        {
            var slots = await _context.TimetableSlots
                .Where(s => s.TeacherId == teacher.Id && s.SemesterId == activeSemester.Id && s.SlotType != "GAP")
                .ToListAsync();

            int hours = slots.Sum(s => s.EndTime.Hour - s.StartTime.Hour);
            teacherWorkloads.Add(new
            {
                name = teacher.Initials,
                actual = hours,
                max = teacher.MaxWeeklyHours
            });
        }

        TeacherWorkloadChartData = JsonSerializer.Serialize(teacherWorkloads);
    }

    private async Task LoadRoomUtilizationChartAsync()
    {
        var activeSemester = await _context.Semesters.FirstOrDefaultAsync(s => s.IsActive);
        if (activeSemester == null)
            return;

        var roomUtilization = new List<object>();

        var rooms = await _context.Rooms
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.TimetableSlots.Count(s => s.SemesterId == activeSemester.Id))
            .Take(5)
            .ToListAsync();

        foreach (var room in rooms)
        {
            int occupied = await _context.TimetableSlots
                .CountAsync(s => s.RoomId == room.Id && s.SemesterId == activeSemester.Id);

            int available = 36; // 6 days × 6 periods
            int utilization = available > 0 ? (occupied * 100) / available : 0;

            roomUtilization.Add(new
            {
                name = room.RoomNumber,
                occupied = occupied,
                available = available,
                utilization = utilization
            });
        }

        RoomUtilizationChartData = JsonSerializer.Serialize(roomUtilization);
    }
}
