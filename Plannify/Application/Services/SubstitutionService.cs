using AutoMapper;
using Plannify.Application.Common;
using Plannify.Application.Contracts;
using Plannify.Application.DTOs;
using Plannify.Domain.Entities;
using Plannify.Services;

namespace Plannify.Application.Services;

/// <summary>
/// Service implementation for substitution business logic
/// </summary>
public class SubstitutionService : ISubstitutionService
{
    private readonly ISubstitutionRepository _repository;
    private readonly AuditService _auditService;
    private readonly IMapper _mapper;

    public SubstitutionService(
        ISubstitutionRepository repository,
        AuditService auditService,
        IMapper mapper)
    {
        _repository = repository;
        _auditService = auditService;
        _mapper = mapper;
    }

    public async Task<Result<SubstitutionDto>> GetByIdAsync(int id)
    {
        if (id <= 0)
            return Result<SubstitutionDto>.Failure("Substitution ID must be positive");

        var substitution = await _repository.GetByIdAsync(id);
        if (substitution == null)
            return Result<SubstitutionDto>.Failure("Substitution not found");

        var dto = _mapper.Map<SubstitutionDto>(substitution);
        return Result<SubstitutionDto>.Success(dto);
    }

    public async Task<Result<List<SubstitutionDto>>> GetAllAsync()
    {
        var substitutions = await _repository.GetAllAsync();
        var dtos = _mapper.Map<List<SubstitutionDto>>(substitutions);
        return Result<List<SubstitutionDto>>.Success(dtos);
    }

    public async Task<Result<List<SubstitutionSummaryDto>>> GetByTeacherAsync(int teacherId)
    {
        if (teacherId <= 0)
            return Result<List<SubstitutionSummaryDto>>.Failure("Teacher ID must be positive");

        var substitutions = await _repository.GetByTeacherAsync(teacherId);
        var dtos = _mapper.Map<List<SubstitutionSummaryDto>>(substitutions);
        return Result<List<SubstitutionSummaryDto>>.Success(dtos);
    }

    public async Task<Result<List<SubstitutionSummaryDto>>> GetByDateAsync(DateOnly date)
    {
        var substitutions = await _repository.GetByDateAsync(date);
        var dtos = _mapper.Map<List<SubstitutionSummaryDto>>(substitutions);
        return Result<List<SubstitutionSummaryDto>>.Success(dtos);
    }

    public async Task<Result<List<SubstitutionSummaryDto>>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        if (endDate < startDate)
            return Result<List<SubstitutionSummaryDto>>.Failure("End date must be after start date");

        var substitutions = await _repository.GetByDateRangeAsync(startDate, endDate);
        var dtos = _mapper.Map<List<SubstitutionSummaryDto>>(substitutions);
        return Result<List<SubstitutionSummaryDto>>.Success(dtos);
    }

    public async Task<Result<SubstitutionDto>> CreateAsync(CreateSubstitutionRequest request)
    {
        // Pre-operation validation
        if (request == null)
            return Result<SubstitutionDto>.Failure("Request cannot be null");

        // Use domain entity factory method for business logic validation
        var (success, error) = Substitution.Create(
            request.TimetableSlotId,
            request.OriginalTeacherId,
            request.SubstituteTeacherId,
            request.Date,
            request.Reason,
            request.ApprovedBy);

        if (!success)
            return Result<SubstitutionDto>.Failure(error ?? "Failed to create substitution");

        // Check for duplicate
        var exists = await _repository.ExistsBySlotAndDateAsync(request.TimetableSlotId, request.Date);
        if (exists)
            return Result<SubstitutionDto>.Failure("Substitution already exists for this slot on this date");

        // Create new substitution
        var substitution = new Substitution();
        // Re-create with factory since creation succeeded
        Substitution.Create(
            request.TimetableSlotId,
            request.OriginalTeacherId,
            request.SubstituteTeacherId,
            request.Date,
            request.Reason,
            request.ApprovedBy);

        // Persist
        var id = await _repository.AddAsync(substitution);

        // Audit log
        await _auditService.LogAsync(
            "CREATE",
            "Substitution",
            id.ToString(),
            null,
            $"Created substitution for slot {request.TimetableSlotId}");

        // Return DTO
        var dto = _mapper.Map<SubstitutionDto>(substitution);
        return Result<SubstitutionDto>.Success(dto);
    }

    public async Task<Result<SubstitutionDto>> UpdateAsync(int id, UpdateSubstitutionRequest request)
    {
        // Pre-operation validation
        if (id <= 0)
            return Result<SubstitutionDto>.Failure("Substitution ID must be positive");

        if (request == null)
            return Result<SubstitutionDto>.Failure("Request cannot be null");

        // Fetch existing
        var substitution = await _repository.GetByIdAsync(id);
        if (substitution == null)
            return Result<SubstitutionDto>.Failure("Substitution not found");

        // Update using domain entity method
        var (success, error) = substitution.Update(request.Reason, request.ApprovedBy);
        if (!success)
            return Result<SubstitutionDto>.Failure(error ?? "Failed to update substitution");

        // Persist
        await _repository.UpdateAsync(substitution);

        // Audit log
        await _auditService.LogAsync(
            "UPDATE",
            "Substitution",
            id.ToString(),
            null,
            "Updated substitution details");

        // Return DTO
        var dto = _mapper.Map<SubstitutionDto>(substitution);
        return Result<SubstitutionDto>.Success(dto);
    }

    public async Task<Result<SubstitutionDto>> ChangeSubstituteAsync(int id, ChangeSubstituteRequest request)
    {
        // Pre-operation validation
        if (id <= 0)
            return Result<SubstitutionDto>.Failure("Substitution ID must be positive");

        if (request == null)
            return Result<SubstitutionDto>.Failure("Request cannot be null");

        // Fetch existing
        var substitution = await _repository.GetByIdAsync(id);
        if (substitution == null)
            return Result<SubstitutionDto>.Failure("Substitution not found");

        // Change substitute using domain entity method
        var (success, error) = substitution.ChangeSubstitute(request.NewSubstituteTeacherId);
        if (!success)
            return Result<SubstitutionDto>.Failure(error ?? "Failed to change substitute");

        // Persist
        await _repository.UpdateAsync(substitution);

        // Audit log
        await _auditService.LogAsync(
            "UPDATE",
            "Substitution",
            id.ToString(),
            null,
            $"Changed substitute teacher to {request.NewSubstituteTeacherId}");

        // Return DTO
        var dto = _mapper.Map<SubstitutionDto>(substitution);
        return Result<SubstitutionDto>.Success(dto);
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        // Pre-operation validation
        if (id <= 0)
            return Result<bool>.Failure("Substitution ID must be positive");

        // Check existence
        var substitution = await _repository.GetByIdAsync(id);
        if (substitution == null)
            return Result<bool>.Failure("Substitution not found");

        // Delete
        await _repository.DeleteAsync(id);

        // Audit log
        await _auditService.LogAsync(
            "DELETE",
            "Substitution",
            id.ToString(),
            null,
            "Deleted substitution");

        return Result<bool>.Success(true);
    }

    public async Task<Result<List<SubstitutionSummaryDto>>> GetActiveAsync()
    {
        var substitutions = await _repository.GetActiveAsync();
        var dtos = _mapper.Map<List<SubstitutionSummaryDto>>(substitutions);
        return Result<List<SubstitutionSummaryDto>>.Success(dtos);
    }

    public async Task<Result<List<SubstitutionSummaryDto>>> GetUrgentAsync()
    {
        var substitutions = await _repository.GetUrgentAsync();
        var dtos = _mapper.Map<List<SubstitutionSummaryDto>>(substitutions);
        return Result<List<SubstitutionSummaryDto>>.Success(dtos);
    }

    public async Task<Result<bool>> ExistsBySlotAndDateAsync(int slotId, DateOnly date)
    {
        if (slotId <= 0)
            return Result<bool>.Failure("Slot ID must be positive");

        var exists = await _repository.ExistsBySlotAndDateAsync(slotId, date);
        return Result<bool>.Success(exists);
    }

    public async Task<Result<bool>> TeacherHasSubstitutionOnDateAsync(int teacherId, DateOnly date)
    {
        if (teacherId <= 0)
            return Result<bool>.Failure("Teacher ID must be positive");

        var exists = await _repository.TeacherHasSubstitutionOnDateAsync(teacherId, date);
        return Result<bool>.Success(exists);
    }

    public async Task<Result<List<SubstitutionSummaryDto>>> GetByTimetableSlotAsync(int slotId)
    {
        if (slotId <= 0)
            return Result<List<SubstitutionSummaryDto>>.Failure("Slot ID must be positive");

        var substitutions = await _repository.GetByTimetableSlotAsync(slotId);
        var dtos = _mapper.Map<List<SubstitutionSummaryDto>>(substitutions);
        return Result<List<SubstitutionSummaryDto>>.Success(dtos);
    }

    public async Task<Result<int>> GetCountAsync()
    {
        var count = await _repository.GetCountAsync();
        return Result<int>.Success(count);
    }

    public async Task<Result<int>> GetCountByTeacherAsync(int teacherId)
    {
        if (teacherId <= 0)
            return Result<int>.Failure("Teacher ID must be positive");

        var count = await _repository.GetCountByTeacherAsync(teacherId);
        return Result<int>.Success(count);
    }

    public async Task<Result<List<SubstitutionSummaryDto>>> GetByOriginalTeacherAsync(int teacherId)
    {
        if (teacherId <= 0)
            return Result<List<SubstitutionSummaryDto>>.Failure("Teacher ID must be positive");

        var substitutions = await _repository.GetByOriginalTeacherAsync(teacherId);
        var dtos = _mapper.Map<List<SubstitutionSummaryDto>>(substitutions);
        return Result<List<SubstitutionSummaryDto>>.Success(dtos);
    }

    public async Task<Result<List<SubstitutionSummaryDto>>> GetBySubstituteTeacherAsync(int teacherId)
    {
        if (teacherId <= 0)
            return Result<List<SubstitutionSummaryDto>>.Failure("Teacher ID must be positive");

        var substitutions = await _repository.GetBySubstituteTeacherAsync(teacherId);
        var dtos = _mapper.Map<List<SubstitutionSummaryDto>>(substitutions);
        return Result<List<SubstitutionSummaryDto>>.Success(dtos);
    }
}
