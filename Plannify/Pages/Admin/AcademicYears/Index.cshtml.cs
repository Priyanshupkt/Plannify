using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Plannify.Data;
using Plannify.Models;
using Plannify.Services;

namespace Plannify.Pages.Admin.AcademicYears;

[Authorize(Roles = "SuperAdmin")]
public class IndexModel : PageModel
{
    private readonly AppDbContext _dbContext;
    private readonly AuditService _auditService;

    public IndexModel(AppDbContext dbContext, AuditService auditService)
    {
        _dbContext = dbContext;
        _auditService = auditService;
    }

    [BindProperty]
    public AcademicYear NewAcademicYear { get; set; } = new();

    public List<AcademicYear> AcademicYears { get; set; } = new();
    public Dictionary<int, int> SemesterCounts { get; set; } = new();

    public async Task OnGetAsync()
    {
        AcademicYears = await _dbContext.AcademicYears
            .OrderByDescending(ay => ay.YearLabel)
            .ToListAsync();

        foreach (var year in AcademicYears)
        {
            var count = await _dbContext.Semesters.CountAsync(s => s.AcademicYearId == year.Id);
            SemesterCounts[year.Id] = count;
        }
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        if (NewAcademicYear.StartDate >= NewAcademicYear.EndDate)
        {
            TempData["Error"] = "Start date must be before end date.";
            await OnGetAsync();
            return Page();
        }

        var yearExists = await _dbContext.AcademicYears.AnyAsync(ay => ay.YearLabel == NewAcademicYear.YearLabel);
        if (yearExists)
        {
            TempData["Error"] = $"Academic year '{NewAcademicYear.YearLabel}' already exists.";
            await OnGetAsync();
            return Page();
        }

        NewAcademicYear.IsActive = false;
        _dbContext.AcademicYears.Add(NewAcademicYear);
        await _dbContext.SaveChangesAsync();

        await _auditService.LogAsync("CREATE", "AcademicYear", NewAcademicYear.Id.ToString(),
            null, $"Year: {NewAcademicYear.YearLabel}, Start: {NewAcademicYear.StartDate}, End: {NewAcademicYear.EndDate}");

        TempData["Success"] = $"Academic year '{NewAcademicYear.YearLabel}' added successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSetActiveAsync(int id)
    {
        var academicYear = await _dbContext.AcademicYears.FindAsync(id);
        if (academicYear == null)
        {
            TempData["Error"] = "Academic year not found.";
            return RedirectToPage();
        }

        var currentlyActive = await _dbContext.AcademicYears.FirstOrDefaultAsync(ay => ay.IsActive);
        if (currentlyActive != null)
        {
            currentlyActive.IsActive = false;
            _dbContext.AcademicYears.Update(currentlyActive);
            await _auditService.LogAsync("UPDATE", "AcademicYear", currentlyActive.Id.ToString(),
                "IsActive: true", "IsActive: false");
        }

        academicYear.IsActive = true;
        _dbContext.AcademicYears.Update(academicYear);
        await _dbContext.SaveChangesAsync();

        await _auditService.LogAsync("UPDATE", "AcademicYear", academicYear.Id.ToString(),
            "IsActive: false", "IsActive: true");

        TempData["Success"] = $"Academic year '{academicYear.YearLabel}' is now active.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var academicYear = await _dbContext.AcademicYears.FindAsync(id);
        if (academicYear == null)
        {
            TempData["Error"] = "Academic year not found.";
            return RedirectToPage();
        }

        if (academicYear.IsActive)
        {
            TempData["Error"] = "Cannot delete the currently active academic year.";
            await OnGetAsync();
            return Page();
        }

        var semesterCount = await _dbContext.Semesters.CountAsync(s => s.AcademicYearId == id);
        var classCount = await _dbContext.ClassBatches.CountAsync(c => c.AcademicYearId == id);

        if (semesterCount > 0 || classCount > 0)
        {
            TempData["Error"] = $"Cannot delete academic year. It has {semesterCount} semesters and {classCount} classes.";
            await OnGetAsync();
            return Page();
        }

        var yearLabel = academicYear.YearLabel;
        _dbContext.AcademicYears.Remove(academicYear);
        await _dbContext.SaveChangesAsync();

        await _auditService.LogAsync("DELETE", "AcademicYear", id.ToString(),
            $"Year: {yearLabel}", null);

        TempData["Success"] = $"Academic year '{yearLabel}' deleted successfully.";
        return RedirectToPage();
    }
}
