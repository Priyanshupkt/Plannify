using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Plannify.Application.Contracts;
using Plannify.Application.DTOs;

namespace Plannify.Pages.Admin.Departments;

/// <summary>
/// Refactored Departments page model
/// ✅ No DbContext injection
/// ✅ No data access code
/// ✅ Only presentation concerns
/// ✅ Delegates to application service
/// </summary>
[Authorize]
public class IndexModel : PageModel
{
    private readonly IDepartmentService _departmentService;

    // ✅ Only DTOs, no entities
    public List<DepartmentDto> Departments { get; set; } = new();

    [BindProperty]
    public CreateDepartmentRequest NewDepartment { get; set; } = new();

    [BindProperty]
    public UpdateDepartmentRequest EditDepartment { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public IndexModel(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    /// <summary>
    /// Load all departments
    /// </summary>
    public async Task OnGetAsync()
    {
        var result = await _departmentService.GetAllAsync();
        if (result.IsSuccess)
        {
            Departments = result.Value!.ToList();
        }
        else
        {
            ErrorMessage = result.ErrorMessage;
            Departments = new();
        }
    }

    /// <summary>
    /// Create new department
    /// </summary>
    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var result = await _departmentService.CreateAsync(NewDepartment);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            await OnGetAsync();
            return Page();
        }

        TempData["Success"] = $"Department '{NewDepartment.Name}' added successfully.";
        return RedirectToPage();
    }

    /// <summary>
    /// Update existing department
    /// </summary>
    public async Task<IActionResult> OnPostUpdateAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var result = await _departmentService.UpdateAsync(EditDepartment);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            await OnGetAsync();
            return Page();
        }

        TempData["Success"] = "Department updated successfully.";
        return RedirectToPage();
    }

    /// <summary>
    /// Delete department
    /// </summary>
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await _departmentService.DeleteAsync(id);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToPage();
        }

        TempData["Success"] = "Department deleted successfully.";
        return RedirectToPage();
    }
}
