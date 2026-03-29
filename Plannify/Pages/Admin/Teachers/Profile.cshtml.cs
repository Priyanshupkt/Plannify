using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Plannify.Data;
using Plannify.Models;
using Plannify.Services;

namespace Plannify.Pages.Admin.Teachers;

public class ProfileModel : PageModel
{
    private readonly AppDbContext _dbContext;
    private readonly AuditService _auditService;

    public ProfileModel(AppDbContext dbContext, AuditService auditService)
    {
        _dbContext = dbContext;
        _auditService = auditService;
    }

    public Plannify.Models.Teacher? Teacher { get; set; }
    public Semester? ActiveSemester { get; set; }
    public decimal CurrentHours { get; set; } = 0;
    public List<Subject> AssignedSubjects { get; set; } = new();
    public List<TimetableSlot> TimetableSlots { get; set; } = new();
    public Dictionary<int, int> SubjectClassCount { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Teacher = await _dbContext.Teachers.Include(t => t.Department).FirstOrDefaultAsync(t => t.Id == id);
        if (Teacher == null)
        {
            return NotFound();
        }

        ActiveSemester = await _dbContext.Semesters.FirstOrDefaultAsync(s => s.IsActive);

        if (ActiveSemester != null)
        {
            TimetableSlots = await _dbContext.TimetableSlots
                .Include(t => t.Subject)
                .Include(t => t.ClassBatch)
                .Include(t => t.Room)
                .Where(t => t.TeacherId == id && t.SemesterId == ActiveSemester.Id)
                .ToListAsync();

            foreach (var slot in TimetableSlots)
            {
                var hours = (slot.EndTime.Hour - slot.StartTime.Hour) + 
                           ((slot.EndTime.Minute - slot.StartTime.Minute) / 60m);
                CurrentHours += hours;
            }

            var subjectIds = TimetableSlots
                .Where(t => t.SubjectId.HasValue)
                .Select(t => t.SubjectId.Value)
                .Distinct()
                .ToList();

            AssignedSubjects = await _dbContext.Subjects
                .Where(s => subjectIds.Contains(s.Id))
                .ToListAsync();

            foreach (var subject in AssignedSubjects)
            {
                var classCount = await _dbContext.TimetableSlots
                    .Where(t => t.SubjectId == subject.Id && t.TeacherId == id && t.SemesterId == ActiveSemester.Id)
                    .Select(t => t.ClassBatchId)
                    .Distinct()
                    .CountAsync();

                SubjectClassCount[subject.Id] = classCount;
            }
        }

        return Page();
    }
}
