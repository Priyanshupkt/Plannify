using AutoMapper;
using Plannify.Application.Common;
using Plannify.Application.Contracts;
using Plannify.Application.DTOs;
using DomainSemester = Plannify.Domain.Entities.Semester;
using Plannify.Services;

namespace Plannify.Application.Services;

/// <summary>
/// Semester service - following clean architecture pattern
/// All semester business logic orchestrated here
/// </summary>
public class SemesterService : ISemesterService
{
    private readonly ISemesterRepository _repository;
    private readonly AuditService _auditService;
    private readonly IMapper _mapper;

    public SemesterService(
        ISemesterRepository repository,
        AuditService auditService,
        IMapper mapper)
    {
        _repository = repository;
        _auditService = auditService;
        _mapper = mapper;
    }

    public async Task<Result<SemesterDto>> GetByIdAsync(int id)
    {
        try
        {
            var semester = await _repository.GetByIdAsync(id);
            if (semester == null)
                return Result<SemesterDto>.Failure("Semester not found");

            var dto = _mapper.Map<SemesterDto>(semester);
            dto.IsCurrent = semester.IsCurrent();
            return Result<SemesterDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<SemesterDto>.Failure($"Error retrieving semester: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<SemesterDto>>> GetAllAsync()
    {
        try
        {
            var semesters = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<SemesterDto>>(semesters);
            
            // Set IsCurrent for each
            foreach (var dto in dtos)
            {
                var sem = semesters.FirstOrDefault(s => s.Id == dto.Id);
                if (sem != null)
                    dto.IsCurrent = sem.IsCurrent();
            }
            
            return Result<IEnumerable<SemesterDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<SemesterDto>>.Failure($"Error retrieving semesters: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<SemesterDto>>> GetByAcademicYearAsync(int academicYearId)
    {
        try
        {
            if (academicYearId <= 0)
                return Result<IEnumerable<SemesterDto>>.Failure("Valid academic year ID is required");

            var semesters = await _repository.GetByAcademicYearAsync(academicYearId);
            var dtos = _mapper.Map<IEnumerable<SemesterDto>>(semesters);
            return Result<IEnumerable<SemesterDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<SemesterDto>>.Failure($"Error retrieving semesters: {ex.Message}");
        }
    }

    public async Task<Result<SemesterDto>> GetCurrentSemesterAsync()
    {
        try
        {
            var semester = await _repository.GetCurrentSemesterAsync();
            if (semester == null)
                return Result<SemesterDto>.Failure("No current semester found");

            var dto = _mapper.Map<SemesterDto>(semester);
            dto.IsCurrent = true;
            return Result<SemesterDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<SemesterDto>.Failure($"Error retrieving current semester: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<SemesterSummaryDto>>> GetSummaryListAsync()
    {
        try
        {
            var semesters = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<SemesterSummaryDto>>(semesters);
            return Result<IEnumerable<SemesterSummaryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<SemesterSummaryDto>>.Failure($"Error retrieving semester list: {ex.Message}");
        }
    }

    public async Task<Result<int>> CreateAsync(CreateSemesterRequest request)
    {
        try
        {
            if (request == null)
                return Result<int>.Failure("Invalid request");

            // Check for duplicate
            if (await _repository.ExistsForYearAsync(request.SemesterNumber, request.AcademicYearId))
                return Result<int>.Failure($"Semester {request.SemesterNumber} already exists for this academic year");

            // Use domain business logic to create entity
            var createResult = DomainSemester.Create(request.Name, request.SemesterNumber, request.AcademicYearId, 
                request.StartDate, request.EndDate);

            if (!createResult.IsSuccess)
                return Result<int>.Failure(createResult.ErrorMessage ?? "Failed to create semester");

            var semester = createResult.Value ?? throw new InvalidOperationException("Semester creation returned null");

            // Persist
            await _repository.AddAsync(semester);
            await _repository.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync("CREATE", "Semester", semester.Id.ToString(),
                null, $"Name: {request.Name}, Number: {request.SemesterNumber}, Year: {request.AcademicYearId}");

            return Result<int>.Success(semester.Id);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error creating semester: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(UpdateSemesterRequest request)
    {
        try
        {
            if (request == null || request.Id <= 0)
                return Result.Failure("Invalid request");

            var semester = await _repository.GetByIdAsync(request.Id);
            if (semester == null)
                return Result.Failure("Semester not found");

            var oldValues = $"Name: {semester.Name}, Number: {semester.SemesterNumber}";

            // Use domain method to update
            var updateResult = semester.Update(request.Name, request.SemesterNumber, 
                request.StartDate, request.EndDate);

            if (!updateResult.IsSuccess)
                return updateResult;

            // Persist
            await _repository.UpdateAsync(semester);
            await _repository.SaveChangesAsync();

            // Audit log
            var newValues = $"Name: {request.Name}, Number: {request.SemesterNumber}";
            await _auditService.LogAsync("UPDATE", "Semester", request.Id.ToString(), oldValues, newValues);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error updating semester: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var semester = await _repository.GetByIdAsync(id);
            if (semester == null)
                return Result.Failure("Semester not found");

            await _repository.DeleteAsync(semester);
            await _repository.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync("DELETE", "Semester", id.ToString(),
                $"Name: {semester.Name}, Number: {semester.SemesterNumber}", null);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error deleting semester: {ex.Message}");
        }
    }
}
