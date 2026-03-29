using AutoMapper;
using Plannify.Application.DTOs;
using DomainSubject = Plannify.Domain.Entities.Subject;

namespace Plannify.Application.Mappings;

/// <summary>
/// AutoMapper profile for Subject entity to DTO mappings
/// </summary>
public class SubjectMappingProfile : Profile
{
    public SubjectMappingProfile()
    {
        // Domain Entity -> Response DTO
        CreateMap<DomainSubject, SubjectDto>()
            .ForMember(dest => dest.DepartmentName, opt => opt.Ignore())  // Set by service when needed
            .ForMember(dest => dest.AllocatedClassesPerWeek, opt => opt.Ignore());  // Set by service

        // Domain Entity -> Summary DTO
        CreateMap<DomainSubject, SubjectSummaryDto>();
    }
}
