using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Plannify.Application.Contracts;
using Plannify.Application.DTOs;

namespace Plannify.Pages.Admin;

public class TeachersModel(ITeacherService teacherService) : PageModel
{
    private readonly ITeacherService _teacherService = teacherService;

    public List<TeacherDto> Teachers { get; set; } = new();

    [BindProperty]
    public CreateTeacherRequest NewTeacher { get; set; } = new();

    public async Task OnGetAsync()
    {
        var result = await _teacherService.GetAllAsync();
        if (result.IsSuccess)
            Teachers = result.Value?.ToList() ?? new();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var result = await _teacherService.CreateTeacherAsync(NewTeacher);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to add teacher");
            await OnGetAsync();
            return Page();
        }

        TempData["Success"] = "Teacher added successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await _teacherService.DeleteTeacherAsync(id);
        if (result.IsSuccess)
            TempData["Success"] = "Teacher deleted successfully.";
        else
            TempData["Error"] = result.ErrorMessage ?? "Failed to delete teacher";

        return RedirectToPage();
    }
}
