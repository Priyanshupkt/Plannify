using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plannify.Models;

public class TimetableSlot
{
    public int Id { get; set; }

    [ForeignKey("Semester")]
    public int SemesterId { get; set; }

    [Required]
    public string Day { get; set; } = string.Empty;

    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    [ForeignKey("Teacher")]
    public int? TeacherId { get; set; }

    [ForeignKey("Subject")]
    public int? SubjectId { get; set; }

    [ForeignKey("ClassBatch")]
    public int ClassBatchId { get; set; }

    [ForeignKey("Room")]
    public int? RoomId { get; set; }

    [Required]
    public string SlotType { get; set; } = "Theory";

    public bool IsLabSession { get; set; }
    public string? LabGroupTag { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    public virtual Semester? Semester { get; set; }
    public virtual Plannify.Models.Teacher? Teacher { get; set; }
    public virtual Plannify.Models.Subject? Subject { get; set; }
    public virtual Plannify.Models.ClassBatch? ClassBatch { get; set; }
    public virtual Room? Room { get; set; }
}

