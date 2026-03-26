using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Plannify.Data;
using Plannify.Models;

namespace Plannify.Pages.Admin;

public class TeachersModel(AppDbContext dbContext) : PageModel
{
    private readonly AppDbContext _dbContext = dbContext;

    public List<Teacher> Teachers { get; set; } = new();

    [BindProperty]
    public Teacher NewTeacher { get; set; } = new();

    public async Task OnGetAsync()
    {
        Teachers = await Task.FromResult(_dbContext.Teachers.ToList());
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        _dbContext.Teachers.Add(NewTeacher);
        await _dbContext.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var teacher = await _dbContext.Teachers.FindAsync(id);
        if (teacher != null)
        {
            _dbContext.Teachers.Remove(teacher);
            await _dbContext.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}
