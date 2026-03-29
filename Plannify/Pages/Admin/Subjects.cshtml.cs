using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Plannify.Application.Contracts;
using Plannify.Application.DTOs;

namespace Plannify.Pages.Admin;

public class SubjectsModel(ISubjectService subjectService) : PageModel
{
    private readonly ISubjectService _subjectService = subjectService;

    public List<SubjectDto> Subjects { get; set; } = new();

    [BindProperty]
    public CreateSubjectRequest NewSubject { get; set; } = new();

    public async Task OnGetAsync()
    {
        var result = await _subjectService.GetAllAsync();
        if (result.IsSuccess)
            Subjects = result.Value?.ToList() ?? new();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var result = await _subjectService.CreateAsync(NewSubject);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to add subject");
            await OnGetAsync();
            return Page();
        }

        TempData["Success"] = "Subject added successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await _subjectService.DeleteAsync(id);
        if (result.IsSuccess)
            TempData["Success"] = "Subject deleted successfully.";
        else
            TempData["Error"] = result.ErrorMessage ?? "Failed to delete subject";

        return RedirectToPage();
    }
}