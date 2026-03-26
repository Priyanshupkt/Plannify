using Microsoft.EntityFrameworkCore;
using Plannify.Data;
using Plannify.Models;

namespace Plannify.Services;

/// <summary>
/// Service to detect scheduling conflicts for teachers, rooms, and classes
/// </summary>
public class ConflictDetector
{
    private readonly AppDbContext _context;

    public ConflictDetector(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Checks for time conflicts with a specific teacher's schedule
    /// </summary>
    public async Task<ConflictResult> CheckTeacherConflictAsync(
        int teacherId, string day, TimeOnly startTime, TimeOnly endTime,
        int semesterId, int? excludeSlotId = null)
    {
        var conflictingSlot = await _context.TimetableSlots
            .AsNoTracking()
            .Where(s =>
                s.TeacherId == teacherId &&
                s.SemesterId == semesterId &&
                s.Day == day &&
                s.SlotType != "GAP" &&
                (excludeSlotId == null || s.Id != excludeSlotId) &&
                // Time overlap formula: !(endTime <= existingStart || startTime >= existingEnd)
                !(endTime <= s.StartTime || startTime >= s.EndTime))
            .Include(s => s.Subject)
            .FirstOrDefaultAsync();

        if (conflictingSlot != null)
        {
            var message = $"Teacher conflict: Slot {conflictingSlot.Id} " +
                         $"({conflictingSlot.Subject?.Name} {conflictingSlot.StartTime:HH:mm}–{conflictingSlot.EndTime:HH:mm})";
            
            return new ConflictResult
            {
                HasConflict = true,
                ConflictingSlotId = conflictingSlot.Id,
                Message = message
            };
        }

        return new ConflictResult { HasConflict = false };
    }

    /// <summary>
    /// Checks for time conflicts with a specific room's schedule
    /// </summary>
    public async Task<ConflictResult> CheckRoomConflictAsync(
        int roomId, string day, TimeOnly startTime, TimeOnly endTime,
        int semesterId, int? excludeSlotId = null)
    {
        var conflictingSlot = await _context.TimetableSlots
            .AsNoTracking()
            .Where(s =>
                s.RoomId == roomId &&
                s.SemesterId == semesterId &&
                s.Day == day &&
                s.SlotType != "GAP" &&
                (excludeSlotId == null || s.Id != excludeSlotId) &&
                // Time overlap formula: !(endTime <= existingStart || startTime >= existingEnd)
                !(endTime <= s.StartTime || startTime >= s.EndTime))
            .Include(s => s.Subject)
            .Include(s => s.ClassBatch)
            .FirstOrDefaultAsync();

        if (conflictingSlot != null)
        {
            var message = $"Room conflict: Slot {conflictingSlot.Id} " +
                         $"({conflictingSlot.Subject?.Name} for {conflictingSlot.ClassBatch?.BatchName} " +
                         $"{conflictingSlot.StartTime:HH:mm}–{conflictingSlot.EndTime:HH:mm})";
            
            return new ConflictResult
            {
                HasConflict = true,
                ConflictingSlotId = conflictingSlot.Id,
                Message = message
            };
        }

        return new ConflictResult { HasConflict = false };
    }

    /// <summary>
    /// Checks for time conflicts with a specific class's schedule
    /// </summary>
    public async Task<ConflictResult> CheckClassConflictAsync(
        int classBatchId, string day, TimeOnly startTime, TimeOnly endTime,
        int semesterId, int? excludeSlotId = null)
    {
        var conflictingSlot = await _context.TimetableSlots
            .AsNoTracking()
            .Where(s =>
                s.ClassBatchId == classBatchId &&
                s.SemesterId == semesterId &&
                s.Day == day &&
                s.SlotType != "GAP" &&
                (excludeSlotId == null || s.Id != excludeSlotId) &&
                // Time overlap formula: !(endTime <= existingStart || startTime >= existingEnd)
                !(endTime <= s.StartTime || startTime >= s.EndTime))
            .Include(s => s.Subject)
            .Include(s => s.Teacher)
            .FirstOrDefaultAsync();

        if (conflictingSlot != null)
        {
            var message = $"Class conflict: Slot {conflictingSlot.Id} " +
                         $"({conflictingSlot.Subject?.Name} with {conflictingSlot.Teacher?.FullName} " +
                         $"{conflictingSlot.StartTime:HH:mm}–{conflictingSlot.EndTime:HH:mm})";
            
            return new ConflictResult
            {
                HasConflict = true,
                ConflictingSlotId = conflictingSlot.Id,
                Message = message
            };
        }

        return new ConflictResult { HasConflict = false };
    }

    /// <summary>
    /// Finds ALL overlapping slots in a semester grouped by conflict type
    /// </summary>
    public async Task<List<ConflictReport>> GetAllConflictsAsync(int semesterId)
    {
        var allSlots = await _context.TimetableSlots
            .AsNoTracking()
            .Where(s => s.SemesterId == semesterId && s.SlotType != "GAP")
            .Include(s => s.Subject)
            .Include(s => s.Teacher)
            .Include(s => s.Room)
            .Include(s => s.ClassBatch)
            .ToListAsync();

        var conflicts = new List<ConflictReport>();

        // Check all pairs of slots for overlaps
        for (int i = 0; i < allSlots.Count; i++)
        {
            for (int j = i + 1; j < allSlots.Count; j++)
            {
                var slot1 = allSlots[i];
                var slot2 = allSlots[j];

                // Skip if on different days
                if (slot1.Day != slot2.Day)
                    continue;

                // Check if times overlap
                if (slot1.EndTime <= slot1.StartTime || slot2.StartTime >= slot2.EndTime)
                    continue;

                // Teacher conflict
                if (slot1.TeacherId == slot2.TeacherId && slot1.TeacherId.HasValue)
                {
                    conflicts.Add(new ConflictReport
                    {
                        ConflictType = "TeacherConflict",
                        Day = slot1.Day,
                        StartTime = slot1.StartTime,
                        EndTime = slot1.EndTime,
                        Slot1Id = slot1.Id,
                        Slot1Description = $"{slot1.Subject?.Name} ({slot1.ClassBatch?.BatchName})",
                        Slot2Id = slot2.Id,
                        Slot2Description = $"{slot2.Subject?.Name} ({slot2.ClassBatch?.BatchName})",
                        AffectedEntity = slot1.Teacher?.FullName ?? string.Empty
                    });
                }

                // Room conflict
                if (slot1.RoomId == slot2.RoomId && slot1.RoomId.HasValue)
                {
                    conflicts.Add(new ConflictReport
                    {
                        ConflictType = "RoomConflict",
                        Day = slot1.Day,
                        StartTime = slot1.StartTime,
                        EndTime = slot1.EndTime,
                        Slot1Id = slot1.Id,
                        Slot1Description = $"{slot1.Subject?.Name} ({slot1.ClassBatch?.BatchName})",
                        Slot2Id = slot2.Id,
                        Slot2Description = $"{slot2.Subject?.Name} ({slot2.ClassBatch?.BatchName})",
                        AffectedEntity = slot1.Room?.RoomNumber ?? string.Empty
                    });
                }

                // Class conflict
                if (slot1.ClassBatchId == slot2.ClassBatchId)
                {
                    conflicts.Add(new ConflictReport
                    {
                        ConflictType = "ClassConflict",
                        Day = slot1.Day,
                        StartTime = slot1.StartTime,
                        EndTime = slot1.EndTime,
                        Slot1Id = slot1.Id,
                        Slot1Description = $"{slot1.Subject?.Name} with {slot1.Teacher?.FullName}",
                        Slot2Id = slot2.Id,
                        Slot2Description = $"{slot2.Subject?.Name} with {slot2.Teacher?.FullName}",
                        AffectedEntity = slot1.ClassBatch?.BatchName ?? string.Empty
                    });
                }
            }
        }

        return conflicts;
    }
}
