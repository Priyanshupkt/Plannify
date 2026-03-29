using AutoMapper;
using Plannify.Application.Common;
using Plannify.Application.Contracts;
using Plannify.Application.DTOs;
using DomainAcademicYear = Plannify.Domain.Entities.AcademicYear;
using Plannify.Services;

namespace Plannify.Application.Services;

/// <summary>
/// AcademicYear service - following clean architecture pattern
/// All academic year business logic orchestrated here
/// </summary>
public class AcademicYearService : IAcademicYearService
{
    private readonly IAcademicYearRepository _repository;
    private readonly AuditService _auditService;
    private readonly IMapper _mapper;

    public AcademicYearService(
        IAcademicYearRepository repository,
        AuditService auditService,
        IMapper mapper)
    {
        _repository = repository;
        _auditService = auditService;
        _mapper = mapper;
    }

    public async Task<Result<AcademicYearDto>> GetByIdAsync(int id)
    {
        try
        {
            var academicYear = await _repository.GetByIdAsync(id);
            if (academicYear == null)
                return Result<AcademicYearDto>.Failure("Academic year not found");

            var dto = _mapper.Map<AcademicYearDto>(academicYear);
            dto.IsCurrent = academicYear.IsCurrent();
            return Result<AcademicYearDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<AcademicYearDto>.Failure($"Error retrieving academic year: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<AcademicYearDto>>> GetAllAsync()
    {
        try
        {
            var academicYears = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<AcademicYearDto>>(academicYears);
            
            // Set IsCurrent for each
            foreach (var dto in dtos)
            {
                var ay = academicYears.FirstOrDefault(a => a.Id == dto.Id);
                if (ay != null)
                    dto.IsCurrent = ay.IsCurrent();
            }
            
            return Result<IEnumerable<AcademicYearDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<AcademicYearDto>>.Failure($"Error retrieving academic years: {ex.Message}");
        }
    }

    public async Task<Result<AcademicYearDto>> GetCurrentAsync()
    {
        try
        {
            var academicYear = await _repository.GetCurrentAcademicYearAsync();
            if (academicYear == null)
                return Result<AcademicYearDto>.Failure("No current academic year found");

            var dto = _mapper.Map<AcademicYearDto>(academicYear);
            dto.IsCurrent = true;
            return Result<AcademicYearDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<AcademicYearDto>.Failure($"Error retrieving current academic year: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<AcademicYearDto>>> GetAllActiveAsync()
    {
        try
        {
            var academicYears = await _repository.GetAllActiveAsync();
            var dtos = _mapper.Map<IEnumerable<AcademicYearDto>>(academicYears);
            return Result<IEnumerable<AcademicYearDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<AcademicYearDto>>.Failure($"Error retrieving active academic years: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<AcademicYearSummaryDto>>> GetSummaryListAsync()
    {
        try
        {
            var academicYears = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<AcademicYearSummaryDto>>(academicYears);
            return Result<IEnumerable<AcademicYearSummaryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<AcademicYearSummaryDto>>.Failure($"Error retrieving academic year list: {ex.Message}");
        }
    }

    public async Task<Result<int>> CreateAsync(CreateAcademicYearRequest request)
    {
        try
        {
            if (request == null)
                return Result<int>.Failure("Invalid request");

            // Check for duplicate
            if (await _repository.YearLabelExistsAsync(request.YearLabel))
                return Result<int>.Failure($"Academic year '{request.YearLabel}' already exists");

            // Use domain business logic to create entity
            var createResult = DomainAcademicYear.Create(request.YearLabel, request.StartDate, request.EndDate);

            if (!createResult.IsSuccess)
                return Result<int>.Failure(createResult.ErrorMessage ?? "Failed to create academic year");

            var academicYear = createResult.Value ?? throw new InvalidOperationException("Academic year creation returned null");

            // Persist
            await _repository.AddAsync(academicYear);
            await _repository.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync("CREATE", "AcademicYear", academicYear.Id.ToString(),
                null, $"Label: {request.YearLabel}, Start: {request.StartDate:yyyy-MM-dd}, End: {request.EndDate:yyyy-MM-dd}");

            return Result<int>.Success(academicYear.Id);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error creating academic year: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(UpdateAcademicYearRequest request)
    {
        try
        {
            if (request == null || request.Id <= 0)
                return Result.Failure("Invalid request");

            var academicYear = await _repository.GetByIdAsync(request.Id);
            if (academicYear == null)
                return Result.Failure("Academic year not found");

            var oldValues = $"Label: {academicYear.YearLabel}, Start: {academicYear.StartDate:yyyy-MM-dd}, End: {academicYear.EndDate:yyyy-MM-dd}";

            // Check if label changed and if new label exists
            if (academicYear.YearLabel != request.YearLabel && await _repository.YearLabelExistsAsync(request.YearLabel, request.Id))
                return Result.Failure($"Academic year '{request.YearLabel}' already exists");

            // Use domain method to update
            var updateResult = academicYear.Update(request.YearLabel, request.StartDate, request.EndDate);

            if (!updateResult.IsSuccess)
                return updateResult;

            // Persist
            await _repository.UpdateAsync(academicYear);
            await _repository.SaveChangesAsync();

            // Audit log
            var newValues = $"Label: {request.YearLabel}, Start: {request.StartDate:yyyy-MM-dd}, End: {request.EndDate:yyyy-MM-dd}";
            await _auditService.LogAsync("UPDATE", "AcademicYear", request.Id.ToString(), oldValues, newValues);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error updating academic year: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var academicYear = await _repository.GetByIdAsync(id);
            if (academicYear == null)
                return Result.Failure("Academic year not found");

            await _repository.DeleteAsync(academicYear);
            await _repository.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync("DELETE", "AcademicYear", id.ToString(),
                $"Label: {academicYear.YearLabel}", null);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error deleting academic year: {ex.Message}");
        }
    }

    public async Task<Result> ActivateAsync(int id)
    {
        try
        {
            var academicYear = await _repository.GetByIdAsync(id);
            if (academicYear == null)
                return Result.Failure("Academic year not found");

            var activateResult = academicYear.Activate();
            if (!activateResult.IsSuccess)
                return activateResult;

            await _repository.UpdateAsync(academicYear);
            await _repository.SaveChangesAsync();

            await _auditService.LogAsync("UPDATE", "AcademicYear", id.ToString(),
                "IsActive: false", "IsActive: true");

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error activating academic year: {ex.Message}");
        }
    }

    public async Task<Result> DeactivateAsync(int id)
    {
        try
        {
            var academicYear = await _repository.GetByIdAsync(id);
            if (academicYear == null)
                return Result.Failure("Academic year not found");

            var deactivateResult = academicYear.Deactivate();
            if (!deactivateResult.IsSuccess)
                return deactivateResult;

            await _repository.UpdateAsync(academicYear);
            await _repository.SaveChangesAsync();

            await _auditService.LogAsync("UPDATE", "AcademicYear", id.ToString(),
                "IsActive: true", "IsActive: false");

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error deactivating academic year: {ex.Message}");
        }
    }
}
