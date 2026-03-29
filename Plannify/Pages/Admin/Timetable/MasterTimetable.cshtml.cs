using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Plannify.Application.Contracts;
using Plannify.Data;
using Plannify.Domain.Entities;

namespace Plannify.Pages.Admin.Timetable;

public class MasterTimetableModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly ITimetableService _timetableService;
    private readonly ISemesterService _semesterService;

    public List<ClassTimetableView> ClassTimetables { get; set; } = new();
    public List<TeacherTimetableView> TeacherTimetables { get; set; } = new();
    public List<RoomTimetableView> RoomTimetables { get; set; } = new();

    public int TotalSlots { get; set; }
    public int TotalClasses { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalRooms { get; set; }
    public int UtilizedRooms { get; set; }

    public MasterTimetableModel(AppDbContext context, ITimetableService timetableService, ISemesterService semesterService)
    {
        _context = context;
        _timetableService = timetableService;
        _semesterService = semesterService;
    }

    public async Task OnGetAsync()
    {
        // Get current active semester
        var semester = await _context.Semesters
            .FirstOrDefaultAsync(s => s.IsActive);

        if (semester == null)
            return;

        // Load all timetable data with relationships
        var semesterId = semester.Id;
        var allSlots = await _context.TimetableSlots
            .Include(t => t.ClassBatch)
            .Include(t => t.Subject)
            .Include(t => t.Teacher)
            .Include(t => t.Room)
            .Where(t => t.SemesterId == semesterId)
            .ToListAsync();

        // Build class-based timetables
        var classesByBatch = allSlots
            .GroupBy(t => new { t.ClassBatchId, BatchName = t.ClassBatch!.BatchName })
            .Select(g => new ClassTimetableView
            {
                ClassBatchId = g.Key.ClassBatchId,
                BatchName = g.Key.BatchName,
                TotalSlots = g.Count(),
                SlotsByDay = g.GroupBy(t => t.Day)
                    .ToDictionary(
                        dg => dg.Key,
                        dg => dg.OrderBy(t => t.StartTime)
                            .Select(t => new SlotDetail
                            {
                                SubjectName = t.Subject?.Name ?? "Unknown",
                                TeacherName = t.Teacher?.FullName ?? "Unknown",
                                RoomNumber = t.Room?.RoomNumber ?? "N/A",
                                StartTime = t.StartTime,
                                EndTime = t.EndTime,
                                SlotType = t.SlotType
                            })
                            .ToList()
                    )
            })
            .OrderBy(c => c.BatchName)
            .ToList();

        ClassTimetables = classesByBatch;

        // Build teacher-based timetables
        var teachersByTeacher = allSlots
            .GroupBy(t => new { t.TeacherId, TeacherName = t.Teacher!.FullName })
            .Select(g => new TeacherTimetableView
            {
                TeacherId = g.Key.TeacherId ?? 0,
                TeacherName = g.Key.TeacherName,
                TotalHours = g.Sum(t => (t.EndTime.Hour - t.StartTime.Hour) + (t.EndTime.Minute - t.StartTime.Minute) / 60.0m),
                SessionCount = g.Count(),
                SlotsByDay = g.GroupBy(t => t.Day)
                    .ToDictionary(
                        dg => dg.Key,
                        dg => dg.OrderBy(t => t.StartTime)
                            .Select(t => new SlotDetail
                            {
                                SubjectName = t.Subject?.Name ?? "Unknown",
                                ClassName = t.ClassBatch?.BatchName ?? "Unknown",
                                RoomNumber = t.Room?.RoomNumber ?? "N/A",
                                StartTime = t.StartTime,
                                EndTime = t.EndTime,
                                SlotType = t.SlotType
                            })
                            .ToList()
                    )
            })
            .OrderBy(t => t.TeacherName)
            .ToList();

        TeacherTimetables = teachersByTeacher;

        // Build room-based timetables
        var roomsByRoom = allSlots
            .GroupBy(t => new { t.RoomId, RoomNumber = t.Room!.RoomNumber, RoomType = t.Room.RoomType })
            .Select(g => new RoomTimetableView
            {
                RoomId = g.Key.RoomId ?? 0,
                RoomNumber = g.Key.RoomNumber,
                RoomType = g.Key.RoomType,
                TotalSlots = g.Count(),
                Utilization = Math.Round((g.Count() * 100.0) / (5 * 20), 2), // Approximation
                SlotsByDay = g.GroupBy(t => t.Day)
                    .ToDictionary(
                        dg => dg.Key,
                        dg => dg.OrderBy(t => t.StartTime)
                            .Select(t => new SlotDetail
                            {
                                ClassName = t.ClassBatch?.BatchName ?? "Unknown",
                                SubjectName = t.Subject?.Name ?? "Unknown",
                                TeacherName = t.Teacher?.FullName ?? "Unknown",
                                StartTime = t.StartTime,
                                EndTime = t.EndTime,
                                SlotType = t.SlotType ?? "Unknown"
                            })
                            .ToList()
                    )
            })
            .OrderBy(r => r.RoomNumber)
            .ToList();

        RoomTimetables = roomsByRoom;

        // Calculate statistics
        TotalSlots = allSlots.Count;
        TotalClasses = allSlots.Select(t => t.ClassBatchId).Distinct().Count();
        TotalTeachers = allSlots.Select(t => t.TeacherId).Distinct().Count();
        TotalRooms = allSlots.Select(t => t.RoomId).Distinct().Count();
        UtilizedRooms = RoomTimetables.Count;
    }
}

public class ClassTimetableView
{
    public int ClassBatchId { get; set; }
    public string BatchName { get; set; } = "";
    public int TotalSlots { get; set; }
    public Dictionary<string, List<SlotDetail>> SlotsByDay { get; set; } = new();
}

public class TeacherTimetableView
{
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = "";
    public decimal TotalHours { get; set; }
    public int SessionCount { get; set; }
    public Dictionary<string, List<SlotDetail>> SlotsByDay { get; set; } = new();
}

public class RoomTimetableView
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = "";
    public string RoomType { get; set; } = "";
    public int TotalSlots { get; set; }
    public double Utilization { get; set; }
    public Dictionary<string, List<SlotDetail>> SlotsByDay { get; set; } = new();
}

public class SlotDetail
{
    public string SubjectName { get; set; } = "";
    public string TeacherName { get; set; } = "";
    public string ClassName { get; set; } = "";
    public string RoomNumber { get; set; } = "";
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string SlotType { get; set; } = "";
}
