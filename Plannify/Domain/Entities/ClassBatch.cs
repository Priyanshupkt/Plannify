using Plannify.Application.Common;

namespace Plannify.Domain.Entities;

/// <summary>
/// ClassBatch domain entity
/// Represents a batch/class of students
/// </summary>
public class ClassBatch
{
    // Parameterless constructor for EF Core
    public ClassBatch()
    {
        BatchName = string.Empty;
    }

    private ClassBatch(int id, string batchName, int strength, int semester, int departmentId, int academicYearId, int? roomId = null)
    {
        Id = id;
        BatchName = batchName;
        Strength = strength;
        Semester = semester;
        DepartmentId = departmentId;
        AcademicYearId = academicYearId;
        RoomId = roomId;
    }

    public int Id { get; set; }
    public string BatchName { get; set; }
    public int Strength { get; set; }
    public int Semester { get; set; }
    public int DepartmentId { get; set; }
    public int AcademicYearId { get; set; }
    public int? RoomId { get; set; }
    public bool IsActive { get; private set; } = true;
    
    // Navigation properties
    public Department? Department { get; set; }
    public AcademicYear? AcademicYear { get; set; }
    public Room? Room { get; set; }
    public ICollection<TimetableSlot>? TimetableSlots { get; set; }

    /// <summary>
    /// Factory method to create a new ClassBatch with business rule validation
    /// </summary>
    public static Result<ClassBatch> Create(string batchName, int strength, int semester, int departmentId, int academicYearId, int? roomId = null)
    {
        // Validate batch name
        if (string.IsNullOrWhiteSpace(batchName))
            return Result<ClassBatch>.Failure("Batch name is required");

        if (batchName.Length < 2 || batchName.Length > 100)
            return Result<ClassBatch>.Failure("Batch name must be between 2 and 100 characters");

        // Validate strength
        if (strength < 1 || strength > 500)
            return Result<ClassBatch>.Failure("Class strength must be between 1 and 500 students");

        // Validate semester
        if (semester < 1 || semester > 8)
            return Result<ClassBatch>.Failure("Semester must be between 1 and 8");

        // Validate department ID
        if (departmentId <= 0)
            return Result<ClassBatch>.Failure("Valid department ID is required");

        // Validate academic year ID
        if (academicYearId <= 0)
            return Result<ClassBatch>.Failure("Valid academic year ID is required");

        // Validate room ID if provided
        if (roomId.HasValue && roomId <= 0)
            return Result<ClassBatch>.Failure("Invalid room ID");

        return Result<ClassBatch>.Success(new ClassBatch(0, batchName, strength, semester, departmentId, academicYearId, roomId));
    }

    /// <summary>
    /// Update class batch details with validation
    /// </summary>
    public Result Update(string batchName, int strength, int semester, int? roomId = null)
    {
        // Validate batch name
        if (string.IsNullOrWhiteSpace(batchName))
            return Result.Failure("Batch name is required");

        if (batchName.Length < 2 || batchName.Length > 100)
            return Result.Failure("Batch name must be between 2 and 100 characters");

        // Validate strength
        if (strength < 1 || strength > 500)
            return Result.Failure("Class strength must be between 1 and 500 students");

        // Validate semester
        if (semester < 1 || semester > 8)
            return Result.Failure("Semester must be between 1 and 8");

        // Validate room ID if provided
        if (roomId.HasValue && roomId <= 0)
            return Result.Failure("Invalid room ID");

        BatchName = batchName;
        Strength = strength;
        Semester = semester;
        RoomId = roomId;

        return Result.Success();
    }

    /// <summary>
    /// Assign room to class batch
    /// </summary>
    public Result AssignRoom(int roomId)
    {
        if (roomId <= 0)
            return Result.Failure("Invalid room ID");

        RoomId = roomId;
        return Result.Success();
    }

    /// <summary>
    /// Remove room assignment from class batch
    /// </summary>
    public Result RemoveRoom()
    {
        RoomId = null;
        return Result.Success();
    }
}
