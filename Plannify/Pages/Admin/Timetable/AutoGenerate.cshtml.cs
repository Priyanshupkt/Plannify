using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Plannify.Application.Contracts;
using Plannify.Data;
using Plannify.Services;

namespace Plannify.Pages.Admin.Timetable;

public class AutoGenerateModel : PageModel
{
    private readonly AppDbContext _dbContext;
    private readonly ISchedulingService _schedulingService;
    private readonly AuditService _auditService;

    public AutoGenerateModel(AppDbContext dbContext, ISchedulingService schedulingService, AuditService auditService)
    {
        _dbContext = dbContext;
        _schedulingService = schedulingService;
        _auditService = auditService;
    }

    [BindProperty]
    public int SelectedAcademicYearId { get; set; }

    [BindProperty]
    public int? SelectedSemesterId { get; set; }

    [BindProperty]
    public int? SelectedClassId { get; set; }

    [BindProperty]
    public bool ClearExisting { get; set; } = false;

    [BindProperty]
    public int StartHour { get; set; } = 9;

    [BindProperty]
    public int EndHour { get; set; } = 17;

    [BindProperty]
    public int SlotDurationMinutes { get; set; } = 60;

    public SelectList? AcademicYears { get; set; }
    public SelectList? Semesters { get; set; }
    public SelectList? Classes { get; set; }

    public SchedulingResult? Result { get; set; }
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadDropdowns();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdowns();
            ErrorMessage = "⨯ Invalid input. Please check all required fields.";
            return Page();
        }

        // Validate time range
        if (StartHour < 0 || StartHour > 23 || EndHour < 0 || EndHour > 23)
        {
            await LoadDropdowns();
            ErrorMessage = "⨯ Invalid time range. Hours must be between 0 and 23.";
            return Page();
        }

        if (StartHour >= EndHour)
        {
            await LoadDropdowns();
            ErrorMessage = "⨯ Start hour must be less than end hour.";
            return Page();
        }

        // Validate slot duration
        if (SlotDurationMinutes <= 0 || SlotDurationMinutes > 480)
        {
            await LoadDropdowns();
            ErrorMessage = "⨯ Slot duration must be between 1 and 480 minutes.";
            return Page();
        }

        // Validate semester is selected
        if (!SelectedSemesterId.HasValue || SelectedSemesterId <= 0)
        {
            await LoadDropdowns();
            ErrorMessage = "⨯ Semester is required. Please select a semester to generate the timetable.";
            return Page();
        }

        try
        {
            // Create scheduling request
            var request = new SchedulingRequest
            {
                AcademicYearId = SelectedAcademicYearId,
                SemesterId = SelectedSemesterId,
                ClassId = SelectedClassId,
                ClearExisting = ClearExisting,
                StartHour = StartHour,
                EndHour = EndHour,
                SlotDurationMinutes = SlotDurationMinutes
            };

            // Generate timetable
            Result = await _schedulingService.GenerateTimetableAsync(request);

            if (Result.Success)
            {
                SuccessMessage = $"✓ Timetable generated successfully! {Result.SlotsGenerated} slots created.";
                
                // Log to audit
                var details = $"Generated {Result.SlotsGenerated} timetable slots for semester {SelectedSemesterId ?? 0}";
                await _auditService.LogAsync("AUTO_GENERATE", "Timetable", SelectedSemesterId.ToString() ?? "All", 
                    null, details);

                // Show any soft constraint violations as warnings
                var softViolations = Result.Violations.Where(v => v.Type == "SoftConstraint").ToList();
                if (softViolations.Any())
                {
                    SuccessMessage += $"\n⚠ Warning: {softViolations.Count} soft constraints were relaxed.";
                }
            }
            else
            {
                ErrorMessage = "⨯ Failed to generate timetable: " + string.Join("; ", Result.Messages);
            }

            await LoadDropdowns();
            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"⨯ Error during generation: {ex.Message}";
            await LoadDropdowns();
            return Page();
        }
    }

    private async Task LoadDropdowns()
    {
        // Load academic years
        var academicYears = await _dbContext.AcademicYears
            .OrderByDescending(a => a.IsActive)
            .ThenByDescending(a => a.YearLabel)
            .ToListAsync();

        AcademicYears = new SelectList(academicYears, "Id", "YearLabel", SelectedAcademicYearId);

        // If no academic year selected, use the active one
        if (SelectedAcademicYearId == 0)
        {
            var activeYear = academicYears.FirstOrDefault(a => a.IsActive);
            if (activeYear != null)
                SelectedAcademicYearId = activeYear.Id;
        }

        // Load semesters
        if (SelectedAcademicYearId > 0)
        {
            var semesters = await _dbContext.Semesters
                .Where(s => s.AcademicYearId == SelectedAcademicYearId)
                .OrderBy(s => s.SemesterNumber)
                .ToListAsync();

            Semesters = new SelectList(semesters, "Id", "Name", SelectedSemesterId);

            // Auto-select first semester if none selected
            if ((!SelectedSemesterId.HasValue || SelectedSemesterId <= 0) && semesters.Any())
            {
                SelectedSemesterId = semesters.First().Id;
            }

            // Load classes if semester selected
            if (SelectedSemesterId.HasValue && SelectedSemesterId > 0)
            {
                var semester = semesters.FirstOrDefault(s => s.Id == SelectedSemesterId);
                if (semester != null)
                {
                    var classes = await _dbContext.ClassBatches
                        .Where(c => c.AcademicYearId == SelectedAcademicYearId && 
                               c.Semester == semester.SemesterNumber &&
                               c.DepartmentId > 0)
                        .OrderBy(c => c.BatchName)
                        .ToListAsync();

                    Classes = new SelectList(classes, "Id", "BatchName", SelectedClassId);
                }
            }
        }
    }
}
