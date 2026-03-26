using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Plannify.Data;
using Plannify.Models;

namespace Plannify.Pages.Teacher;

public class ViewTimetableModel(AppDbContext dbContext) : PageModel
{
    private readonly AppDbContext _dbContext = dbContext;

    [BindProperty(SupportsGet = true)]
    public int? SelectedTeacherId { get; set; }

    public SelectList Teachers { get; set; } = default!;
    public List<Plannify.Models.TimetableSlot> Slots { get; set; } = new();
    public string SelectedTeacherName { get; set; } = "";
    public int TotalClasses { get; set; }
    public int TotalGaps { get; set; }

    public async Task OnGetAsync()
    {
        var teachers = await _dbContext.Teachers.ToListAsync();
        Teachers = new SelectList(teachers, "Id", "FullName");

        if (SelectedTeacherId.HasValue)
        {
            var teacher = await _dbContext.Teachers.FindAsync(SelectedTeacherId.Value);
            if (teacher != null)
            {
                SelectedTeacherName = teacher.FullName;

                Slots = await _dbContext.TimetableSlots
                    .Where(t => t.TeacherId == SelectedTeacherId.Value)
                    .Include(t => t.Subject)
                    .Include(t => t.ClassBatch)
                    .OrderBy(t => t.Day)
                    .ThenBy(t => t.StartTime)
                    .ToListAsync();

                TotalClasses = Slots.Count(t => t.SlotType != "GAP");
                TotalGaps = Slots.Count(t => t.SlotType == "GAP");
            }
        }
    }
}
