using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Plannify.Application.Contracts;
using Plannify.Application.DTOs;

namespace Plannify.Pages.Admin.Classes;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly IClassBatchService _classBatchService;
    private readonly IDepartmentService _departmentService;
    private readonly IAcademicYearService _academicYearService;
    private readonly IRoomService _roomService;

    public IndexModel(IClassBatchService classBatchService, IDepartmentService departmentService, 
        IAcademicYearService academicYearService, IRoomService roomService)
    {
        _classBatchService = classBatchService;
        _departmentService = departmentService;
        _academicYearService = academicYearService;
        _roomService = roomService;
    }

    [BindProperty]
    public CreateClassBatchRequest NewClassBatch { get; set; } = new();

    public List<ClassBatchDto> ClassBatches { get; set; } = new();
    public List<DepartmentDto> Departments { get; set; } = new();
    public List<AcademicYearDto> AcademicYears { get; set; } = new();
    public List<RoomDto> Rooms { get; set; } = new();
    public Dictionary<int, int> SlotCounts { get; set; } = new();

    public async Task OnGetAsync()
    {
        var batchResult = await _classBatchService.GetAllAsync();
        if (batchResult.IsSuccess)
            ClassBatches = batchResult.Value?.ToList() ?? new();

        var deptResult = await _departmentService.GetAllAsync();
        if (deptResult.IsSuccess)
            Departments = deptResult.Value?.ToList() ?? new();

        var yearResult = await _academicYearService.GetAllAsync();
        if (yearResult.IsSuccess)
            AcademicYears = yearResult.Value?.ToList() ?? new();

        var roomResult = await _roomService.GetAllAsync();
        if (roomResult.IsSuccess)
            Rooms = roomResult.Value?.ToList() ?? new();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var result = await _classBatchService.CreateAsync(NewClassBatch);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to add class batch");
            await OnGetAsync();
            return Page();
        }

        TempData["Success"] = "Class batch added successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync(int id, string batchName, int strength, int departmentId, 
        int academicYearId, int semester, int? roomId)
    {
        var request = new UpdateClassBatchRequest
        {
            Id = id,
            BatchName = batchName,
            Strength = strength,
            DepartmentId = departmentId,
            AcademicYearId = academicYearId,
            Semester = semester,
            RoomId = roomId
        };

        var result = await _classBatchService.UpdateAsync(request);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage ?? "Failed to update class batch";
            await OnGetAsync();
            return Page();
        }

        TempData["Success"] = "Class batch updated successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await _classBatchService.DeleteAsync(id);
        if (!result.IsSuccess)
            TempData["Error"] = result.ErrorMessage ?? "Failed to delete class batch";
        else
            TempData["Success"] = "Class batch deleted successfully.";

        return RedirectToPage();
    }
}
