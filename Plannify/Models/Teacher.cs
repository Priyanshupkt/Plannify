using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plannify.Models;

public class Teacher
{
    public int Id { get; set; }

    [Required]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(5)]
    public string Initials { get; set; } = string.Empty;

    [Required]
    public string EmployeeCode { get; set; } = string.Empty;

    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [ForeignKey("Department")]
    public int DepartmentId { get; set; }

    public string Designation { get; set; } = "Assistant Professor";
    public int MaxWeeklyHours { get; set; } = 18;
    public bool IsActive { get; set; } = true;

    public virtual Department? Department { get; set; }
    public virtual List<TimetableSlot> TimetableSlots { get; set; } = new();
}