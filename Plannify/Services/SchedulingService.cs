using Microsoft.EntityFrameworkCore;
using Plannify.Application.Contracts;
using Plannify.Data;
using Plannify.Models;

namespace Plannify.Services;

/// <summary>
/// Service for automatically generating optimized timetables using constraint satisfaction
/// </summary>
public class SchedulingService : ISchedulingService
{
    private readonly AppDbContext _dbContext;
    private readonly List<ConstraintViolation> _violations = new();

    public SchedulingService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Generate an optimized timetable for the specified criteria
    /// </summary>
    public async Task<SchedulingResult> GenerateTimetableAsync(SchedulingRequest request)
    {
        _violations.Clear();
        var result = new SchedulingResult();

        try
        {
            // Fetch required data
            var semester = await _dbContext.Semesters
                .Include(s => s.AcademicYear)
                .FirstOrDefaultAsync(s => s.AcademicYearId == request.AcademicYearId 
                    && (request.SemesterId == null || s.Id == request.SemesterId));

            if (semester == null)
            {
                result.Success = false;
                result.Messages.Add("Semester not found");
                return result;
            }

            // Get classes for this semester
            var query = _dbContext.ClassBatches
                .Include(c => c.Department)
                .AsQueryable();

            if (request.ClassId.HasValue)
                query = query.Where(c => c.Id == request.ClassId);
            else
                query = query.Where(c => c.Semester == semester.SemesterNumber 
                    && c.AcademicYearId == request.AcademicYearId);

            var classes = await query.ToListAsync();

            if (!classes.Any())
            {
                result.Success = false;
                result.Messages.Add("No classes found for scheduling");
                return result;
            }

            // Clear existing slots if requested
            if (request.ClearExisting)
            {
                var existingSlots = await _dbContext.TimetableSlots
                    .Where(t => t.SemesterId == semester.Id && 
                           classes.Select(c => c.Id).Contains(t.ClassBatchId))
                    .ToListAsync();
                _dbContext.TimetableSlots.RemoveRange(existingSlots);
                await _dbContext.SaveChangesAsync();
            }

            // Build scheduling data
            var assignments = await BuildClassSubjectAssignments(semester, classes);

            if (!assignments.Any())
            {
                // Diagnostic information to identify data mismatch
                var diagnostics = new List<string>();
                diagnostics.Add($"📋 DIAGNOSTIC INFO:");
                diagnostics.Add($"   Semester: {semester.Name} (ID: {semester.Id}, SemNum: {semester.SemesterNumber})");
                diagnostics.Add($"   Classes selected: {classes.Count}");
                
                foreach (var c in classes)
                {
                    var deptName = c.Department?.Name ?? "Unknown";
                    var subjectsInDept = await _dbContext.Subjects
                        .Where(s => s.DepartmentId == c.DepartmentId)
                        .CountAsync();
                    var subjectsForSem = await _dbContext.Subjects
                        .Where(s => s.DepartmentId == c.DepartmentId && 
                                    s.SemesterNumber == semester.SemesterNumber)
                        .CountAsync();
                    diagnostics.Add($"   • Class: {c.BatchName} (Dept:{deptName}, ID:{c.DepartmentId})");
                    diagnostics.Add($"     - Subjects in dept: {subjectsInDept} (all semesters)");
                    diagnostics.Add($"     - Subjects for semester {semester.SemesterNumber}: {subjectsForSem}");
                }
                
                var totalSubjects = await _dbContext.Subjects.CountAsync();
                var totalDepts = await _dbContext.Departments.CountAsync();
                diagnostics.Add($"   Total subjects in database: {totalSubjects}");
                diagnostics.Add($"   Total departments: {totalDepts}");
                
                result.Success = false;
                result.Messages.Add("❌ No subjects found for the specified classes");
                result.Messages.AddRange(diagnostics);
                result.Messages.Add("💡 FIX: Verify subjects are assigned to the correct department AND semester");
                return result;
            }

            // Get available rooms
            var rooms = await _dbContext.Rooms
                .Where(r => r.IsActive)
                .ToListAsync();

            if (!rooms.Any())
            {
                result.Success = false;
                result.Messages.Add("No available rooms for scheduling");
                return result;
            }

            // Validate time parameters BEFORE generating slots
            if (request.StartHour >= request.EndHour)
            {
                result.Success = false;
                result.Messages.Add("Start hour must be less than end hour");
                return result;
            }

            if (request.SlotDurationMinutes <= 0)
            {
                result.Success = false;
                result.Messages.Add("Slot duration must be positive");
                return result;
            }

            // Generate time slots for the week
            var timeSlots = GenerateTimeSlots(request.StartHour, request.EndHour, request.SlotDurationMinutes);

            // Validate that slots were generated
            if (!timeSlots.Any())
            {
                result.Success = false;
                result.Messages.Add("No valid time slots generated with the given parameters");
                return result;
            }

            // Attempt to schedule each assignment
            var generatedSlots = 0;
            foreach (var assignment in assignments)
            {
                var slotsScheduled = await ScheduleClassSubject(assignment, semester, timeSlots, rooms);
                generatedSlots += slotsScheduled;
            }

            result.Success = true;
            result.SlotsGenerated = generatedSlots;
            result.ConflictsDetected = _violations.Count(v => v.Type == "HardConstraint");
            result.Violations = _violations;

            if (generatedSlots > 0)
                result.Messages.Add($"Successfully generated {generatedSlots} timetable slots");
            else
                result.Messages.Add("Warning: No slots could be generated");

            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Messages.Add($"Error during scheduling: {ex.Message}");
            return result;
        }
    }

    /// <summary>
    /// Build class-subject assignments from database
    /// </summary>
    private async Task<List<ClassSubjectAssignment>> BuildClassSubjectAssignments(Semester semester, List<ClassBatch> classes)
    {
        var assignments = new List<ClassSubjectAssignment>();
        var debugLog = new List<string>();

        foreach (var classBatch in classes)
        {
            var subjects = await _dbContext.Subjects
                .Where(s => s.DepartmentId == classBatch.DepartmentId && 
                       s.SemesterNumber == semester.SemesterNumber)
                .ToListAsync();

            debugLog.Add($"Processing class {classBatch.BatchName}: found {subjects.Count} subjects");

            foreach (var subject in subjects)
            {
                // FIXED: Get all active teachers from the department
                var teachers = await _dbContext.Teachers
                    .Where(t => t.DepartmentId == classBatch.DepartmentId && t.IsActive)
                    .Select(t => t.Id)
                    .ToListAsync();

                // If no teachers found, skip and log violation
                if (!teachers.Any())
                {
                    var teacherCount = await _dbContext.Teachers
                        .Where(t => t.DepartmentId == classBatch.DepartmentId)
                        .CountAsync();
                    debugLog.Add($"  ⚠ Subject '{subject.Name}': {teacherCount} total teachers in dept, 0 active");
                    
                    _violations.Add(new ConstraintViolation
                    {
                        Type = "HardConstraint",
                        Description = $"No active teachers in {classBatch.Department?.Name} for {subject.Name}",
                        Details = $"Total teachers: {teacherCount}, Active: 0"
                    });
                    continue;
                }

                assignments.Add(new ClassSubjectAssignment
                {
                    ClassBatchId = classBatch.Id,
                    SubjectId = subject.Id,
                    SubjectName = subject.Name,
                    SubjectType = subject.SubjectType,
                    AvailableTeacherIds = teachers,
                    ClassStrength = classBatch.Strength,
                    RequiredSessions = subject.SubjectType == "Lab" ? 2 : 1,
                    HoursPerSession = 2
                });
                
                debugLog.Add($"  ✓ Subject '{subject.Name}': {teachers.Count} teachers available");
            }
        }

        // Store debug info in violations if no assignments
        if (!assignments.Any() && debugLog.Any())
        {
            foreach (var log in debugLog)
            {
                _violations.Add(new ConstraintViolation
                {
                    Type = "Info",
                    Description = log
                });
            }
        }

        return assignments;
    }

    /// <summary>
    /// Generate all possible time slots for a week
    /// </summary>
    private List<(string Day, TimeOnly StartTime, TimeOnly EndTime)> GenerateTimeSlots(int startHour, int endHour, int slotDurationMinutes)
    {
        var slots = new List<(string, TimeOnly, TimeOnly)>();
        var days = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };

        foreach (var day in days)
        {
            var currentTime = new TimeOnly(startHour, 0);
            var endTime = new TimeOnly(endHour, 0);

            while (currentTime < endTime)
            {
                var slotEnd = currentTime.AddMinutes(slotDurationMinutes);
                if (slotEnd > endTime)
                    slotEnd = endTime;

                slots.Add((day, currentTime, slotEnd));
                currentTime = slotEnd;
            }
        }

        return slots;
    }

    /// <summary>
    /// Schedule a class-subject assignment
    /// </summary>
    private async Task<int> ScheduleClassSubject(ClassSubjectAssignment assignment, Semester semester,
        List<(string Day, TimeOnly StartTime, TimeOnly EndTime)> timeSlots, List<Room> rooms)
    {
        var slotsCreated = 0;

        // If no teachers available, log violation
        if (!assignment.AvailableTeacherIds.Any())
        {
            _violations.Add(new ConstraintViolation
            {
                Type = "HardConstraint",
                Description = $"No teachers available for {assignment.SubjectName}",
                Details = $"Class: {assignment.ClassBatchId}"
            });
            return 0;
        }

        // Schedule required sessions
        for (int session = 0; session < assignment.RequiredSessions; session++)
        {
            var (slotCreated, violations) = await TryScheduleSlot(assignment, semester, timeSlots, rooms, assignment.HoursPerSession);
            if (slotCreated)
                slotsCreated++;
            _violations.AddRange(violations);
        }

        return slotsCreated;
    }

    /// <summary>
    /// Try to schedule a single slot, return (success, violations)
    /// </summary>
    private async Task<(bool Success, List<ConstraintViolation> Violations)> TryScheduleSlot(
        ClassSubjectAssignment assignment, Semester semester,
        List<(string Day, TimeOnly StartTime, TimeOnly EndTime)> availableSlots, List<Room> rooms, int hoursNeeded)
    {
        var localViolations = new List<ConstraintViolation>();

        // Find an available teacher for this subject
        var teacher = assignment.AvailableTeacherIds.FirstOrDefault();
        if (teacher == 0)
        {
            localViolations.Add(new ConstraintViolation
            {
                Type = "HardConstraint",
                Description = $"No teacher available for subject {assignment.SubjectName}",
            });
            return (false, localViolations);
        }

        // Find a room with sufficient capacity
        var suitableRoom = rooms.FirstOrDefault(r => r.Capacity >= assignment.ClassStrength);
        if (suitableRoom == null)
        {
            suitableRoom = rooms.FirstOrDefault(); // Fallback to any room
            if (suitableRoom == null)
            {
                localViolations.Add(new ConstraintViolation
                {
                    Type = "HardConstraint",
                    Description = "No available rooms for scheduling"
                });
                return (false, localViolations);
            }
        }

        // Find an available slot (not conflicting)
        var availableSlot = await FindAvailableSlot(assignment.ClassBatchId, suitableRoom.Id, 
            availableSlots, semester.Id, teacher);

        if (availableSlot == null)
        {
            localViolations.Add(new ConstraintViolation
            {
                Type = "SoftConstraint",
                Description = $"Could not find available slot for {assignment.SubjectName}",
                Details = $"Class: {assignment.ClassBatchId}, Room: {suitableRoom.RoomNumber}"
            });
            return (false, localViolations);
        }

        // Create the timetable slot
        var (day, startTime, endTime) = availableSlot.Value;
        var slot = new TimetableSlot
        {
            SemesterId = semester.Id,
            Day = day,
            StartTime = startTime,
            EndTime = endTime,
            ClassBatchId = assignment.ClassBatchId,
            SubjectId = assignment.SubjectId,
            TeacherId = teacher,
            RoomId = suitableRoom.Id,
            SlotType = assignment.SubjectType,
            IsLabSession = assignment.SubjectType == "Lab",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "AutoScheduling"
        };

        try
        {
            _dbContext.TimetableSlots.Add(slot);
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            localViolations.Add(new ConstraintViolation
            {
                Type = "HardConstraint",
                Description = $"Database error saving slot: {ex.InnerException?.Message ?? ex.Message}",
                Details = $"Subject: {assignment.SubjectName}"
            });
            return (false, localViolations);
        }

        return (true, localViolations);
    }

    /// <summary>
    /// Find an available slot that doesn't conflict with existing bookings
    /// </summary>
    private async Task<(string Day, TimeOnly StartTime, TimeOnly EndTime)?> FindAvailableSlot(
        int classId, int roomId, List<(string Day, TimeOnly StartTime, TimeOnly EndTime)> slots,
        int semesterId, int teacherId)
    {
        foreach (var (day, startTime, endTime) in slots.OrderBy(s => s.Day).ThenBy(s => s.StartTime))
        {
            // Fetch all bookings for this day in one query (no custom methods in LINQ)
            var dayBookings = await _dbContext.TimetableSlots
                .Where(t => t.SemesterId == semesterId && t.Day == day)
                .ToListAsync();

            // Check conflicts client-side (after fetching from DB)
            var teacherConflict = dayBookings.Any(t => t.TeacherId == teacherId &&
                       TimeConflict(t.StartTime, t.EndTime, startTime, endTime));

            if (teacherConflict)
                continue;

            var roomConflict = dayBookings.Any(t => t.RoomId == roomId &&
                       TimeConflict(t.StartTime, t.EndTime, startTime, endTime));

            if (roomConflict)
                continue;

            var classConflict = dayBookings.Any(t => t.ClassBatchId == classId &&
                       TimeConflict(t.StartTime, t.EndTime, startTime, endTime));

            if (classConflict)
                continue;

            // Slot is available
            return (day, startTime, endTime);
        }

        return null;
    }

    /// <summary>
    /// Generate optimized time slots that minimize gaps in the school day (9:00-16:00)
    /// </summary>
    private List<(string Day, TimeOnly StartTime, TimeOnly EndTime)> GenerateOptimizedTimeSlots(int startHour = 9, int endHour = 16, int slotDurationMinutes = 50)
    {
        // Generate slots for 9-16:00 window with minimal gaps
        var slots = new List<(string, TimeOnly, TimeOnly)>();
        var days = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };

        foreach (var day in days)
        {
            var currentTime = new TimeOnly(startHour, 0);
            var schoolEndTime = new TimeOnly(endHour, 0);

            while (currentTime.AddMinutes(slotDurationMinutes) <= schoolEndTime)
            {
                var slotEnd = currentTime.AddMinutes(slotDurationMinutes);
                slots.Add((day, currentTime, slotEnd));
                
                // Add small break (5-10 minutes) between slots
                currentTime = slotEnd.AddMinutes(5);
            }
        }

        return slots;
    }

    /// <summary>
    /// Check if two time windows conflict
    /// </summary>
    private bool TimeConflict(TimeOnly start1, TimeOnly end1, TimeOnly start2, TimeOnly end2)
    {
        return start1 < end2 && start2 < end1;
    }
}
