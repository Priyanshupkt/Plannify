using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Plannify.Data;

namespace Plannify.Pages.Admin;

[Authorize(Roles = "SuperAdmin,HOD")]
public class DashboardModel : PageModel
{
    private readonly AppDbContext _dbContext;

    public DashboardModel(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public int TeacherCount { get; set; }
    public int SubjectCount { get; set; }
    public int ClassCount { get; set; }
    public int SlotCount { get; set; }
    public int DepartmentCount { get; set; }
    public string CurrentSemester { get; set; } = "Semester 1";
    public string CurrentAcademicYear { get; set; } = "2025-26";

    public async Task OnGetAsync()
    {
        TeacherCount = await _dbContext.Teachers.CountAsync();
        SubjectCount = await _dbContext.Subjects.CountAsync();
        ClassCount = await _dbContext.ClassBatches.CountAsync();
        SlotCount = await _dbContext.TimetableSlots.CountAsync();
        DepartmentCount = await _dbContext.Departments.CountAsync();

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
