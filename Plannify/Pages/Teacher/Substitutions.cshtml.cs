using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Plannify.Data;
using Plannify.Models;

namespace Plannify.Pages.Teacher;

[Authorize(Roles = "Admin")]
public class SubstitutionsModel : PageModel
{
    private readonly AppDbContext _context;

    public SubstitutionsModel(AppDbContext context)
    {
        _context = context;
    }

    public List<SubstitutionRecord> SlotsICovered { get; set; } = new();
    public List<SubstitutionRecord> SlotsCoveredForMe { get; set; } = new();

    public async Task OnGetAsync()
    {
        var teacherId = GetCurrentTeacherId();
        if (teacherId == 0)
        {
            ModelState.AddModelError(string.Empty, "Teacher record not found.");
            return;
        }

        // Slots I covered for others
        SlotsICovered = await _context.SubstitutionRecords
            .Where(s => s.SubstituteTeacherId == teacherId)
            .Include(s => s.OriginalTeacher)
            .Include(s => s.TimetableSlot)
            .ThenInclude(s => s.Subject)
            .Include(s => s.TimetableSlot)
            .ThenInclude(s => s.ClassBatch)
            .OrderByDescending(s => s.Date)
            .ToListAsync();

        // Slots covered for me
        SlotsCoveredForMe = await _context.SubstitutionRecords
            .Where(s => s.OriginalTeacherId == teacherId)
            .Include(s => s.SubstituteTeacher)
            .Include(s => s.TimetableSlot)
            .ThenInclude(s => s.Subject)
            .Include(s => s.TimetableSlot)
            .ThenInclude(s => s.ClassBatch)
            .OrderByDescending(s => s.Date)
            .ToListAsync();
    }

    private int GetCurrentTeacherId()
    {
        var userName = User.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
            return 0;

        var teacher = _context.Teachers
            .AsNoTracking()
            .FirstOrDefault(t => t.EmployeeCode == userName || t.Email == userName);

        return teacher?.Id ?? 0;
    }
}
