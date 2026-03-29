using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Plannify.Data;
using Plannify.Models;
using Plannify.Services;

namespace Plannify.Pages.Admin.Classes;

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
    public ClassBatch NewClassBatch { get; set; } = new();

    public List<ClassBatch> ClassBatches { get; set; } = new();
    public List<Department> Departments { get; set; } = new();
    public List<AcademicYear> AcademicYears { get; set; } = new();
    public List<Room> Rooms { get; set; } = new();
    public Dictionary<int, int> SlotCounts { get; set; } = new();

    public async Task OnGetAsync()
    {
        ClassBatches = await _dbContext.ClassBatches.OrderBy(c => c.BatchName).ToListAsync();
        Departments = await _dbContext.Departments.ToListAsync();
        AcademicYears = await _dbContext.AcademicYears.OrderByDescending(ay => ay.YearLabel).ToListAsync();
        Rooms = await _dbContext.Rooms.OrderBy(r => r.RoomNumber).ToListAsync();

        foreach (var batch in ClassBatches)
        {
            var count = await _dbContext.TimetableSlots.CountAsync(t => t.ClassBatchId == batch.Id);
            SlotCounts[batch.Id] = count;
        }
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var batchExists = await _dbContext.ClassBatches.AnyAsync(c => c.BatchName == NewClassBatch.BatchName);
        if (batchExists)
        {
            TempData["Error"] = $"Class batch '{NewClassBatch.BatchName}' already exists.";
            await OnGetAsync();
            return Page();
        }

        _dbContext.ClassBatches.Add(NewClassBatch);
        await _dbContext.SaveChangesAsync();

        await _auditService.LogAsync("CREATE", "ClassBatch", NewClassBatch.Id.ToString(),
            null, $"Name: {NewClassBatch.BatchName}, Strength: {NewClassBatch.Strength}, Semester: {NewClassBatch.Semester}");

        TempData["Success"] = $"Class batch '{NewClassBatch.BatchName}' added successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync(int id, string batchName, int strength, int departmentId, 
        int academicYearId, int semester, int? roomId)
    {
        var batch = await _dbContext.ClassBatches.FindAsync(id);
        if (batch == null)
        {
            TempData["Error"] = "Class batch not found.";
            return RedirectToPage();
        }

        var nameExists = await _dbContext.ClassBatches.AnyAsync(c => c.BatchName == batchName && c.Id != id);
        if (nameExists)
        {
            TempData["Error"] = $"Class batch name '{batchName}' already exists.";
            await OnGetAsync();
            return Page();
        }

        var oldValues = $"Name: {batch.BatchName}, Strength: {batch.Strength}";

        batch.BatchName = batchName;
        batch.Strength = strength;
        batch.DepartmentId = departmentId;
        batch.AcademicYearId = academicYearId;
        batch.Semester = semester;
        batch.RoomId = roomId;

        _dbContext.ClassBatches.Update(batch);
        await _dbContext.SaveChangesAsync();

        var newValues = $"Name: {batch.BatchName}, Strength: {batch.Strength}";
        await _auditService.LogAsync("UPDATE", "ClassBatch", batch.Id.ToString(), oldValues, newValues);

        TempData["Success"] = "Class batch updated successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var batch = await _dbContext.ClassBatches.FindAsync(id);
        if (batch == null)
        {
            TempData["Error"] = "Class batch not found.";
            return RedirectToPage();
        }

        var slotCount = await _dbContext.TimetableSlots.CountAsync(t => t.ClassBatchId == id);
        if (slotCount > 0)
        {
            TempData["Error"] = $"Cannot delete class batch. It has {slotCount} timetable slots assigned.";
            await OnGetAsync();
            return Page();
        }

        var batchName = batch.BatchName;
        _dbContext.ClassBatches.Remove(batch);
        await _dbContext.SaveChangesAsync();

        await _auditService.LogAsync("DELETE", "ClassBatch", id.ToString(),
            $"Name: {batchName}, Strength: {batch.Strength}", null);

        TempData["Success"] = $"Class batch '{batchName}' deleted successfully.";
        return RedirectToPage();
    }
}
