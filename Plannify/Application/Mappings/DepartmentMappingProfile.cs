using AutoMapper;
using Plannify.Application.DTOs;
using DomainDepartment = Plannify.Domain.Entities.Department;

namespace Plannify.Application.Mappings;

/// <summary>
/// AutoMapper profile for Department entity to DTO mappings
/// </summary>
public class DepartmentMappingProfile : Profile
{
    public DepartmentMappingProfile()
    {
        // Domain Entity -> Response DTO
        CreateMap<DomainDepartment, DepartmentDto>()
            .ForMember(dest => dest.TeacherCount, opt => opt.Ignore())  // Set by service
            .ForMember(dest => dest.SubjectCount, opt => opt.Ignore())  // Set by service
            .ForMember(dest => dest.ClassCount, opt => opt.Ignore());   // Set by service

        // Domain Entity -> Summary DTO
        CreateMap<DomainDepartment, DepartmentSummaryDto>();
    }
}
