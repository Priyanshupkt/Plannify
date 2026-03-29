using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Plannify.Application.Contracts;
using Plannify.Application.DTOs;

namespace Plannify.Pages.Admin.AcademicYears;

public class IndexModel : PageModel
{
    private readonly IAcademicYearService _academicYearService;

    public IndexModel(IAcademicYearService academicYearService)
    {
        _academicYearService = academicYearService;
    }

    [BindProperty]
    public CreateAcademicYearRequest NewAcademicYear { get; set; } = new();

    public List<AcademicYearDto> AcademicYears { get; set; } = new();
    public Dictionary<int, int> SemesterCounts { get; set; } = new();

    public async Task OnGetAsync()
    {
        var result = await _academicYearService.GetAllAsync();
        if (result.IsSuccess)
            AcademicYears = result.Value?.ToList() ?? new();

        foreach (var year in AcademicYears)
        {
            SemesterCounts[year.Id] = 0;  // TODO: Get from SemesterService if needed
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

        var result = await _academicYearService.CreateAsync(NewAcademicYear);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to add academic year");
            await OnGetAsync();
            return Page();
        }

        TempData["Success"] = "Academic Year added successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await _academicYearService.DeleteAsync(id);
        if (result.IsSuccess)
            TempData["Success"] = "Academic Year deleted successfully.";
        else
            TempData["Error"] = result.ErrorMessage ?? "Failed to delete academic year";

        return RedirectToPage();
    }
}
