using AutoMapper;
using Plannify.Application.Common;
using Plannify.Application.Contracts;
using Plannify.Application.DTOs;
using Plannify.Domain.Entities;
using Plannify.Services;
using Plannify.Data;
using Plannify.Models;
using Microsoft.EntityFrameworkCore;
using domainTeacher = Plannify.Domain.Entities.Teacher;
using domainSemester = Plannify.Domain.Entities.Semester;

namespace Plannify.Application.Services;

/// <summary>
/// Teacher application service
/// Orchestrates all teacher-related business operations
/// </summary>
public class TeacherService : ITeacherService
{
    private readonly ITeacherRepository _repository;
    private readonly AuditService _auditService;
    private readonly IMapper _mapper;

    public TeacherService(
        ITeacherRepository repository,
        AuditService auditService,
        IMapper mapper)
    {
        _repository = repository;
        _auditService = auditService;
        _mapper = mapper;
    }

    public async Task<Result<TeacherDto>> GetByIdAsync(int id)
    {
        try
        {
            var teacher = await _repository.GetByIdAsync(id);
            if (teacher == null)
                return Result<TeacherDto>.Failure("Teacher not found");

            var dto = _mapper.Map<TeacherDto>(teacher);
            
            // Get current allocated hours
            var activeSemester = await GetActiveSemesterAsync();
            if (activeSemester != null)
            {
                dto.CurrentWeeklyHours = await _repository.GetAllocatedHoursAsync(id, activeSemester.Id);
                dto.CanAcceptMore = teacher.CanAcceptMoreHours(dto.CurrentWeeklyHours);
            }

            return Result<TeacherDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<TeacherDto>.Failure($"Error retrieving teacher: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<TeacherDto>>> GetAllAsync()
    {
        try
        {
            var teachers = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<TeacherDto>>(teachers);

            var activeSemester = await GetActiveSemesterAsync();
            if (activeSemester != null)
            {
                foreach (var dto in dtos)
                {
                    var teacher = teachers.FirstOrDefault(t => t.Id == dto.Id);
                    if (teacher != null)
                    {
                        dto.CurrentWeeklyHours = await _repository.GetAllocatedHoursAsync(dto.Id, activeSemester.Id);
                        dto.CanAcceptMore = teacher.CanAcceptMoreHours(dto.CurrentWeeklyHours);
                    }
                }
            }

            return Result<IEnumerable<TeacherDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<TeacherDto>>.Failure($"Error retrieving teachers: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<TeacherDto>>> GetActiveAsync()
    {
        try
        {
            var teachers = await _repository.GetActiveAsync();
            var dtos = _mapper.Map<IEnumerable<TeacherDto>>(teachers);

            var activeSemester = await GetActiveSemesterAsync();
            if (activeSemester != null)
            {
                foreach (var dto in dtos)
                {
                    dto.CurrentWeeklyHours = await _repository.GetAllocatedHoursAsync(dto.Id, activeSemester.Id);
                    var teacher = teachers.FirstOrDefault(t => t.Id == dto.Id);
                    if (teacher != null)
                        dto.CanAcceptMore = teacher.CanAcceptMoreHours(dto.CurrentWeeklyHours);
                }
            }

            return Result<IEnumerable<TeacherDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<TeacherDto>>.Failure($"Error retrieving active teachers: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<TeacherDto>>> GetByDepartmentAsync(int departmentId)
    {
        try
        {
            if (departmentId <= 0)
                return Result<IEnumerable<TeacherDto>>.Failure("Invalid department ID");

            var teachers = await _repository.GetByDepartmentAsync(departmentId);
            var dtos = _mapper.Map<IEnumerable<TeacherDto>>(teachers);

            return Result<IEnumerable<TeacherDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<TeacherDto>>.Failure($"Error retrieving department teachers: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<TeacherSummaryDto>>> GetSummaryListAsync()
    {
        try
        {
            var teachers = await _repository.GetActiveAsync();
            var dtos = _mapper.Map<IEnumerable<TeacherSummaryDto>>(teachers);
            return Result<IEnumerable<TeacherSummaryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<TeacherSummaryDto>>.Failure($"Error retrieving teacher list: {ex.Message}");
        }
    }

    public async Task<Result<int>> CreateTeacherAsync(CreateTeacherRequest request)
    {
        try
        {
            if (request == null)
                return Result<int>.Failure("Invalid request");

            // Check for duplicate employee code
            if (await _repository.EmployeeCodeExistsAsync(request.EmployeeCode))
                return Result<int>.Failure($"Teacher with employee code '{request.EmployeeCode}' already exists");

            // Use domain business logic to create entity
            var createResult = domainTeacher.Create(
                request.FullName,
                request.EmployeeCode,
                request.Email,
                request.DepartmentId,
                request.Initials);

            if (!createResult.IsSuccess)
                return Result<int>.Failure(createResult.ErrorMessage ?? "Failed to create teacher");

            var teacher = createResult.Value ?? throw new InvalidOperationException("Teacher creation returned null");
            teacher.Designation = request.Designation;
            teacher.MaxWeeklyHours = request.MaxWeeklyHours;

            // Persist
            await _repository.AddAsync(teacher);
            await _repository.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync("CREATE", "Teacher", teacher.Id.ToString(),
                null, $"Name: {request.FullName}, Code: {request.EmployeeCode}, Email: {request.Email}");

            return Result<int>.Success(teacher.Id);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error creating teacher: {ex.Message}");
        }
    }

    public async Task<Result> UpdateTeacherAsync(UpdateTeacherRequest request)
    {
        try
        {
            if (request == null || request.Id <= 0)
                return Result.Failure("Invalid request");

            var teacher = await _repository.GetByIdAsync(request.Id);
            if (teacher == null)
                return Result.Failure("Teacher not found");

            // Check for duplicate employee code (if changed)
            if (teacher.EmployeeCode != request.FullName &&
                await _repository.EmployeeCodeExistsAsync(request.FullName, request.Id))
                return Result.Failure("Employee code already exists");

            var oldValues = $"Name: {teacher.FullName}, Email: {teacher.Email}, Designation: {teacher.Designation}";

            // Use domain method to update
            var updateResult = teacher.Update(
                request.FullName,
                request.Email,
                request.Designation,
                request.MaxWeeklyHours,
                request.DepartmentId);

            if (!updateResult.IsSuccess)
                return updateResult;

            // Persist
            await _repository.UpdateAsync(teacher);
            await _repository.SaveChangesAsync();

            var newValues = $"Name: {teacher.FullName}, Email: {teacher.Email}, Designation: {teacher.Designation}";
            await _auditService.LogAsync("UPDATE", "Teacher", teacher.Id.ToString(), oldValues, newValues);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error updating teacher: {ex.Message}");
        }
    }

    public async Task<Result> DeleteTeacherAsync(int id)
    {
        try
        {
            if (id <= 0)
                return Result.Failure("Invalid teacher ID");

            var teacher = await _repository.GetWithTimetableSlotsAsync(id, 0); // Get with slots
            if (teacher == null)
                return Result.Failure("Teacher not found");

            // Business rule: Cannot delete if has timetable slots
            if (teacher.TimetableSlots.Any())
                return Result.Failure($"Cannot delete teacher with {teacher.TimetableSlots.Count} timetable slots assigned. " +
                    "Please remove all assignments first.");

            var teacherName = teacher.FullName;

            // Delete
            await _repository.DeleteAsync(id);
            await _repository.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync("DELETE", "Teacher", id.ToString(),
                $"Name: {teacherName}, Code: {teacher.EmployeeCode}", null);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error deleting teacher: {ex.Message}");
        }
    }

    public async Task<Result> DeactivateTeacherAsync(int id)
    {
        try
        {
            if (id <= 0)
                return Result.Failure("Invalid teacher ID");

            var teacher = await _repository.GetByIdAsync(id);
            if (teacher == null)
                return Result.Failure("Teacher not found");

            var result = teacher.Deactivate();
            if (!result.IsSuccess)
                return result;

            await _repository.UpdateAsync(teacher);
            await _repository.SaveChangesAsync();

            await _auditService.LogAsync("UPDATE", "Teacher", id.ToString(),
                $"Status: Active", $"Status: Inactive");

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error deactivating teacher: {ex.Message}");
        }
    }

    public async Task<Result<bool>> CanAcceptMoreHoursAsync(int teacherId, int semesterId, decimal proposedHours)
    {
        try
        {
            var teacher = await _repository.GetByIdAsync(teacherId);
            if (teacher == null)
                return Result<bool>.Failure("Teacher not found");

            var allocatedHours = await _repository.GetAllocatedHoursAsync(teacherId, semesterId);
            var totalHours = allocatedHours + proposedHours;

            return Result<bool>.Success(teacher.CanAcceptMoreHours(totalHours));
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error checking hours capacity: {ex.Message}");
        }
    }

    public async Task<Result<decimal>> GetAllocatedHoursAsync(int teacherId, int semesterId)
    {
        try
        {
            var hours = await _repository.GetAllocatedHoursAsync(teacherId, semesterId);
            return Result<decimal>.Success(hours);
        }
        catch (Exception ex)
        {
            return Result<decimal>.Failure($"Error retrieving allocated hours: {ex.Message}");
        }
    }

    // Helper method
    private Task<domainSemester?> GetActiveSemesterAsync()
    {
        // TODO: Inject a SemesterRepository or similar
        // For now, return null - this will be refactored
        return Task.FromResult<domainSemester?>(null);
    }
}

