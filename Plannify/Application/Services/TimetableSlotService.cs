using AutoMapper;
using Plannify.Application.Common;
using Plannify.Application.Contracts;
using Plannify.Application.DTOs;
using DomainTimetableSlot = Plannify.Domain.Entities.TimetableSlot;
using Plannify.Services;

namespace Plannify.Application.Services;

/// <summary>
/// TimetableSlot service - following clean architecture pattern
/// All timetable slot business logic orchestrated here
/// </summary>
public class TimetableSlotService : ITimetableSlotService
{
    private readonly ITimetableSlotRepository _repository;
    private readonly AuditService _auditService;
    private readonly IMapper _mapper;

    public TimetableSlotService(
        ITimetableSlotRepository repository,
        AuditService auditService,
        IMapper mapper)
    {
        _repository = repository;
        _auditService = auditService;
        _mapper = mapper;
    }

    public async Task<Result<TimetableSlotDto>> GetByIdAsync(int id)
    {
        try
        {
            var slot = await _repository.GetByIdAsync(id);
            if (slot == null)
                return Result<TimetableSlotDto>.Failure("Timetable slot not found");

            var dto = _mapper.Map<TimetableSlotDto>(slot);
            return Result<TimetableSlotDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<TimetableSlotDto>.Failure($"Error retrieving timetable slot: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<TimetableSlotDto>>> GetAllAsync()
    {
        try
        {
            var slots = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<TimetableSlotDto>>(slots);
            return Result<IEnumerable<TimetableSlotDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<TimetableSlotDto>>.Failure($"Error retrieving timetable slots: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<TimetableSlotDto>>> GetBySemesterAsync(int semesterId)
    {
        try
        {
            if (semesterId <= 0)
                return Result<IEnumerable<TimetableSlotDto>>.Failure("Valid semester ID is required");

            var slots = await _repository.GetBySemesterAsync(semesterId);
            var dtos = _mapper.Map<IEnumerable<TimetableSlotDto>>(slots);
            return Result<IEnumerable<TimetableSlotDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<TimetableSlotDto>>.Failure($"Error retrieving timetable slots: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<TimetableSlotDto>>> GetByClassBatchAsync(int classBatchId)
    {
        try
        {
            if (classBatchId <= 0)
                return Result<IEnumerable<TimetableSlotDto>>.Failure("Valid class batch ID is required");

            var slots = await _repository.GetByClassBatchAsync(classBatchId);
            var dtos = _mapper.Map<IEnumerable<TimetableSlotDto>>(slots);
            return Result<IEnumerable<TimetableSlotDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<TimetableSlotDto>>.Failure($"Error retrieving timetable slots: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<TimetableSlotDto>>> GetByTeacherAsync(int teacherId)
    {
        try
        {
            if (teacherId <= 0)
                return Result<IEnumerable<TimetableSlotDto>>.Failure("Valid teacher ID is required");

            var slots = await _repository.GetByTeacherAsync(teacherId);
            var dtos = _mapper.Map<IEnumerable<TimetableSlotDto>>(slots);
            return Result<IEnumerable<TimetableSlotDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<TimetableSlotDto>>.Failure($"Error retrieving timetable slots: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<TimetableSlotDto>>> GetByRoomAsync(int roomId)
    {
        try
        {
            if (roomId <= 0)
                return Result<IEnumerable<TimetableSlotDto>>.Failure("Valid room ID is required");

            var slots = await _repository.GetByRoomAsync(roomId);
            var dtos = _mapper.Map<IEnumerable<TimetableSlotDto>>(slots);
            return Result<IEnumerable<TimetableSlotDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<TimetableSlotDto>>.Failure($"Error retrieving timetable slots: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<TimetableSlotDto>>> GetBySubjectAsync(int subjectId)
    {
        try
        {
            if (subjectId <= 0)
                return Result<IEnumerable<TimetableSlotDto>>.Failure("Valid subject ID is required");

            var slots = await _repository.GetBySubjectAsync(subjectId);
            var dtos = _mapper.Map<IEnumerable<TimetableSlotDto>>(slots);
            return Result<IEnumerable<TimetableSlotDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<TimetableSlotDto>>.Failure($"Error retrieving timetable slots: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<TimetableSlotDto>>> GetClassTimetableByDayAsync(int classBatchId, string day)
    {
        try
        {
            if (classBatchId <= 0)
                return Result<IEnumerable<TimetableSlotDto>>.Failure("Valid class batch ID is required");

            var slots = await _repository.GetByClassBatchAndDayAsync(classBatchId, day);
            var dtos = _mapper.Map<IEnumerable<TimetableSlotDto>>(slots);
            return Result<IEnumerable<TimetableSlotDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<TimetableSlotDto>>.Failure($"Error retrieving class timetable: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<TimetableSlotDto>>> GetTeacherTimetableByDayAsync(int teacherId, string day)
    {
        try
        {
            if (teacherId <= 0)
                return Result<IEnumerable<TimetableSlotDto>>.Failure("Valid teacher ID is required");

            var slots = await _repository.GetByTeacherAndDayAsync(teacherId, day);
            var dtos = _mapper.Map<IEnumerable<TimetableSlotDto>>(slots);
            return Result<IEnumerable<TimetableSlotDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<TimetableSlotDto>>.Failure($"Error retrieving teacher timetable: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<TimetableSlotDto>>> GetRoomScheduleByDayAsync(int roomId, string day)
    {
        try
        {
            if (roomId <= 0)
                return Result<IEnumerable<TimetableSlotDto>>.Failure("Valid room ID is required");

            var slots = await _repository.GetByRoomAndDayAsync(roomId, day);
            var dtos = _mapper.Map<IEnumerable<TimetableSlotDto>>(slots);
            return Result<IEnumerable<TimetableSlotDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<TimetableSlotDto>>.Failure($"Error retrieving room schedule: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<TimetableSlotDto>>> GetLabSessionsByClassAsync(int classBatchId)
    {
        try
        {
            if (classBatchId <= 0)
                return Result<IEnumerable<TimetableSlotDto>>.Failure("Valid class batch ID is required");

            var slots = await _repository.GetLabSessionsByClassBatchAsync(classBatchId);
            var dtos = _mapper.Map<IEnumerable<TimetableSlotDto>>(slots);
            return Result<IEnumerable<TimetableSlotDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<TimetableSlotDto>>.Failure($"Error retrieving lab sessions: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<TimetableSlotSummaryDto>>> GetSummaryListAsync()
    {
        try
        {
            var slots = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<TimetableSlotSummaryDto>>(slots);
            return Result<IEnumerable<TimetableSlotSummaryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<TimetableSlotSummaryDto>>.Failure($"Error retrieving timetable slot list: {ex.Message}");
        }
    }

    public async Task<Result<int>> CreateAsync(CreateTimetableSlotRequest request)
    {
        try
        {
            if (request == null)
                return Result<int>.Failure("Invalid request");

            // Use domain business logic to create entity
            var createResult = DomainTimetableSlot.Create(
                request.SemesterId,
                request.Day,
                request.StartTime,
                request.EndTime,
                request.ClassBatchId,
                request.TeacherId,
                request.SubjectId,
                request.RoomId,
                request.SlotType,
                request.IsLabSession,
                request.LabGroupTag);

            if (!createResult.IsSuccess)
                return Result<int>.Failure(createResult.ErrorMessage ?? "Failed to create timetable slot");

            var slot = createResult.Value;

            // Check for conflicts
            if (slot.RoomId.HasValue && await _repository.IsRoomOccupiedAsync(slot.RoomId.Value, request.Day, request.StartTime, request.EndTime))
                return Result<int>.Failure($"Room is already occupied at this time");

            if (slot.TeacherId.HasValue && await _repository.IsTeacherOccupiedAsync(slot.TeacherId.Value, request.Day, request.StartTime, request.EndTime))
                return Result<int>.Failure($"Teacher is already scheduled at this time");

            if (await _repository.IsClassOccupiedAsync(request.ClassBatchId, request.Day, request.StartTime, request.EndTime))
                return Result<int>.Failure($"Class batch already has a session at this time");

            // Persist
            await _repository.AddAsync(slot);
            await _repository.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync("CREATE", "TimetableSlot", slot.Id.ToString(),
                null, $"Day: {request.Day}, Time: {request.StartTime:HH:mm}-{request.EndTime:HH:mm}, Class: {request.ClassBatchId}");

            return Result<int>.Success(slot.Id);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error creating timetable slot: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(UpdateTimetableSlotRequest request)
    {
        try
        {
            if (request == null || request.Id <= 0)
                return Result.Failure("Invalid request");

            var slot = await _repository.GetByIdAsync(request.Id);
            if (slot == null)
                return Result.Failure("Timetable slot not found");

            var oldValues = $"Day: {slot.Day}, Time: {slot.StartTime:HH:mm}-{slot.EndTime:HH:mm}";

            // Use domain method to update
            var updateResult = slot.Update(
                request.Day,
                request.StartTime,
                request.EndTime,
                request.TeacherId,
                request.SubjectId,
                request.RoomId,
                request.SlotType,
                request.IsLabSession,
                request.LabGroupTag);

            if (!updateResult.IsSuccess)
                return updateResult;

            // Check for conflicts (exclude current slot)
            if (slot.RoomId.HasValue && await _repository.IsRoomOccupiedAsync(slot.RoomId.Value, request.Day, request.StartTime, request.EndTime, request.Id))
                return Result.Failure($"Room is already occupied at this time");

            if (slot.TeacherId.HasValue && await _repository.IsTeacherOccupiedAsync(slot.TeacherId.Value, request.Day, request.StartTime, request.EndTime, request.Id))
                return Result.Failure($"Teacher is already scheduled at this time");

            if (await _repository.IsClassOccupiedAsync(slot.ClassBatchId, request.Day, request.StartTime, request.EndTime, request.Id))
                return Result.Failure($"Class batch already has a session at this time");

            // Persist
            await _repository.UpdateAsync(slot);
            await _repository.SaveChangesAsync();

            // Audit log
            var newValues = $"Day: {request.Day}, Time: {request.StartTime:HH:mm}-{request.EndTime:HH:mm}";
            await _auditService.LogAsync("UPDATE", "TimetableSlot", request.Id.ToString(), oldValues, newValues);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error updating timetable slot: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var slot = await _repository.GetByIdAsync(id);
            if (slot == null)
                return Result.Failure("Timetable slot not found");

            await _repository.DeleteAsync(slot);
            await _repository.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync("DELETE", "TimetableSlot", id.ToString(),
                $"Day: {slot.Day}, Time: {slot.StartTime:HH:mm}-{slot.EndTime:HH:mm}", null);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error deleting timetable slot: {ex.Message}");
        }
    }

    public async Task<Result> AssignTeacherAsync(int slotId, int teacherId)
    {
        try
        {
            var slot = await _repository.GetByIdAsync(slotId);
            if (slot == null)
                return Result.Failure("Timetable slot not found");

            // Check conflict with new teacher
            if (await _repository.IsTeacherOccupiedAsync(teacherId, slot.Day, slot.StartTime, slot.EndTime, slotId))
                return Result.Failure($"Teacher is already scheduled at this time");

            var assignResult = slot.AssignTeacher(teacherId);
            if (!assignResult.IsSuccess)
                return assignResult;

            await _repository.UpdateAsync(slot);
            await _repository.SaveChangesAsync();

            await _auditService.LogAsync("UPDATE", "TimetableSlot", slotId.ToString(),
                $"Teacher: {slot.TeacherId}", $"Teacher: {teacherId}");

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error assigning teacher: {ex.Message}");
        }
    }

    public async Task<Result> RemoveTeacherAsync(int slotId)
    {
        try
        {
            var slot = await _repository.GetByIdAsync(slotId);
            if (slot == null)
                return Result.Failure("Timetable slot not found");

            var oldTeacherId = slot.TeacherId;
            var removeResult = slot.RemoveTeacher();
            
            if (!removeResult.IsSuccess)
                return removeResult;

            await _repository.UpdateAsync(slot);
            await _repository.SaveChangesAsync();

            await _auditService.LogAsync("UPDATE", "TimetableSlot", slotId.ToString(),
                $"Teacher: {oldTeacherId}", "Teacher: null");

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error removing teacher assignment: {ex.Message}");
        }
    }

    public async Task<Result> AssignSubjectAsync(int slotId, int subjectId)
    {
        try
        {
            var slot = await _repository.GetByIdAsync(slotId);
            if (slot == null)
                return Result.Failure("Timetable slot not found");

            var assignResult = slot.AssignSubject(subjectId);
            if (!assignResult.IsSuccess)
                return assignResult;

            await _repository.UpdateAsync(slot);
            await _repository.SaveChangesAsync();

            await _auditService.LogAsync("UPDATE", "TimetableSlot", slotId.ToString(),
                $"Subject: {slot.SubjectId}", $"Subject: {subjectId}");

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error assigning subject: {ex.Message}");
        }
    }

    public async Task<Result> RemoveSubjectAsync(int slotId)
    {
        try
        {
            var slot = await _repository.GetByIdAsync(slotId);
            if (slot == null)
                return Result.Failure("Timetable slot not found");

            var removeResult = slot.RemoveSubject();
            if (!removeResult.IsSuccess)
                return removeResult;

            await _repository.UpdateAsync(slot);
            await _repository.SaveChangesAsync();

            await _auditService.LogAsync("UPDATE", "TimetableSlot", slotId.ToString(),
                "Subject: assigned", "Subject: null");

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error removing subject assignment: {ex.Message}");
        }
    }

    public async Task<Result> AssignRoomAsync(int slotId, int roomId)
    {
        try
        {
            var slot = await _repository.GetByIdAsync(slotId);
            if (slot == null)
                return Result.Failure("Timetable slot not found");

            // Check conflict with new room
            if (await _repository.IsRoomOccupiedAsync(roomId, slot.Day, slot.StartTime, slot.EndTime, slotId))
                return Result.Failure($"Room is already occupied at this time");

            var assignResult = slot.AssignRoom(roomId);
            if (!assignResult.IsSuccess)
                return assignResult;

            await _repository.UpdateAsync(slot);
            await _repository.SaveChangesAsync();

            await _auditService.LogAsync("UPDATE", "TimetableSlot", slotId.ToString(),
                $"Room: {slot.RoomId}", $"Room: {roomId}");

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error assigning room: {ex.Message}");
        }
    }

    public async Task<Result> RemoveRoomAsync(int slotId)
    {
        try
        {
            var slot = await _repository.GetByIdAsync(slotId);
            if (slot == null)
                return Result.Failure("Timetable slot not found");

            var oldRoomId = slot.RoomId;
            var removeResult = slot.RemoveRoom();
            
            if (!removeResult.IsSuccess)
                return removeResult;

            await _repository.UpdateAsync(slot);
            await _repository.SaveChangesAsync();

            await _auditService.LogAsync("UPDATE", "TimetableSlot", slotId.ToString(),
                $"Room: {oldRoomId}", "Room: null");

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error removing room assignment: {ex.Message}");
        }
    }
}
