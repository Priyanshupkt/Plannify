using AutoMapper;
using Plannify.Application.Common;
using Plannify.Application.Contracts;
using Plannify.Application.DTOs;
using Plannify.Domain.Entities;
using Plannify.Services;

namespace Plannify.Application.Services;

/// <summary>
/// Service implementation for Timetable business logic
/// Orchestrates complex timetable operations and validates business rules
/// </summary>
public class TimetableService : ITimetableService
{
    private readonly ITimetableRepository _timetableRepository;
    private readonly ITimetableSlotRepository _slotRepository;
    private readonly AuditService _auditService;
    private readonly IMapper _mapper;

    public TimetableService(
        ITimetableRepository timetableRepository,
        ITimetableSlotRepository slotRepository,
        AuditService auditService,
        IMapper mapper)
    {
        _timetableRepository = timetableRepository;
        _slotRepository = slotRepository;
        _auditService = auditService;
        _mapper = mapper;
    }

    /// <summary>
    /// Get timetable by ID
    /// </summary>
    public async Task<Result<TimetableDto>> GetByIdAsync(int id)
    {
        if (id <= 0)
            return Result<TimetableDto>.Failure("Invalid timetable ID");

        var timetable = await _timetableRepository.GetWithSlotsAsync(id);

        if (timetable == null)
            return Result<TimetableDto>.Failure("Timetable not found");

        var dto = _mapper.Map<TimetableDto>(timetable);
        return Result<TimetableDto>.Success(dto);
    }

    /// <summary>
    /// Get all timetables
    /// </summary>
    public async Task<Result<IEnumerable<TimetableSummaryDto>>> GetAllAsync()
    {
        var timetables = await _timetableRepository.GetAllAsync();
        var dtos = _mapper.Map<IEnumerable<TimetableSummaryDto>>(timetables);
        return Result<IEnumerable<TimetableSummaryDto>>.Success(dtos);
    }

    /// <summary>
    /// Get timetables by semester
    /// </summary>
    public async Task<Result<IEnumerable<TimetableSummaryDto>>> GetBySemesterAsync(int semesterId)
    {
        if (semesterId <= 0)
            return Result<IEnumerable<TimetableSummaryDto>>.Failure("Invalid semester ID");

        var timetables = await _timetableRepository.GetBySemesterAsync(semesterId);
        var dtos = _mapper.Map<IEnumerable<TimetableSummaryDto>>(timetables);
        return Result<IEnumerable<TimetableSummaryDto>>.Success(dtos);
    }

    /// <summary>
    /// Get finalized timetables for a semester
    /// </summary>
    public async Task<Result<IEnumerable<TimetableSummaryDto>>> GetFinalizedBySemesterAsync(int semesterId)
    {
        if (semesterId <= 0)
            return Result<IEnumerable<TimetableSummaryDto>>.Failure("Invalid semester ID");

        var timetables = await _timetableRepository.GetFinalizedBySemesterAsync(semesterId);
        var dtos = _mapper.Map<IEnumerable<TimetableSummaryDto>>(timetables);
        return Result<IEnumerable<TimetableSummaryDto>>.Success(dtos);
    }

    /// <summary>
    /// Get active (non-finalized) timetables for a semester
    /// </summary>
    public async Task<Result<IEnumerable<TimetableSummaryDto>>> GetActiveBySemesterAsync(int semesterId)
    {
        if (semesterId <= 0)
            return Result<IEnumerable<TimetableSummaryDto>>.Failure("Invalid semester ID");

        var timetables = await _timetableRepository.GetActiveBySemesterAsync(semesterId);
        var dtos = _mapper.Map<IEnumerable<TimetableSummaryDto>>(timetables);
        return Result<IEnumerable<TimetableSummaryDto>>.Success(dtos);
    }

    /// <summary>
    /// Create a new timetable
    /// </summary>
    public async Task<Result<TimetableDto>> CreateAsync(CreateTimetableRequest request)
    {
        if (request == null)
            return Result<TimetableDto>.Failure("Request cannot be null");

        // Check for duplicate name
        var nameExists = await TimetableNameExistsAsync(request.Name, request.SemesterId);
        if (nameExists)
            return Result<TimetableDto>.Failure($"Timetable with name '{request.Name}' already exists for this semester");

        // Create domain entity with factory method
        var createResult = Timetable.Create(request.SemesterId, request.Name);
        if (!createResult.IsSuccess)
            return Result<TimetableDto>.Failure(createResult.ErrorMessage!);

        var timetable = createResult.Value!;

        // Add to repository
        await _timetableRepository.AddAsync(timetable);

        // Log audit
        await _auditService.LogAsync("Create", "Timetable", timetable.Id.ToString(), null, $"Timetable: {timetable.Name} created");

        var dto = _mapper.Map<TimetableDto>(timetable);
        return Result<TimetableDto>.Success(dto);
    }

    /// <summary>
    /// Update a timetable
    /// </summary>
    public async Task<Result<TimetableDto>> UpdateAsync(UpdateTimetableRequest request)
    {
        if (request == null)
            return Result<TimetableDto>.Failure("Request cannot be null");

        if (request.Id <= 0)
            return Result<TimetableDto>.Failure("Invalid timetable ID");

        var timetable = await _timetableRepository.GetByIdAsync(request.Id);
        if (timetable == null)
            return Result<TimetableDto>.Failure("Timetable not found");

        // Check for duplicate name
        var nameExists = await TimetableNameExistsAsync(request.Name, timetable.SemesterId, request.Id);
        if (nameExists)
            return Result<TimetableDto>.Failure($"Timetable with name '{request.Name}' already exists for this semester");

        // Update domain entity
        var (success, error) = timetable.Update(request.Name);
        if (!success)
            return Result<TimetableDto>.Failure(error!);

        // Persist changes
        await _timetableRepository.UpdateAsync(timetable);

        // Log audit
        await _auditService.LogAsync("Update", "Timetable", timetable.Id.ToString(), null, $"Name updated to: {timetable.Name}");

        var dto = _mapper.Map<TimetableDto>(timetable);
        return Result<TimetableDto>.Success(dto);
    }

    /// <summary>
    /// Delete a timetable
    /// </summary>
    public async Task<Result<string>> DeleteAsync(int id)
    {
        if (id <= 0)
            return Result<string>.Failure("Invalid timetable ID");

        var timetable = await _timetableRepository.GetByIdAsync(id);
        if (timetable == null)
            return Result<string>.Failure("Timetable not found");

        if (timetable.IsFinalized)
            return Result<string>.Failure("Cannot delete a finalized timetable");

        // Delete from repository
        await _timetableRepository.DeleteAsync(timetable);

        // Log audit
        await _auditService.LogAsync("Delete", "Timetable", id.ToString(), $"Timetable: {timetable.Name}", null);

        return Result<string>.Success("Timetable deleted successfully");
    }

    /// <summary>
    /// Finalize a timetable (lock against modifications)
    /// </summary>
    public async Task<Result<string>> FinalizeAsync(int id)
    {
        if (id <= 0)
            return Result<string>.Failure("Invalid timetable ID");

        var timetable = await _timetableRepository.GetWithSlotsAsync(id);
        if (timetable == null)
            return Result<string>.Failure("Timetable not found");

        // Attempt to finalize via domain entity
        var (success, error) = timetable.Finalize();
        if (!success)
            return Result<string>.Failure(error!);

        // Persist changes
        await _timetableRepository.UpdateAsync(timetable);

        // Log audit
        await _auditService.LogAsync("Finalize", "Timetable", timetable.Id.ToString(), null, $"Timetable finalized");

        return Result<string>.Success("Timetable finalized successfully");
    }

    /// <summary>
    /// Unfinalize a timetable (allow modifications)
    /// </summary>
    public async Task<Result<string>> UnfinalizeAsync(int id)
    {
        if (id <= 0)
            return Result<string>.Failure("Invalid timetable ID");

        var timetable = await _timetableRepository.GetByIdAsync(id);
        if (timetable == null)
            return Result<string>.Failure("Timetable not found");

        // Attempt to unfinalize via domain entity
        var (success, error) = timetable.Unfinalize();
        if (!success)
            return Result<string>.Failure(error!);

        // Persist changes
        await _timetableRepository.UpdateAsync(timetable);

        // Log audit
        await _auditService.LogAsync("Unfinalize", "Timetable", timetable.Id.ToString(), null, $"Timetable unfinalized");

        return Result<string>.Success("Timetable unfinalized successfully");
    }

    /// <summary>
    /// Get timetable statistics
    /// </summary>
    public async Task<Result<TimetableStatisticsDto>> GetStatisticsAsync(int id)
    {
        if (id <= 0)
            return Result<TimetableStatisticsDto>.Failure("Invalid timetable ID");

        var timetable = await _timetableRepository.GetWithSlotsAsync(id);
        if (timetable == null)
            return Result<TimetableStatisticsDto>.Failure("Timetable not found");

        var stats = timetable.GetStatistics();
        var dto = new TimetableStatisticsDto
        {
            TotalSlots = stats.TotalSlots,
            TotalClassBatches = stats.TotalClassBatches,
            TotalTeachers = stats.TotalTeachers,
            TotalRooms = stats.TotalRooms,
            TotalSubjects = stats.TotalSubjects,
            LabSessions = stats.LabSessions,
            UnassignedTeacherSlots = stats.UnassignedTeacherSlots,
            UnassignedSubjectSlots = stats.UnassignedSubjectSlots,
            UnassignedRoomSlots = stats.UnassignedRoomSlots,
            CompletionPercentage = stats.GetCompletionPercentage()
        };

        return Result<TimetableStatisticsDto>.Success(dto);
    }

    /// <summary>
    /// Get slots by class batch for a timetable
    /// </summary>
    public async Task<Result<IEnumerable<TimetableSlotSummaryDto>>> GetClassBatchSlotsAsync(int timetableId, int classBatchId)
    {
        if (timetableId <= 0)
            return Result<IEnumerable<TimetableSlotSummaryDto>>.Failure("Invalid timetable ID");

        if (classBatchId <= 0)
            return Result<IEnumerable<TimetableSlotSummaryDto>>.Failure("Invalid class batch ID");

        var timetable = await _timetableRepository.GetWithSlotsAsync(timetableId);
        if (timetable == null)
            return Result<IEnumerable<TimetableSlotSummaryDto>>.Failure("Timetable not found");

        var slots = timetable.GetClassBatchSlots(classBatchId);
        var dtos = _mapper.Map<IEnumerable<TimetableSlotSummaryDto>>(slots);
        return Result<IEnumerable<TimetableSlotSummaryDto>>.Success(dtos);
    }

    /// <summary>
    /// Get slots by teacher for a timetable
    /// </summary>
    public async Task<Result<IEnumerable<TimetableSlotSummaryDto>>> GetTeacherSlotsAsync(int timetableId, int teacherId)
    {
        if (timetableId <= 0)
            return Result<IEnumerable<TimetableSlotSummaryDto>>.Failure("Invalid timetable ID");

        if (teacherId <= 0)
            return Result<IEnumerable<TimetableSlotSummaryDto>>.Failure("Invalid teacher ID");

        var timetable = await _timetableRepository.GetWithSlotsAsync(timetableId);
        if (timetable == null)
            return Result<IEnumerable<TimetableSlotSummaryDto>>.Failure("Timetable not found");

        var slots = timetable.GetTeacherSlots(teacherId);
        var dtos = _mapper.Map<IEnumerable<TimetableSlotSummaryDto>>(slots);
        return Result<IEnumerable<TimetableSlotSummaryDto>>.Success(dtos);
    }

    /// <summary>
    /// Get slots by room for a timetable
    /// </summary>
    public async Task<Result<IEnumerable<TimetableSlotSummaryDto>>> GetRoomSlotsAsync(int timetableId, int roomId)
    {
        if (timetableId <= 0)
            return Result<IEnumerable<TimetableSlotSummaryDto>>.Failure("Invalid timetable ID");

        if (roomId <= 0)
            return Result<IEnumerable<TimetableSlotSummaryDto>>.Failure("Invalid room ID");

        var timetable = await _timetableRepository.GetWithSlotsAsync(timetableId);
        if (timetable == null)
            return Result<IEnumerable<TimetableSlotSummaryDto>>.Failure("Timetable not found");

        var slots = timetable.GetRoomSlots(roomId);
        var dtos = _mapper.Map<IEnumerable<TimetableSlotSummaryDto>>(slots);
        return Result<IEnumerable<TimetableSlotSummaryDto>>.Success(dtos);
    }

    /// <summary>
    /// Get slots by day for a timetable
    /// </summary>
    public async Task<Result<IEnumerable<TimetableSlotSummaryDto>>> GetDaySlotsAsync(int timetableId, string day)
    {
        if (timetableId <= 0)
            return Result<IEnumerable<TimetableSlotSummaryDto>>.Failure("Invalid timetable ID");

        if (string.IsNullOrWhiteSpace(day))
            return Result<IEnumerable<TimetableSlotSummaryDto>>.Failure("Day is required");

        var timetable = await _timetableRepository.GetWithSlotsAsync(timetableId);
        if (timetable == null)
            return Result<IEnumerable<TimetableSlotSummaryDto>>.Failure("Timetable not found");

        var slots = timetable.GetDaySlots(day);
        var dtos = _mapper.Map<IEnumerable<TimetableSlotSummaryDto>>(slots);
        return Result<IEnumerable<TimetableSlotSummaryDto>>.Success(dtos);
    }

    /// <summary>
    /// Check if timetable name exists for a semester
    /// </summary>
    public async Task<bool> TimetableNameExistsAsync(string name, int semesterId, int? excludeId = null)
    {
        return await _timetableRepository.TimetableNameExistsAsync(name, semesterId, excludeId);
    }

    /// <summary>
    /// Get full timetable with all slots and relationships
    /// </summary>
    public async Task<Result<TimetableDto>> GetFullTimetableAsync(int id)
    {
        if (id <= 0)
            return Result<TimetableDto>.Failure("Invalid timetable ID");

        var timetable = await _timetableRepository.GetWithSlotsAndNavigationAsync(id);
        if (timetable == null)
            return Result<TimetableDto>.Failure("Timetable not found");

        var dto = _mapper.Map<TimetableDto>(timetable);
        return Result<TimetableDto>.Success(dto);
    }
}
