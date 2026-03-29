using AutoMapper;
using Plannify.Application.DTOs;
using Plannify.Domain.Entities;

namespace Plannify.Application.Mappings;

/// <summary>
/// AutoMapper profile for AcademicYear entity to DTO mappings
/// </summary>
public class AcademicYearMappingProfile : Profile
{
    public AcademicYearMappingProfile()
    {
        // Domain Entity -> Response DTO
        CreateMap<AcademicYear, AcademicYearDto>()
            .ForMember(dest => dest.IsCurrent, opt => opt.Ignore()); // Set by service

        // Domain Entity -> Summary DTO
        CreateMap<AcademicYear, AcademicYearSummaryDto>();

        // Request DTO -> Domain Entity (handled by business logic, not auto-mapping)
        // This ensures we use proper domain methods like AcademicYear.Create()
    }
}
