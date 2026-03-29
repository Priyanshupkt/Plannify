using AutoMapper;
using Plannify.Application.Common;
using Plannify.Application.Contracts;
using Plannify.Application.DTOs;
using DomainClassBatch = Plannify.Domain.Entities.ClassBatch;
using Plannify.Services;

namespace Plannify.Application.Services;

/// <summary>
/// ClassBatch service - following clean architecture pattern
/// All class batch business logic orchestrated here
/// </summary>
public class ClassBatchService : IClassBatchService
{
    private readonly IClassBatchRepository _repository;
    private readonly AuditService _auditService;
    private readonly IMapper _mapper;

    public ClassBatchService(
        IClassBatchRepository repository,
        AuditService auditService,
        IMapper mapper)
    {
        _repository = repository;
        _auditService = auditService;
        _mapper = mapper;
    }

    public async Task<Result<ClassBatchDto>> GetByIdAsync(int id)
    {
        try
        {
            var classBatch = await _repository.GetByIdAsync(id);
            if (classBatch == null)
                return Result<ClassBatchDto>.Failure("Class batch not found");

            var dto = _mapper.Map<ClassBatchDto>(classBatch);
            return Result<ClassBatchDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<ClassBatchDto>.Failure($"Error retrieving class batch: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<ClassBatchDto>>> GetAllAsync()
    {
        try
        {
            var classBatches = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<ClassBatchDto>>(classBatches);
            return Result<IEnumerable<ClassBatchDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<ClassBatchDto>>.Failure($"Error retrieving class batches: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<ClassBatchDto>>> GetByDepartmentAsync(int departmentId)
    {
        try
        {
            if (departmentId <= 0)
                return Result<IEnumerable<ClassBatchDto>>.Failure("Valid department ID is required");

            var classBatches = await _repository.GetByDepartmentAsync(departmentId);
            var dtos = _mapper.Map<IEnumerable<ClassBatchDto>>(classBatches);
            return Result<IEnumerable<ClassBatchDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<ClassBatchDto>>.Failure($"Error retrieving class batches: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<ClassBatchDto>>> GetByAcademicYearAsync(int academicYearId)
    {
        try
        {
            if (academicYearId <= 0)
                return Result<IEnumerable<ClassBatchDto>>.Failure("Valid academic year ID is required");

            var classBatches = await _repository.GetByAcademicYearAsync(academicYearId);
            var dtos = _mapper.Map<IEnumerable<ClassBatchDto>>(classBatches);
            return Result<IEnumerable<ClassBatchDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<ClassBatchDto>>.Failure($"Error retrieving class batches: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<ClassBatchDto>>> GetByDepartmentAndSemesterAsync(int departmentId, int semester)
    {
        try
        {
            if (departmentId <= 0)
                return Result<IEnumerable<ClassBatchDto>>.Failure("Valid department ID is required");

            if (semester < 1 || semester > 8)
                return Result<IEnumerable<ClassBatchDto>>.Failure("Semester must be between 1 and 8");

            var classBatches = await _repository.GetByDepartmentAndSemesterAsync(departmentId, semester);
            var dtos = _mapper.Map<IEnumerable<ClassBatchDto>>(classBatches);
            return Result<IEnumerable<ClassBatchDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<ClassBatchDto>>.Failure($"Error retrieving class batches: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<ClassBatchDto>>> GetByRoomAsync(int roomId)
    {
        try
        {
            if (roomId <= 0)
                return Result<IEnumerable<ClassBatchDto>>.Failure("Valid room ID is required");

            var classBatches = await _repository.GetByRoomAsync(roomId);
            var dtos = _mapper.Map<IEnumerable<ClassBatchDto>>(classBatches);
            return Result<IEnumerable<ClassBatchDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<ClassBatchDto>>.Failure($"Error retrieving class batches: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<ClassBatchSummaryDto>>> GetSummaryListAsync()
    {
        try
        {
            var classBatches = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<ClassBatchSummaryDto>>(classBatches);
            return Result<IEnumerable<ClassBatchSummaryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<ClassBatchSummaryDto>>.Failure($"Error retrieving class batch list: {ex.Message}");
        }
    }

    public async Task<Result<int>> CreateAsync(CreateClassBatchRequest request)
    {
        try
        {
            if (request == null)
                return Result<int>.Failure("Invalid request");

            // Check for duplicate
            if (await _repository.BatchNameExistsAsync(request.BatchName, request.DepartmentId, request.AcademicYearId))
                return Result<int>.Failure($"Batch '{request.BatchName}' already exists in this department for this academic year");

            // Use domain business logic to create entity
            var createResult = DomainClassBatch.Create(
                request.BatchName, 
                request.Strength, 
                request.Semester, 
                request.DepartmentId, 
                request.AcademicYearId, 
                request.RoomId);

            if (!createResult.IsSuccess)
                return Result<int>.Failure(createResult.ErrorMessage ?? "Failed to create class batch");

            var classBatch = createResult.Value;

            // Persist
            await _repository.AddAsync(classBatch);
            await _repository.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync("CREATE", "ClassBatch", classBatch.Id.ToString(),
                null, $"Name: {request.BatchName}, Semester: {request.Semester}, Strength: {request.Strength}, DeptId: {request.DepartmentId}");

            return Result<int>.Success(classBatch.Id);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error creating class batch: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(UpdateClassBatchRequest request)
    {
        try
        {
            if (request == null || request.Id <= 0)
                return Result.Failure("Invalid request");

            var classBatch = await _repository.GetByIdAsync(request.Id);
            if (classBatch == null)
                return Result.Failure("Class batch not found");

            var oldValues = $"Name: {classBatch.BatchName}, Semester: {classBatch.Semester}, Strength: {classBatch.Strength}";

            // Use domain method to update
            var updateResult = classBatch.Update(request.BatchName, request.Strength, request.Semester, request.RoomId);

            if (!updateResult.IsSuccess)
                return updateResult;

            // Persist
            await _repository.UpdateAsync(classBatch);
            await _repository.SaveChangesAsync();

            // Audit log
            var newValues = $"Name: {request.BatchName}, Semester: {request.Semester}, Strength: {request.Strength}";
            await _auditService.LogAsync("UPDATE", "ClassBatch", request.Id.ToString(), oldValues, newValues);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error updating class batch: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var classBatch = await _repository.GetByIdAsync(id);
            if (classBatch == null)
                return Result.Failure("Class batch not found");

            await _repository.DeleteAsync(classBatch);
            await _repository.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync("DELETE", "ClassBatch", id.ToString(),
                $"Name: {classBatch.BatchName}, Semester: {classBatch.Semester}", null);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error deleting class batch: {ex.Message}");
        }
    }

    public async Task<Result> AssignRoomAsync(int classId, int roomId)
    {
        try
        {
            var classBatch = await _repository.GetByIdAsync(classId);
            if (classBatch == null)
                return Result.Failure("Class batch not found");

            var assignResult = classBatch.AssignRoom(roomId);
            if (!assignResult.IsSuccess)
                return assignResult;

            await _repository.UpdateAsync(classBatch);
            await _repository.SaveChangesAsync();

            await _auditService.LogAsync("UPDATE", "ClassBatch", classId.ToString(),
                $"Room: {classBatch.RoomId}", $"Room: {roomId}");

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error assigning room: {ex.Message}");
        }
    }

    public async Task<Result> RemoveRoomAsync(int classId)
    {
        try
        {
            var classBatch = await _repository.GetByIdAsync(classId);
            if (classBatch == null)
                return Result.Failure("Class batch not found");

            var oldRoom = classBatch.RoomId;
            var removeResult = classBatch.RemoveRoom();
            
            if (!removeResult.IsSuccess)
                return removeResult;

            await _repository.UpdateAsync(classBatch);
            await _repository.SaveChangesAsync();

            await _auditService.LogAsync("UPDATE", "ClassBatch", classId.ToString(),
                $"Room: {oldRoom}", "Room: null");

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error removing room assignment: {ex.Message}");
        }
    }
}
