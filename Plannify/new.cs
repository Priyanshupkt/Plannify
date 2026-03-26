I am working on an ASP.NET Core Razor Pages project using Entity Framework Core.

I am getting the following error:
**'Teacher' is a namespace but is used like a type**

Here is my current PageModel code:

```csharp
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
```

Please do the following:

1. Identify the exact cause of the error.
2. Fix any namespace conflicts in the `Teacher` model.
3. Provide the corrected `Teacher.cs` model file.
4. Provide the corrected `TeachersModel` if needed.
5. Follow best practices for ASP.NET Core and EF Core.
6. Keep the code clean, minimal, and production-ready.

Also explain briefly what was wrong and why the fix works.
