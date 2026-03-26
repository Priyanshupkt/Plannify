using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plannify.Models;

public class Semester
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public int SemesterNumber { get; set; }

    [ForeignKey("AcademicYear")]
    public int AcademicYearId { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }

    public virtual AcademicYear? AcademicYear { get; set; }
    public virtual List<TimetableSlot> TimetableSlots { get; set; } = new();
}
