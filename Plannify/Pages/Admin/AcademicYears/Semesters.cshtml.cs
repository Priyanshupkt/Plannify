using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Plannify.Application.Contracts;
using Plannify.Application.DTOs;

namespace Plannify.Pages.Admin.AcademicYears;

public class SemestersModel : PageModel
{
    private readonly ISemesterService _semesterService;
    private readonly IAcademicYearService _academicYearService;
    private readonly ITimetableSlotService _timetableSlotService;

    public SemestersModel(ISemesterService semesterService, IAcademicYearService academicYearService, ITimetableSlotService timetableSlotService)
    {
        _semesterService = semesterService;
        _academicYearService = academicYearService;
        _timetableSlotService = timetableSlotService;
    }

    [BindProperty]
    public CreateSemesterRequest NewSemester { get; set; } = new();

    public AcademicYearDto? AcademicYear { get; set; }
    public List<SemesterDto> Semesters { get; set; } = new();
    public Dictionary<int, int> SlotCounts { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int yearId)
    {
        var yearResult = await _academicYearService.GetByIdAsync(yearId);
        if (!yearResult.IsSuccess || yearResult.Value == null)
        {
            return NotFound();
        }

        AcademicYear = yearResult.Value;

        var semesterResult = await _semesterService.GetByAcademicYearAsync(yearId);
        if (semesterResult.IsSuccess)
        {
            Semesters = semesterResult.Value?.ToList() ?? new();

            foreach (var semester in Semesters)
            {
                var slotResult = await _timetableSlotService.GetBySemesterAsync(semester.Id);
                SlotCounts[semester.Id] = slotResult.IsSuccess ? slotResult.Value?.Count() ?? 0 : 0;
            }

            Semesters = Semesters.OrderBy(s => s.SemesterNumber).ToList();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync(int yearId)
    {
        var yearResult = await _academicYearService.GetByIdAsync(yearId);
        if (!yearResult.IsSuccess || yearResult.Value == null)
        {
            return NotFound();
        }

        AcademicYear = yearResult.Value;

        if (!ModelState.IsValid)
        {
            await OnGetAsync(yearId);
            return Page();
        }

        NewSemester.AcademicYearId = yearId;
        var result = await _semesterService.CreateAsync(NewSemester);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to add semester");
            await OnGetAsync(yearId);
            return Page();
        }

        TempData["Success"] = $"Semester '{NewSemester.Name}' added successfully.";
        return RedirectToPage(new { yearId });
    }

    public async Task<IActionResult> OnPostSetActiveAsync(int id, int yearId)
    {
        var semesterResult = await _semesterService.GetByIdAsync(id);
        if (!semesterResult.IsSuccess || semesterResult.Value == null)
        {
            TempData["Error"] = "Semester not found.";
            return RedirectToPage();
        }

        var semester = semesterResult.Value;
        
        var updateRequest = new UpdateSemesterRequest
        {
            Id = semester.Id,
            Name = semester.Name,
            SemesterNumber = semester.SemesterNumber,
            StartDate = semester.StartDate,
            EndDate = semester.EndDate
        };

        var updateResult = await _semesterService.UpdateAsync(updateRequest);
        if (!updateResult.IsSuccess)
        {
            TempData["Error"] = updateResult.ErrorMessage ?? "Failed to update semester";
        }
        else
        {
            TempData["Success"] = $"Semester '{semester.Name}' is now active.";
        }

        return RedirectToPage(new { yearId = semester.AcademicYearId });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, int yearId)
    {
        var semesterResult = await _semesterService.GetByIdAsync(id);
        if (!semesterResult.IsSuccess || semesterResult.Value == null)
        {
            TempData["Error"] = "Semester not found.";
            return RedirectToPage(new { yearId });
        }

        var semester = semesterResult.Value;

        if (semester.IsActive)
        {
            TempData["Error"] = "Cannot delete the currently active semester.";
            await OnGetAsync(yearId);
            return Page();
        }

        var slotResult = await _timetableSlotService.GetBySemesterAsync(id);
        var slotCount = slotResult.IsSuccess ? slotResult.Value?.Count() ?? 0 : 0;
        
        if (slotCount > 0)
        {
            TempData["Error"] = $"Cannot delete semester. It has {slotCount} timetable slots.";
            await OnGetAsync(yearId);
            return Page();
        }

        var deleteResult = await _semesterService.DeleteAsync(id);
        if (!deleteResult.IsSuccess)
        {
            TempData["Error"] = deleteResult.ErrorMessage ?? "Failed to delete semester";
        }
        else
        {
            TempData["Success"] = $"Semester '{semester.Name}' deleted successfully.";
        }

        return RedirectToPage(new { yearId });
    }
}
