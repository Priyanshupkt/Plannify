using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Plannify.Application.Contracts;
using Plannify.Application.DTOs;

namespace Plannify.Pages.Admin.Teachers;

/// <summary>
/// Refactored Teachers page model
/// ✅ No DbContext injection
/// ✅ No data access code
/// ✅ Only presentation concerns
/// ✅ Delegates to application service
/// </summary>
[Authorize]
public class IndexModel : PageModel
{
    private readonly ITeacherService _teacherService;
    private readonly IDepartmentService _departmentService;

    // ✅ Only DTOs, no entities
    public List<TeacherDto> Teachers { get; set; } = new();
    public List<DepartmentDto> Departments { get; set; } = new();
    public Dictionary<int, decimal> TeacherWorkloads { get; set; } = new();  // For backward compatibility
    
    [BindProperty]
    public CreateTeacherRequest NewTeacher { get; set; } = new();

    [BindProperty]
    public UpdateTeacherRequest EditTeacher { get; set; } = new();

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public IndexModel(
        ITeacherService teacherService,
        IDepartmentService departmentService)
    {
        _teacherService = teacherService;
        _departmentService = departmentService;
    }

    /// <summary>
    /// Load all teachers and departments
    /// </summary>
    public async Task OnGetAsync()
    {
        await LoadTeachersAsync();
        await LoadDepartmentsAsync();
    }

    /// <summary>
    /// Create new teacher
    /// </summary>
    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadTeachersAsync();
            await LoadDepartmentsAsync();
            return Page();
        }

        var result = await _teacherService.CreateTeacherAsync(NewTeacher);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            await LoadTeachersAsync();
            await LoadDepartmentsAsync();
            return Page();
        }

        TempData["Success"] = $"Teacher '{NewTeacher.FullName}' added successfully.";
        return RedirectToPage();
    }

    /// <summary>
    /// Update existing teacher
    /// </summary>
    public async Task<IActionResult> OnPostUpdateAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadTeachersAsync();
            await LoadDepartmentsAsync();
            return Page();
        }

        var result = await _teacherService.UpdateTeacherAsync(EditTeacher);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            await LoadTeachersAsync();
            await LoadDepartmentsAsync();
            return Page();
        }

        TempData["Success"] = "Teacher updated successfully.";
        return RedirectToPage();
    }

    /// <summary>
    /// Delete teacher
    /// </summary>
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await _teacherService.DeleteTeacherAsync(id);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToPage();
        }

        TempData["Success"] = "Teacher deleted successfully.";
        return RedirectToPage();
    }

    /// <summary>
    /// Deactivate teacher without deleting
    /// </summary>
    public async Task<IActionResult> OnPostDeactivateAsync(int id)
    {
        var result = await _teacherService.DeactivateTeacherAsync(id);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToPage();
        }

        TempData["Success"] = "Teacher deactivated successfully.";
        return RedirectToPage();
    }

    // ======== Helper Methods ========

    private async Task LoadTeachersAsync()
    {
        var result = await _teacherService.GetAllAsync();
        if (result.IsSuccess)
        {
            Teachers = result.Value!.ToList();
            // Populate workload dictionary from DTOs for backward compatibility
            foreach (var teacher in Teachers)
            {
                TeacherWorkloads[teacher.Id] = teacher.CurrentWeeklyHours;
            }
        }
        else
        {
            ErrorMessage = result.ErrorMessage;
            Teachers = new();
        }
    }

    private async Task LoadDepartmentsAsync()
    {
        var result = await _departmentService.GetAllAsync();
        if (result.IsSuccess)
        {
            Departments = result.Value!.ToList();
        }
        else
        {
            Departments = new();
        }
    }
}
