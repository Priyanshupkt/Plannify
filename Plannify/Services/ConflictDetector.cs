using Microsoft.EntityFrameworkCore;
using Plannify.Application.Contracts;
using Plannify.Data;
using Plannify.Domain.Entities;

namespace Plannify.Services;

/// <summary>
/// Service to detect scheduling conflicts for teachers, rooms, and classes
/// </summary>
public class ConflictDetector : IConflictDetectorService
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

    /// <summary>
    /// Finds available time slots for a teacher on a specific day
    /// </summary>
    public async Task<List<(TimeOnly Start, TimeOnly End)>> GetAvailableTeacherSlotsAsync(
        int teacherId, string day, int semesterId, int durationMinutes = 60)
    {
        var busyTimes = await _context.TimetableSlots
            .AsNoTracking()
            .Where(s => s.TeacherId == teacherId && s.Day == day && 
                       s.SemesterId == semesterId && s.SlotType != "GAP")
            .OrderBy(s => s.StartTime)
            .ToListAsync();

        return FindAvailableSlots(busyTimes, durationMinutes);
    }

    /// <summary>
    /// Finds available time slots for a room on a specific day
    /// </summary>
    public async Task<List<(TimeOnly Start, TimeOnly End)>> GetAvailableRoomSlotsAsync(
        int roomId, string day, int semesterId, int durationMinutes = 60)
    {
        var busyTimes = await _context.TimetableSlots
            .AsNoTracking()
            .Where(s => s.RoomId == roomId && s.Day == day && 
                       s.SemesterId == semesterId && s.SlotType != "GAP")
            .OrderBy(s => s.StartTime)
            .ToListAsync();

        return FindAvailableSlots(busyTimes, durationMinutes);
    }

    /// <summary>
    /// Finds available time slots for a class on a specific day
    /// </summary>
    public async Task<List<(TimeOnly Start, TimeOnly End)>> GetAvailableClassSlotsAsync(
        int classBatchId, string day, int semesterId, int durationMinutes = 60)
    {
        var busyTimes = await _context.TimetableSlots
            .AsNoTracking()
            .Where(s => s.ClassBatchId == classBatchId && s.Day == day && 
                       s.SemesterId == semesterId && s.SlotType != "GAP")
            .OrderBy(s => s.StartTime)
            .ToListAsync();

        return FindAvailableSlots(busyTimes, durationMinutes);
    }

    /// <summary>
    /// Suggests alternative days/times if current slot conflicts
    /// Returns up to maxSuggestions with preferred day prioritized
    /// </summary>
    public async Task<List<TimeslotSuggestion>> SuggestAlternativeSlotsAsync(
        int teacherId, int? roomId, int classBatchId, TimeOnly requestedStart, TimeOnly requestedEnd,
        string preferredDay, int semesterId, int maxSuggestions = 3)
    {
        var suggestions = new List<TimeslotSuggestion>();
        var days = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
        var durationMinutes = (int)(requestedEnd - requestedStart).TotalMinutes;
        
        // Prioritize preferred day
        var daysToCheck = new List<string>();
        if (!string.IsNullOrEmpty(preferredDay) && days.Contains(preferredDay))
        {
            daysToCheck.Add(preferredDay);
            daysToCheck.AddRange(days.Where(d => d != preferredDay));
        }
        else
        {
            daysToCheck.AddRange(days);
        }

        // Search for available slots
        foreach (var day in daysToCheck)
        {
            if (suggestions.Count >= maxSuggestions) break;

            var availableSlots = await GetAvailableTeacherSlotsAsync(teacherId, day, semesterId, durationMinutes);
            
            foreach (var slot in availableSlots.Take(2))
            {
                // Only suggest slots that fit the requested duration
                if ((slot.End - slot.Start).TotalMinutes >= durationMinutes)
                {
                    var endTime = new TimeOnly(slot.Start.Hour, slot.Start.Minute).Add(TimeSpan.FromMinutes(durationMinutes));
                    suggestions.Add(new TimeslotSuggestion
                    {
                        Day = day,
                        StartTime = slot.Start,
                        EndTime = endTime,
                        Reason = day == preferredDay ? "✓ Available on preferred day" : $"Available on {day}"
                    });
                    
                    if (suggestions.Count >= maxSuggestions) break;
                }
            }
        }

        return suggestions;
    }

    /// <summary>
    /// Helper: Calculates available time gaps in a busy schedule
    /// Assumes working hours 08:00 - 18:00
    /// </summary>
    private List<(TimeOnly Start, TimeOnly End)> FindAvailableSlots(
        List<TimetableSlot> busyTimes, int durationMinutes)
    {
        var available = new List<(TimeOnly, TimeOnly)>();
        var dayStart = new TimeOnly(8, 0);    // 08:00 AM
        var dayEnd = new TimeOnly(18, 0);    // 06:00 PM

        if (!busyTimes.Any())
        {
            available.Add((dayStart, dayEnd));
            return available;
        }

        var current = dayStart;

        foreach (var busy in busyTimes)
        {
            if (busy.StartTime > current)
            {
                var gapDuration = (busy.StartTime - current).TotalMinutes;
                if (gapDuration >= durationMinutes)
                {
                    available.Add((current, busy.StartTime));
                }
            }
            current = busy.EndTime > current ? busy.EndTime : current;
        }

        // Check final gap
        if (current < dayEnd && (dayEnd - current).TotalMinutes >= durationMinutes)
        {
            available.Add((current, dayEnd));
        }

        return available;
    }
}

/// <summary>
/// Represents a suggested time slot for timetable creation
/// </summary>
public class TimeslotSuggestion
{
    public string Day { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Reason { get; set; } = string.Empty;
}
