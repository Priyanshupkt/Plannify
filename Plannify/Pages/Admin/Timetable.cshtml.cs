using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Plannify.Data;
using Plannify.Domain.Entities;

namespace Plannify.Pages.Admin;

public class TimetableModel(AppDbContext dbContext) : PageModel
{
    private readonly AppDbContext _dbContext = dbContext;

    [BindProperty]
    public TimetableSlot NewSlot { get; set; } = new();

    public List<TimetableSlot> Slots { get; set; } = new();

    public SelectList DayOptions { get; set; } = default!;
    public SelectList TeacherList { get; set; } = default!;
    public SelectList SubjectList { get; set; } = default!;
    public SelectList ClassList { get; set; } = default!;

    public async Task OnGetAsync()
    {
        await LoadDropdowns();
        Slots = await _dbContext.TimetableSlots
            .Include(t => t.Teacher)
            .Include(t => t.Subject)
            .Include(t => t.ClassBatch)
            .OrderBy(t => t.Day)
            .ThenBy(t => t.StartTime)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdowns();
            Slots = await _dbContext.TimetableSlots
                .Include(t => t.Teacher)
                .Include(t => t.Subject)
                .Include(t => t.ClassBatch)
                .ToListAsync();
            return Page();
        }

        _dbContext.TimetableSlots.Add(NewSlot);
        await _dbContext.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var slot = await _dbContext.TimetableSlots.FindAsync(id);
        if (slot != null)
        {
            _dbContext.TimetableSlots.Remove(slot);
            await _dbContext.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    private async Task LoadDropdowns()
    {
        var days = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
        DayOptions = new SelectList(days);

        var teachers = await _dbContext.Teachers.ToListAsync();
        TeacherList = new SelectList(teachers, "Id", "FullName");

        var subjects = await _dbContext.Subjects.ToListAsync();
        SubjectList = new SelectList(subjects, "Id", "Name");

        var classes = await _dbContext.ClassBatches.ToListAsync();
        ClassList = new SelectList(classes, "Id", "BatchName");
    }
}
