using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Plannify.Data;
using Plannify.Models;

namespace Plannify.Pages.Admin;

public class SubjectsModel(AppDbContext dbContext) : PageModel
{
    private readonly AppDbContext _dbContext = dbContext;

    public List<Plannify.Models.Subject> Subjects { get; set; } = new();

    [BindProperty]
    public Plannify.Models.Subject NewSubject { get; set; } = new();

    public async Task OnGetAsync()
    {
        Subjects = await Task.FromResult(_dbContext.Subjects.ToList());
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        _dbContext.Subjects.Add(NewSubject);
        await _dbContext.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var subject = await _dbContext.Subjects.FindAsync(id);
        if (subject != null)
        {
            _dbContext.Subjects.Remove(subject);
            await _dbContext.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}
