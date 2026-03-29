using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Plannify.Application.Contracts;
using Plannify.Data;
using Plannify.Models;

namespace Plannify.Pages.Admin.Teachers;

[Authorize(Roles = "Admin")]
public class ProfileModel : PageModel
{
    private readonly ITeacherService _teacherService;
    private readonly ISemesterService _semesterService;
    private readonly ITimetableSlotService _timetableSlotService;
    private readonly AppDbContext _context;

    public ProfileModel(ITeacherService teacherService, ISemesterService semesterService, 
        ITimetableSlotService timetableSlotService, AppDbContext context)
    {
        _teacherService = teacherService;
        _semesterService = semesterService;
        _timetableSlotService = timetableSlotService;
        _context = context;
    }

    public Models.Teacher? Teacher { get; set; }
    public Semester? ActiveSemester { get; set; }
    public decimal CurrentHours { get; set; } = 0;
    public List<Subject> AssignedSubjects { get; set; } = new();
    public List<TimetableSlot> TimetableSlots { get; set; } = new();
    public Dictionary<int, int> SubjectClassCount { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var teacherResult = await _teacherService.GetByIdAsync(id);
        if (!teacherResult.IsSuccess || teacherResult.Value == null)
        {
            return NotFound();
        }

        Teacher = await _context.Teachers.Include(t => t.Department).FirstOrDefaultAsync(t => t.Id == id);
        if (Teacher == null)
        {
            return NotFound();
        }

        var semesterResult = await _semesterService.GetCurrentSemesterAsync();
        if (semesterResult.IsSuccess && semesterResult.Value != null)
        {
            ActiveSemester = await _context.Semesters.FirstOrDefaultAsync(s => s.Id == semesterResult.Value.Id);

            if (ActiveSemester != null)
            {
                var semesterId = ActiveSemester.Id;
                TimetableSlots = await _context.TimetableSlots
                    .Include(t => t.Subject)
                    .Include(t => t.ClassBatch)
                    .Include(t => t.Room)
                    .Where(t => t.TeacherId == id && t.SemesterId == semesterId)
                    .ToListAsync();

                foreach (var slot in TimetableSlots)
                {
                    var hours = (slot.EndTime.Hour - slot.StartTime.Hour) + 
                               ((slot.EndTime.Minute - slot.StartTime.Minute) / 60m);
                    CurrentHours += hours;
                }

                var subjectIds = TimetableSlots
                    .Where(t => t.SubjectId.HasValue)
                    .Select(t => t.SubjectId!.Value)
                    .Distinct()
                    .ToList();

                AssignedSubjects = await _context.Subjects
                    .Where(s => subjectIds.Contains(s.Id))
                    .ToListAsync();

                foreach (var subject in AssignedSubjects)
                {
                    var classCount = TimetableSlots
                        .Where(t => t.SubjectId == subject.Id && t.TeacherId == id)
                        .Select(t => t.ClassBatchId)
                        .Distinct()
                        .Count();

                    SubjectClassCount[subject.Id] = classCount;
                }
            }
        }

        return Page();
    }
}
