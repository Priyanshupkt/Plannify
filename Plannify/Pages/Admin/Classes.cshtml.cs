using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Plannify.Data;
using Plannify.Models;

namespace Plannify.Pages.Admin;

public class ClassesModel(AppDbContext dbContext) : PageModel
{
    private readonly AppDbContext _dbContext = dbContext;

    public List<Plannify.Models.ClassBatch> Classes { get; set; } = new();

    [BindProperty]
    public Plannify.Models.ClassBatch NewClass { get; set; } = new();

    public async Task OnGetAsync()
    {
        Classes = await Task.FromResult(_dbContext.ClassBatches.ToList());
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        _dbContext.ClassBatches.Add(NewClass);
        await _dbContext.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var classItem = await _dbContext.ClassBatches.FindAsync(id);
        if (classItem != null)
        {
            _dbContext.ClassBatches.Remove(classItem);
            await _dbContext.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}
