using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plannify.Models;

public class SubstitutionRecord
{
    public int Id { get; set; }

    [ForeignKey("TimetableSlot")]
    public int TimetableSlotId { get; set; }

    [ForeignKey("OriginalTeacher")]
    public int OriginalTeacherId { get; set; }

    [ForeignKey("SubstituteTeacher")]
    public int SubstituteTeacherId { get; set; }

    public DateOnly Date { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual TimetableSlot? TimetableSlot { get; set; }
    public virtual Teacher? OriginalTeacher { get; set; }
    public virtual Teacher? SubstituteTeacher { get; set; }
}
