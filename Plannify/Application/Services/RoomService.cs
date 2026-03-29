using AutoMapper;
using Plannify.Application.Common;
using Plannify.Application.Contracts;
using Plannify.Application.DTOs;
using DomainRoom = Plannify.Domain.Entities.Room;
using Plannify.Services;

namespace Plannify.Application.Services;

/// <summary>
/// Room service - following same pattern as TeacherService and DepartmentService
/// All room business logic orchestrated here
/// </summary>
public class RoomService : IRoomService
{
    private readonly IRoomRepository _repository;
    private readonly AuditService _auditService;
    private readonly IMapper _mapper;

    public RoomService(
        IRoomRepository repository,
        AuditService auditService,
        IMapper mapper)
    {
        _repository = repository;
        _auditService = auditService;
        _mapper = mapper;
    }

    public async Task<Result<RoomDto>> GetByIdAsync(int id)
    {
        try
        {
            var room = await _repository.GetByIdAsync(id);
            if (room == null)
                return Result<RoomDto>.Failure("Room not found");

            var dto = _mapper.Map<RoomDto>(room);
            return Result<RoomDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<RoomDto>.Failure($"Error retrieving room: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<RoomDto>>> GetAllAsync()
    {
        try
        {
            var rooms = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<RoomDto>>(rooms);
            return Result<IEnumerable<RoomDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<RoomDto>>.Failure($"Error retrieving rooms: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<RoomSummaryDto>>> GetSummaryListAsync()
    {
        try
        {
            var rooms = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<RoomSummaryDto>>(rooms);
            return Result<IEnumerable<RoomSummaryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<RoomSummaryDto>>.Failure($"Error retrieving room list: {ex.Message}");
        }
    }

    public async Task<Result<int>> CreateAsync(CreateRoomRequest request)
    {
        try
        {
            if (request == null)
                return Result<int>.Failure("Invalid request");

            // Check for duplicate room number
            if (await _repository.RoomNumberExistsAsync(request.RoomNumber))
                return Result<int>.Failure($"Room '{request.RoomNumber}' already exists");

            // Use domain business logic to create entity
            var createResult = DomainRoom.Create(request.RoomNumber, request.BuildingName, request.Capacity, request.RoomType);

            if (!createResult.IsSuccess)
                return Result<int>.Failure(createResult.ErrorMessage ?? "Failed to create room");

            var room = createResult.Value;

            // Persist
            await _repository.AddAsync(room);
            await _repository.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync("CREATE", "Room", room.Id.ToString(),
                null, $"Number: {request.RoomNumber}, Building: {request.BuildingName}, Capacity: {request.Capacity}, Type: {request.RoomType}");

            return Result<int>.Success(room.Id);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error creating room: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(UpdateRoomRequest request)
    {
        try
        {
            if (request == null || request.Id <= 0)
                return Result.Failure("Invalid request");

            var room = await _repository.GetByIdAsync(request.Id);
            if (room == null)
                return Result.Failure("Room not found");

            // Check for duplicate room number (if changed)
            if (room.RoomNumber != request.RoomNumber &&
                await _repository.RoomNumberExistsAsync(request.RoomNumber, request.Id))
                return Result.Failure($"Room number '{request.RoomNumber}' already exists");

            var oldValues = $"Number: {room.RoomNumber}, Building: {room.BuildingName}";

            // Use domain method to update
            var updateResult = room.Update(request.RoomNumber, request.BuildingName, request.Capacity, request.RoomType);

            if (!updateResult.IsSuccess)
                return updateResult;

            // Persist
            await _repository.UpdateAsync(room);
            await _repository.SaveChangesAsync();

            // Audit log
            var newValues = $"Number: {request.RoomNumber}, Building: {request.BuildingName}";
            await _auditService.LogAsync("UPDATE", "Room", request.Id.ToString(), oldValues, newValues);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error updating room: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var room = await _repository.GetByIdAsync(id);
            if (room == null)
                return Result.Failure("Room not found");

            await _repository.DeleteAsync(room);
            await _repository.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync("DELETE", "Room", id.ToString(),
                $"Number: {room.RoomNumber}, Building: {room.BuildingName}", null);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error deleting room: {ex.Message}");
        }
    }
}
