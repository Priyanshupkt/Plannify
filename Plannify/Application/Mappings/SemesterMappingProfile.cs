using AutoMapper;
using Plannify.Application.DTOs;
using Plannify.Domain.Entities;

namespace Plannify.Application.Mappings;

/// <summary>
/// AutoMapper profile for Semester entity to DTO mappings
/// </summary>
public class SemesterMappingProfile : Profile
{
    public SemesterMappingProfile()
    {
        // Domain Entity -> Response DTO
        CreateMap<Semester, SemesterDto>()
            .ForMember(dest => dest.IsCurrent, opt => opt.Ignore()); // Set by service

        // Domain Entity -> Summary DTO
        CreateMap<Semester, SemesterSummaryDto>();

        // Request DTO -> Domain Entity (handled by business logic, not auto-mapping)
        // This ensures we use proper domain methods like Semester.Create()
    }
}
