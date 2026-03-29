using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Plannify.Data;
using Plannify.Models;
using Plannify.Services;

namespace Plannify.Pages.Admin.AcademicYears;

public class SemestersModel : PageModel
{
    private readonly AppDbContext _dbContext;
    private readonly AuditService _auditService;

    public SemestersModel(AppDbContext dbContext, AuditService auditService)
    {
        _dbContext = dbContext;
        _auditService = auditService;
    }

    [BindProperty]
    public Semester NewSemester { get; set; } = new();

    public AcademicYear? AcademicYear { get; set; }
    public List<Semester> Semesters { get; set; } = new();
    public Dictionary<int, int> SlotCounts { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int yearId)
    {
        AcademicYear = await _dbContext.AcademicYears.FindAsync(yearId);
        if (AcademicYear == null)
        {
            return NotFound();
        }

        Semesters = await _dbContext.Semesters
            .Where(s => s.AcademicYearId == yearId)
            .OrderBy(s => s.SemesterNumber)
            .ToListAsync();

        foreach (var semester in Semesters)
        {
            var count = await _dbContext.TimetableSlots.CountAsync(t => t.SemesterId == semester.Id);
            SlotCounts[semester.Id] = count;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync(int yearId)
    {
        AcademicYear = await _dbContext.AcademicYears.FindAsync(yearId);
        if (AcademicYear == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await OnGetAsync(yearId);
            return Page();
        }

        var semesterExists = await _dbContext.Semesters.AnyAsync(s =>
            s.AcademicYearId == yearId && s.SemesterNumber == NewSemester.SemesterNumber);
        if (semesterExists)
        {
            TempData["Error"] = $"Semester {NewSemester.SemesterNumber} already exists for this academic year.";
            await OnGetAsync(yearId);
            return Page();
        }

        NewSemester.AcademicYearId = yearId;
        NewSemester.IsActive = false;
        _dbContext.Semesters.Add(NewSemester);
        await _dbContext.SaveChangesAsync();

        await _auditService.LogAsync("CREATE", "Semester", NewSemester.Id.ToString(),
            null, $"Name: {NewSemester.Name}, Number: {NewSemester.SemesterNumber}, Year: {AcademicYear.YearLabel}");

        TempData["Success"] = $"Semester '{NewSemester.Name}' added successfully.";
        return RedirectToPage(new { yearId });
    }

    public async Task<IActionResult> OnPostSetActiveAsync(int id)
    {
        var semester = await _dbContext.Semesters.Include(s => s.AcademicYear).FirstOrDefaultAsync(s => s.Id == id);
        if (semester == null)
        {
            TempData["Error"] = "Semester not found.";
            return RedirectToPage();
        }

        var currentlyActive = await _dbContext.Semesters.FirstOrDefaultAsync(s => s.IsActive && s.AcademicYearId == semester.AcademicYearId);
        if (currentlyActive != null)
        {
            currentlyActive.IsActive = false;
            _dbContext.Semesters.Update(currentlyActive);
            await _auditService.LogAsync("UPDATE", "Semester", currentlyActive.Id.ToString(),
                "IsActive: true", "IsActive: false");
        }

        semester.IsActive = true;
        _dbContext.Semesters.Update(semester);
        await _dbContext.SaveChangesAsync();

        await _auditService.LogAsync("UPDATE", "Semester", semester.Id.ToString(),
            "IsActive: false", "IsActive: true");

        TempData["Success"] = $"Semester '{semester.Name}' is now active.";
        return RedirectToPage(new { yearId = semester.AcademicYearId });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var semester = await _dbContext.Semesters.FindAsync(id);
        if (semester == null)
        {
            TempData["Error"] = "Semester not found.";
            return RedirectToPage();
        }

        if (semester.IsActive)
        {
            TempData["Error"] = "Cannot delete the currently active semester.";
            await OnGetAsync(semester.AcademicYearId);
            return Page();
        }

        var slotCount = await _dbContext.TimetableSlots.CountAsync(t => t.SemesterId == id);
        if (slotCount > 0)
        {
            TempData["Error"] = $"Cannot delete semester. It has {slotCount} timetable slots.";
            await OnGetAsync(semester.AcademicYearId);
            return Page();
        }

        var semesterName = semester.Name;
        var yearId = semester.AcademicYearId;
        _dbContext.Semesters.Remove(semester);
        await _dbContext.SaveChangesAsync();

        await _auditService.LogAsync("DELETE", "Semester", id.ToString(),
            $"Name: {semesterName}", null);

        TempData["Success"] = $"Semester '{semesterName}' deleted successfully.";
        return RedirectToPage(new { yearId });
    }
}
