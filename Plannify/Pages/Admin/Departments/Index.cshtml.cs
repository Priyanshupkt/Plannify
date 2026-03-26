using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Plannify.Data;
using Plannify.Models;
using Plannify.Services;

namespace Plannify.Pages.Admin.Departments;

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
    public Department NewDepartment { get; set; } = new();

    public List<Department> Departments { get; set; } = new();
    public Dictionary<int, int> TeacherCounts { get; set; } = new();

    public async Task OnGetAsync()
    {
        Departments = await _dbContext.Departments.ToListAsync();
        
        foreach (var dept in Departments)
        {
            var count = await _dbContext.Teachers.CountAsync(t => t.DepartmentId == dept.Id);
            TeacherCounts[dept.Id] = count;
        }
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var deptExists = await _dbContext.Departments.AnyAsync(d => d.Code == NewDepartment.Code);
        if (deptExists)
        {
            TempData["Error"] = $"Department with code '{NewDepartment.Code}' already exists.";
            await OnGetAsync();
            return Page();
        }

        _dbContext.Departments.Add(NewDepartment);
        await _dbContext.SaveChangesAsync();

        await _auditService.LogAsync("CREATE", "Department", NewDepartment.Id.ToString(), 
            null, $"Name: {NewDepartment.Name}, Code: {NewDepartment.Code}");

        TempData["Success"] = $"Department '{NewDepartment.Name}' added successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync(int id, string name, string code, string? shortName, string? hodName)
    {
        var department = await _dbContext.Departments.FindAsync(id);
        if (department == null)
        {
            TempData["Error"] = "Department not found.";
            return RedirectToPage();
        }

        var oldValues = $"Name: {department.Name}, Code: {department.Code}";

        department.Name = name;
        department.Code = code;
        department.ShortName = shortName;
        department.HODName = hodName;

        _dbContext.Departments.Update(department);
        await _dbContext.SaveChangesAsync();

        var newValues = $"Name: {department.Name}, Code: {department.Code}";
        await _auditService.LogAsync("UPDATE", "Department", department.Id.ToString(), oldValues, newValues);

        TempData["Success"] = "Department updated successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var department = await _dbContext.Departments.FindAsync(id);
        if (department == null)
        {
            TempData["Error"] = "Department not found.";
            return RedirectToPage();
        }

        var teacherCount = await _dbContext.Teachers.CountAsync(t => t.DepartmentId == id);
        var subjectCount = await _dbContext.Subjects.CountAsync(s => s.DepartmentId == id);
        var classCount = await _dbContext.ClassBatches.CountAsync(c => c.DepartmentId == id);

        if (teacherCount > 0 || subjectCount > 0 || classCount > 0)
        {
            TempData["Error"] = $"Cannot delete department. It has {teacherCount} teachers, {subjectCount} subjects, and {classCount} classes.";
            await OnGetAsync();
            return Page();
        }

        var deptName = department.Name;
        _dbContext.Departments.Remove(department);
        await _dbContext.SaveChangesAsync();

        await _auditService.LogAsync("DELETE", "Department", id.ToString(), 
            $"Name: {deptName}, Code: {department.Code}", null);

        TempData["Success"] = $"Department '{deptName}' deleted successfully.";
        return RedirectToPage();
    }
}
