using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Plannify.Data;
using Plannify.Models;
using Plannify.Services;
using System.Text.Json;

namespace Plannify.Pages.Admin.Timetable;

[Authorize(Roles = "Admin,HOD")]
public class ConflictsModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly ConflictDetector _conflictDetector;
    private readonly TimetableExportService _exportService;

    public ConflictsModel(AppDbContext context, ConflictDetector conflictDetector, TimetableExportService exportService)
    {
        _context = context;
        _conflictDetector = conflictDetector;
        _exportService = exportService;
    }

    public List<SelectListItem> Semesters { get; set; } = new();
    public List<ConflictReport> AllConflicts { get; set; } = new();
    public List<ConflictReport> TeacherConflicts { get; set; } = new();
    public List<ConflictReport> RoomConflicts { get; set; } = new();
    public List<ConflictReport> ClassConflicts { get; set; } = new();

    public int CurrentSemesterId { get; set; }
    public DateTime? LastScanTime { get; set; }

    public async Task OnGetAsync(int? semesterId)
    {
        await LoadSemestersAsync();

        if (!semesterId.HasValue)
        {
            var activeSemester = await _context.Semesters.FirstOrDefaultAsync(s => s.IsActive);
            CurrentSemesterId = activeSemester?.Id ?? 0;
        }
        else
        {
            CurrentSemesterId = semesterId.Value;
        }

        if (TempData["LastScanTime"] is string scanTime)
        {
            LastScanTime = DateTime.Parse(scanTime);
        }
    }

    public async Task<IActionResult> OnPostScanAsync(int semesterId)
    {
        CurrentSemesterId = semesterId;
        await LoadSemestersAsync();

        AllConflicts = await _conflictDetector.GetAllConflictsAsync(semesterId);

        TeacherConflicts = AllConflicts.Where(c => c.ConflictType == "TeacherConflict").ToList();
        RoomConflicts = AllConflicts.Where(c => c.ConflictType == "RoomConflict").ToList();
        ClassConflicts = AllConflicts.Where(c => c.ConflictType == "ClassConflict").ToList();

        TempData["LastScanTime"] = DateTime.Now.ToString("o");
        LastScanTime = DateTime.Now;

        return Page();
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
