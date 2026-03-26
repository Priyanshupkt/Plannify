using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plannify.Models;

public class Subject
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;

    public int Credits { get; set; }

    [Required]
    public string SubjectType { get; set; } = "Theory";

    [ForeignKey("Department")]
    public int DepartmentId { get; set; }

    public int SemesterNumber { get; set; }

    public virtual Department? Department { get; set; }
    public virtual List<TimetableSlot> TimetableSlots { get; set; } = new();
}
