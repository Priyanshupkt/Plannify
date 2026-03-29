using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Plannify.Application.Contracts;
using Plannify.Data;
using Plannify.Domain.Entities;
using Plannify.Services;

namespace Plannify.Pages.Admin.Timetable;

public class ConflictsModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly IConflictDetectorService _conflictDetector;
    private readonly ISemesterService _semesterService;

    public ConflictsModel(AppDbContext context, IConflictDetectorService conflictDetector, ISemesterService semesterService)
    {
        _context = context;
        _conflictDetector = conflictDetector;
        _semesterService = semesterService;
    }

    public List<SelectListItem> Semesters { get; set; } = new();
    public List<ConflictReport> AllConflicts { get; set; } = new();
    public List<ConflictReport> TeacherConflicts { get; set; } = new();
    public List<ConflictReport> RoomConflicts { get; set; } = new();
    public List<ConflictReport> ClassConflicts { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int ScanSemesterId { get; set; }

    public bool ScanRan { get; set; }
    public DateTime? LastScanTime { get; set; }

    public async Task OnGetAsync()
    {
        await LoadSemestersAsync();

        if (ScanSemesterId == 0)
        {
            var activeSemester = await _context.Semesters.FirstOrDefaultAsync(s => s.IsActive);
            ScanSemesterId = activeSemester?.Id ?? 0;
        }

        if (TempData["LastScanTime"] is string scanTime)
        {
            LastScanTime = DateTime.Parse(scanTime);
            ScanRan = true;
        }
        if (TempData["AllConflicts"] is string conflictsJson)
        {
            AllConflicts = System.Text.Json.JsonSerializer.Deserialize<List<ConflictReport>>(conflictsJson) ?? new();
            TeacherConflicts = AllConflicts.Where(c => c.ConflictType == "Teacher").ToList();
            RoomConflicts = AllConflicts.Where(c => c.ConflictType == "Room").ToList();
            ClassConflicts = AllConflicts.Where(c => c.ConflictType == "Class").ToList();
            ScanRan = true;
        }
    }

    public async Task<IActionResult> OnPostScanAsync()
    {
        await LoadSemestersAsync();

        if (ScanSemesterId == 0)
            return RedirectToPage();

        AllConflicts = await _conflictDetector.GetAllConflictsAsync(ScanSemesterId);

        TeacherConflicts = AllConflicts.Where(c => c.ConflictType == "Teacher").ToList();
        RoomConflicts = AllConflicts.Where(c => c.ConflictType == "Room").ToList();
        ClassConflicts = AllConflicts.Where(c => c.ConflictType == "Class").ToList();

        TempData["LastScanTime"] = DateTime.Now.ToString("o");
        TempData["AllConflicts"] = System.Text.Json.JsonSerializer.Serialize(AllConflicts);

        return RedirectToPage(new { ScanSemesterId });
    }

    private async Task LoadSemestersAsync()
    {
        Semesters = await _context.Semesters
            .OrderByDescending(s => s.IsActive)
            .ThenByDescending(s => s.StartDate)
            .Select(s => new SelectListItem($"{s.Name} {(s.IsActive ? "(Active)" : "")}", s.Id.ToString()))
            .ToListAsync();
    }
}
