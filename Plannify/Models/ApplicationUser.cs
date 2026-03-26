using Microsoft.AspNetCore.Identity;

namespace Plannify.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "Teacher";
    public int? DepartmentId { get; set; }
    public int? TeacherId { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual Department? Department { get; set; }
    public virtual Teacher? Teacher { get; set; }
}
