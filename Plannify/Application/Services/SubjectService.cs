using AutoMapper;
using Plannify.Application.Common;
using Plannify.Application.Contracts;
using Plannify.Application.DTOs;
using DomainSubject = Plannify.Domain.Entities.Subject;
using Plannify.Services;

namespace Plannify.Application.Services;

/// <summary>
/// Subject service - following same pattern as TeacherService, DepartmentService, and RoomService
/// All subject business logic orchestrated here
/// </summary>
public class SubjectService : ISubjectService
{
    private readonly ISubjectRepository _repository;
    private readonly AuditService _auditService;
    private readonly IMapper _mapper;

    public SubjectService(
        ISubjectRepository repository,
        AuditService auditService,
        IMapper mapper)
    {
        _repository = repository;
        _auditService = auditService;
        _mapper = mapper;
    }

    public async Task<Result<SubjectDto>> GetByIdAsync(int id)
    {
        try
        {
            var subject = await _repository.GetByIdAsync(id);
            if (subject == null)
                return Result<SubjectDto>.Failure("Subject not found");

            var dto = _mapper.Map<SubjectDto>(subject);
            return Result<SubjectDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<SubjectDto>.Failure($"Error retrieving subject: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<SubjectDto>>> GetAllAsync()
    {
        try
        {
            var subjects = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<SubjectDto>>(subjects);
            return Result<IEnumerable<SubjectDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<SubjectDto>>.Failure($"Error retrieving subjects: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<SubjectDto>>> GetByDepartmentAsync(int departmentId)
    {
        try
        {
            var subjects = await _repository.GetByDepartmentAsync(departmentId);
            var dtos = _mapper.Map<IEnumerable<SubjectDto>>(subjects);
            return Result<IEnumerable<SubjectDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<SubjectDto>>.Failure($"Error retrieving department subjects: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<SubjectDto>>> GetBySemesterAsync(int semesterNumber)
    {
        try
        {
            if (semesterNumber < 1 || semesterNumber > 8)
                return Result<IEnumerable<SubjectDto>>.Failure("Invalid semester number");

            var subjects = await _repository.GetBySemesterAsync(semesterNumber);
            var dtos = _mapper.Map<IEnumerable<SubjectDto>>(subjects);
            return Result<IEnumerable<SubjectDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<SubjectDto>>.Failure($"Error retrieving semester subjects: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<SubjectSummaryDto>>> GetSummaryListAsync()
    {
        try
        {
            var subjects = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<SubjectSummaryDto>>(subjects);
            return Result<IEnumerable<SubjectSummaryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<SubjectSummaryDto>>.Failure($"Error retrieving subject list: {ex.Message}");
        }
    }

    public async Task<Result<int>> CreateAsync(CreateSubjectRequest request)
    {
        try
        {
            if (request == null)
                return Result<int>.Failure("Invalid request");

            // Check for duplicate code in department
            if (await _repository.CodeExistsInDepartmentAsync(request.Code, request.DepartmentId))
                return Result<int>.Failure($"Subject code '{request.Code}' already exists in this department");

            // Use domain business logic to create entity
            var createResult = DomainSubject.Create(request.Name, request.Code, request.DepartmentId, 
                request.SemesterNumber, request.Credits, request.MaxClassesPerWeek);

            if (!createResult.IsSuccess)
                return Result<int>.Failure(createResult.ErrorMessage ?? "Failed to create subject");

            var subject = createResult.Value ?? throw new InvalidOperationException("Subject creation returned null");

            // Persist
            await _repository.AddAsync(subject);
            await _repository.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync("CREATE", "Subject", subject.Id.ToString(),
                null, $"Name: {request.Name}, Code: {request.Code}, Dept: {request.DepartmentId}, Sem: {request.SemesterNumber}");

            return Result<int>.Success(subject.Id);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error creating subject: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(UpdateSubjectRequest request)
    {
        try
        {
            if (request == null || request.Id <= 0)
                return Result.Failure("Invalid request");

            var subject = await _repository.GetByIdAsync(request.Id);
            if (subject == null)
                return Result.Failure("Subject not found");

            // Check for duplicate code (if changed)
            if (subject.Code != request.Code &&
                await _repository.CodeExistsInDepartmentAsync(request.Code, request.DepartmentId, request.Id))
                return Result.Failure($"Subject code '{request.Code}' already exists in this department");

            var oldValues = $"Name: {subject.Name}, Code: {subject.Code}";

            // Use domain method to update
            var updateResult = subject.Update(request.Name, request.Code, request.DepartmentId, 
                request.SemesterNumber, request.Credits, request.MaxClassesPerWeek);

            if (!updateResult.IsSuccess)
                return updateResult;

            // Persist
            await _repository.UpdateAsync(subject);
            await _repository.SaveChangesAsync();

            // Audit log
            var newValues = $"Name: {request.Name}, Code: {request.Code}";
            await _auditService.LogAsync("UPDATE", "Subject", request.Id.ToString(), oldValues, newValues);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error updating subject: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var subject = await _repository.GetByIdAsync(id);
            if (subject == null)
                return Result.Failure("Subject not found");

            await _repository.DeleteAsync(subject);
            await _repository.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync("DELETE", "Subject", id.ToString(),
                $"Name: {subject.Name}, Code: {subject.Code}", null);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error deleting subject: {ex.Message}");
        }
    }
}
