using System.ComponentModel.DataAnnotations;

namespace Plannify.Models;

public class AcademicYear
{
    public int Id { get; set; }

    [Required]
    public string YearLabel { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }

    public virtual List<Semester> Semesters { get; set; } = new();
}
