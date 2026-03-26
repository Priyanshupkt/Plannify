using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Plannify.Data;
using Plannify.Models;

namespace Plannify.Pages.Admin;

public class ClassesModel(AppDbContext dbContext) : PageModel
{
    private readonly AppDbContext _dbContext = dbContext;

    public List<Plannify.Models.Class> Classes { get; set; } = new();

    [BindProperty]
    public Plannify.Models.Class NewClass { get; set; } = new();

    public async Task OnGetAsync()
    {
        Classes = await Task.FromResult(_dbContext.Classes.ToList());
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        _dbContext.Classes.Add(NewClass);
        await _dbContext.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var classItem = await _dbContext.Classes.FindAsync(id);
        if (classItem != null)
        {
            _dbContext.Classes.Remove(classItem);
            await _dbContext.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}
