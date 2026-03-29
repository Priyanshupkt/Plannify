using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Plannify.Data;
using Plannify.Models;
using Plannify.Services;

namespace Plannify.Pages.Admin.Subjects;

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
    public Subject NewSubject { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int? FilterDepartmentId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? FilterSubjectType { get; set; }

    public List<Subject> Subjects { get; set; } = new();
    public List<Department> Departments { get; set; } = new();

    public async Task OnGetAsync()
    {
        Departments = await _dbContext.Departments.ToListAsync();

        var query = _dbContext.Subjects.AsQueryable();

        if (FilterDepartmentId.HasValue)
        {
            query = query.Where(s => s.DepartmentId == FilterDepartmentId.Value);
        }

        if (!string.IsNullOrEmpty(FilterSubjectType))
        {
            query = query.Where(s => s.SubjectType == FilterSubjectType);
        }

        Subjects = await query.OrderBy(s => s.SemesterNumber).ThenBy(s => s.Code).ToListAsync();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var codeExists = await _dbContext.Subjects.AnyAsync(s => s.Code == NewSubject.Code);
        if (codeExists)
        {
            TempData["Error"] = $"Subject code '{NewSubject.Code}' already exists.";
            await OnGetAsync();
            return Page();
        }

        _dbContext.Subjects.Add(NewSubject);
        await _dbContext.SaveChangesAsync();

        await _auditService.LogAsync("CREATE", "Subject", NewSubject.Id.ToString(),
            null, $"Code: {NewSubject.Code}, Name: {NewSubject.Name}, Type: {NewSubject.SubjectType}");

        TempData["Success"] = $"Subject '{NewSubject.Code}' added successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync(int id, string name, string code, int credits, 
        string subjectType, int departmentId, int semesterNumber)
    {
        var subject = await _dbContext.Subjects.FindAsync(id);
        if (subject == null)
        {
            TempData["Error"] = "Subject not found.";
            return RedirectToPage();
        }

        var codeExists = await _dbContext.Subjects.AnyAsync(s => s.Code == code && s.Id != id);
        if (codeExists)
        {
            TempData["Error"] = $"Subject code '{code}' already exists.";
            await OnGetAsync();
            return Page();
        }

        var oldValues = $"Code: {subject.Code}, Name: {subject.Name}";

        subject.Name = name;
        subject.Code = code;
        subject.Credits = credits;
        subject.SubjectType = subjectType;
        subject.DepartmentId = departmentId;
        subject.SemesterNumber = semesterNumber;

        _dbContext.Subjects.Update(subject);
        await _dbContext.SaveChangesAsync();

        var newValues = $"Code: {subject.Code}, Name: {subject.Name}";
        await _auditService.LogAsync("UPDATE", "Subject", subject.Id.ToString(), oldValues, newValues);

        TempData["Success"] = "Subject updated successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var subject = await _dbContext.Subjects.FindAsync(id);
        if (subject == null)
        {
            TempData["Error"] = "Subject not found.";
            return RedirectToPage();
        }

        var slotCount = await _dbContext.TimetableSlots.CountAsync(t => t.SubjectId == id);
        if (slotCount > 0)
        {
            TempData["Error"] = $"Cannot delete subject. It has {slotCount} timetable slots assigned.";
            await OnGetAsync();
            return Page();
        }

        var subjectCode = subject.Code;
        _dbContext.Subjects.Remove(subject);
        await _dbContext.SaveChangesAsync();

        await _auditService.LogAsync("DELETE", "Subject", id.ToString(),
            $"Code: {subjectCode}, Name: {subject.Name}", null);

        TempData["Success"] = $"Subject '{subjectCode}' deleted successfully.";
        return RedirectToPage();
    }
}
