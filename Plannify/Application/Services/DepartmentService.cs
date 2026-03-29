using AutoMapper;
using Plannify.Application.Common;
using Plannify.Application.Contracts;
using Plannify.Application.DTOs;
using DomainDepartment = Plannify.Domain.Entities.Department;
using Plannify.Services;

namespace Plannify.Application.Services;

/// <summary>
/// Department service - following same pattern as TeacherService
/// All department business logic orchestrated here
/// </summary>
public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repository;
    private readonly AuditService _auditService;
    private readonly IMapper _mapper;

    public DepartmentService(
        IDepartmentRepository repository,
        AuditService auditService,
        IMapper mapper)
    {
        _repository = repository;
        _auditService = auditService;
        _mapper = mapper;
    }

    public async Task<Result<DepartmentDto>> GetByIdAsync(int id)
    {
        try
        {
            var department = await _repository.GetByIdAsync(id);
            if (department == null)
                return Result<DepartmentDto>.Failure("Department not found");

            var dto = _mapper.Map<DepartmentDto>(department);

            // Get related counts
            dto.TeacherCount = await _repository.GetTeacherCountAsync(id);
            dto.SubjectCount = await _repository.GetSubjectCountAsync(id);
            dto.ClassCount = await _repository.GetClassCountAsync(id);

            return Result<DepartmentDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<DepartmentDto>.Failure($"Error retrieving department: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<DepartmentDto>>> GetAllAsync()
    {
        try
        {
            var departments = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<DepartmentDto>>(departments);

            // Get counts for each department
            foreach (var dto in dtos)
            {
                dto.TeacherCount = await _repository.GetTeacherCountAsync(dto.Id);
                dto.SubjectCount = await _repository.GetSubjectCountAsync(dto.Id);
                dto.ClassCount = await _repository.GetClassCountAsync(dto.Id);
            }

            return Result<IEnumerable<DepartmentDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<DepartmentDto>>.Failure($"Error retrieving departments: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<DepartmentSummaryDto>>> GetSummaryListAsync()
    {
        try
        {
            var departments = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<DepartmentSummaryDto>>(departments);
            return Result<IEnumerable<DepartmentSummaryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<DepartmentSummaryDto>>.Failure($"Error retrieving department list: {ex.Message}");
        }
    }

    public async Task<Result<int>> CreateAsync(CreateDepartmentRequest request)
    {
        try
        {
            if (request == null)
                return Result<int>.Failure("Invalid request");

            // Check for duplicate code
            if (await _repository.CodeExistsAsync(request.Code))
                return Result<int>.Failure($"Department with code '{request.Code}' already exists");

            // Use domain business logic to create entity
            var createResult = DomainDepartment.Create(request.Name, request.Code, request.ShortName);

            if (!createResult.IsSuccess)
                return Result<int>.Failure(createResult.ErrorMessage ?? "Failed to create department");

            var department = createResult.Value;

            // Persist
            await _repository.AddAsync(department);
            await _repository.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync("CREATE", "Department", department.Id.ToString(),
                null, $"Name: {request.Name}, Code: {request.Code}");

            return Result<int>.Success(department.Id);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error creating department: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(UpdateDepartmentRequest request)
    {
        try
        {
            if (request == null || request.Id <= 0)
                return Result.Failure("Invalid request");

            var department = await _repository.GetByIdAsync(request.Id);
            if (department == null)
                return Result.Failure("Department not found");

            // Check for duplicate code (if changed)
            if (department.Code != request.Code &&
                await _repository.CodeExistsAsync(request.Code, request.Id))
                return Result.Failure($"Code '{request.Code}' already exists");

            var oldValues = $"Name: {department.Name}, Code: {department.Code}";

            // Use domain method to update
            var updateResult = department.Update(request.Name, request.Code, request.ShortName);

            if (!updateResult.IsSuccess)
                return updateResult;

            // Persist
            await _repository.UpdateAsync(department);
            await _repository.SaveChangesAsync();

            var newValues = $"Name: {department.Name}, Code: {department.Code}";
            await _auditService.LogAsync("UPDATE", "Department", department.Id.ToString(), oldValues, newValues);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error updating department: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            if (id <= 0)
                return Result.Failure("Invalid department ID");

            var department = await _repository.GetByIdAsync(id);
            if (department == null)
                return Result.Failure("Department not found");

            // Check dependencies
            var teacherCount = await _repository.GetTeacherCountAsync(id);
            var subjectCount = await _repository.GetSubjectCountAsync(id);
            var classCount = await _repository.GetClassCountAsync(id);

            if (teacherCount > 0 || subjectCount > 0 || classCount > 0)
                return Result.Failure(
                    $"Cannot delete department. It has {teacherCount} teachers, {subjectCount} subjects, and {classCount} classes.");

            var deptName = department.Name;

            // Delete
            await _repository.DeleteAsync(id);
            await _repository.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync("DELETE", "Department", id.ToString(),
                $"Name: {deptName}, Code: {department.Code}", null);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error deleting department: {ex.Message}");
        }
    }

    public async Task<Result<bool>> CanDeleteAsync(int id)
    {
        try
        {
            var teacherCount = await _repository.GetTeacherCountAsync(id);
            var subjectCount = await _repository.GetSubjectCountAsync(id);
            var classCount = await _repository.GetClassCountAsync(id);

            var canDelete = teacherCount == 0 && subjectCount == 0 && classCount == 0;
            return Result<bool>.Success(canDelete);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error checking delete eligibility: {ex.Message}");
        }
    }
}
