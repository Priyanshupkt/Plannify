using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Plannify.Application.Contracts;
using Plannify.Application.DTOs;

namespace Plannify.Pages.Admin.Subjects;

public class IndexModel : PageModel
{
    private readonly ISubjectService _subjectService;
    private readonly IDepartmentService _departmentService;

    public IndexModel(ISubjectService subjectService, IDepartmentService departmentService)
    {
        _subjectService = subjectService;
        _departmentService = departmentService;
    }

    [BindProperty]
    public CreateSubjectRequest NewSubject { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int? FilterDepartmentId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? FilterSubjectType { get; set; }

    public List<SubjectDto> Subjects { get; set; } = new();
    public List<DepartmentDto> Departments { get; set; } = new();

    public async Task OnGetAsync()
    {
        var deptResult = await _departmentService.GetAllAsync();
        if (deptResult.IsSuccess)
            Departments = deptResult.Value?.ToList() ?? new();

        var result = await _subjectService.GetAllAsync();
        if (result.IsSuccess)
        {
            var subjects = result.Value?.ToList() ?? new();
            
            if (FilterDepartmentId.HasValue)
                subjects = subjects.Where(s => s.DepartmentId == FilterDepartmentId.Value).ToList();

            if (!string.IsNullOrEmpty(FilterSubjectType))
                subjects = subjects.Where(s => s.SubjectType == FilterSubjectType).ToList();

            Subjects = subjects.OrderBy(s => s.SemesterNumber).ThenBy(s => s.Code).ToList();
        }
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

    public async Task<IActionResult> OnPostUpdateAsync(int id, string name, string code, int credits, 
        string subjectType, int departmentId, int semesterNumber)
    {
        var request = new UpdateSubjectRequest
        {
            Id = id,
            Name = name,
            Code = code,
            Credits = credits,
            SubjectType = subjectType,
            DepartmentId = departmentId,
            SemesterNumber = semesterNumber
        };

        var result = await _subjectService.UpdateAsync(request);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage ?? "Failed to update subject";
            await OnGetAsync();
            return Page();
        }

        TempData["Success"] = "Subject updated successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await _subjectService.DeleteAsync(id);
        if (!result.IsSuccess)
            TempData["Error"] = result.ErrorMessage ?? "Failed to delete subject";
        else
            TempData["Success"] = "Subject deleted successfully.";

        return RedirectToPage();
    }
}
