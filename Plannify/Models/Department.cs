using System.ComponentModel.DataAnnotations;

namespace Plannify.Models;

public class Department
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(20)]
    public string ShortName { get; set; } = string.Empty;

    public string? HODName { get; set; }

    public virtual List<Teacher> Teachers { get; set; } = new();
    public virtual List<Subject> Subjects { get; set; } = new();
    public virtual List<ClassBatch> ClassBatches { get; set; } = new();
}
