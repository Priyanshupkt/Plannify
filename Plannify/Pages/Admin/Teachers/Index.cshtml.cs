using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Plannify.Data;
using Plannify.Models;
using Plannify.Services;

namespace Plannify.Pages.Admin.Teachers;

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
    public Plannify.Models.Teacher NewTeacher { get; set; } = new();

    public List<Plannify.Models.Teacher> Teachers { get; set; } = new();
    public List<Department> Departments { get; set; } = new();
    public Dictionary<int, decimal> TeacherWorkloads { get; set; } = new();

    public async Task OnGetAsync()
    {
        Teachers = await _dbContext.Teachers.Include(t => t.Department).ToListAsync();
        Departments = await _dbContext.Departments.ToListAsync();

        var activeSemester = await _dbContext.Semesters.FirstOrDefaultAsync(s => s.IsActive);

        foreach (var teacher in Teachers)
        {
            decimal totalHours = 0;

            if (activeSemester != null)
            {
                var slots = await _dbContext.TimetableSlots
                    .Where(t => t.TeacherId == teacher.Id && t.SemesterId == activeSemester.Id)
                    .ToListAsync();

                foreach (var slot in slots)
                {
                    var hours = (slot.EndTime.Hour - slot.StartTime.Hour) + 
                               ((slot.EndTime.Minute - slot.StartTime.Minute) / 60m);
                    totalHours += hours;
                }
            }

            TeacherWorkloads[teacher.Id] = totalHours;
        }
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var codeExists = await _dbContext.Teachers.AnyAsync(t => t.EmployeeCode == NewTeacher.EmployeeCode);
        if (codeExists)
        {
            TempData["Error"] = $"Teacher with employee code '{NewTeacher.EmployeeCode}' already exists.";
            await OnGetAsync();
            return Page();
        }

        _dbContext.Teachers.Add(NewTeacher);
        await _dbContext.SaveChangesAsync();

        await _auditService.LogAsync("CREATE", "Teacher", NewTeacher.Id.ToString(),
            null, $"Name: {NewTeacher.FullName}, Code: {NewTeacher.EmployeeCode}, Email: {NewTeacher.Email}");

        TempData["Success"] = $"Teacher '{NewTeacher.FullName}' added successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync(int id, string fullName, string employeeCode, string email, 
        string designation, int departmentId, int maxWeeklyHours)
    {
        var teacher = await _dbContext.Teachers.FindAsync(id);
        if (teacher == null)
        {
            TempData["Error"] = "Teacher not found.";
            return RedirectToPage();
        }

        var codeExists = await _dbContext.Teachers.AnyAsync(t => t.EmployeeCode == employeeCode && t.Id != id);
        if (codeExists)
        {
            TempData["Error"] = $"Employee code '{employeeCode}' already exists.";
            await OnGetAsync();
            return Page();
        }

        var oldValues = $"Name: {teacher.FullName}, Code: {teacher.EmployeeCode}";

        teacher.FullName = fullName;
        teacher.EmployeeCode = employeeCode;
        teacher.Email = email;
        teacher.Designation = designation;
        teacher.DepartmentId = departmentId;
        teacher.MaxWeeklyHours = maxWeeklyHours;

        _dbContext.Teachers.Update(teacher);
        await _dbContext.SaveChangesAsync();

        var newValues = $"Name: {teacher.FullName}, Code: {teacher.EmployeeCode}";
        await _auditService.LogAsync("UPDATE", "Teacher", teacher.Id.ToString(), oldValues, newValues);

        TempData["Success"] = "Teacher updated successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var teacher = await _dbContext.Teachers.FindAsync(id);
        if (teacher == null)
        {
            TempData["Error"] = "Teacher not found.";
            return RedirectToPage();
        }

        var slotCount = await _dbContext.TimetableSlots.CountAsync(t => t.TeacherId == id);
        if (slotCount > 0)
        {
            TempData["Error"] = $"Cannot delete teacher. They have {slotCount} timetable slots assigned.";
            await OnGetAsync();
            return Page();
        }

        var teacherName = teacher.FullName;
        _dbContext.Teachers.Remove(teacher);
        await _dbContext.SaveChangesAsync();

        await _auditService.LogAsync("DELETE", "Teacher", id.ToString(),
            $"Name: {teacherName}, Code: {teacher.EmployeeCode}", null);

        TempData["Success"] = $"Teacher '{teacherName}' deleted successfully.";
        return RedirectToPage();
    }
}
