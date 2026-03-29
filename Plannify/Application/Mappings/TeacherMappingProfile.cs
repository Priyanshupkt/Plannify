using AutoMapper;
using Plannify.Application.DTOs;
using Plannify.Domain.Entities;

namespace Plannify.Application.Mappings;

/// <summary>
/// AutoMapper profile for Teacher entity to DTO mappings
/// </summary>
public class TeacherMappingProfile : Profile
{
    public TeacherMappingProfile()
    {
        // Domain Entity -> Response DTO
        CreateMap<Teacher, TeacherDto>()
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department!.Name))
            .ForMember(dest => dest.CurrentWeeklyHours, opt => opt.Ignore()) // Set by service
            .ForMember(dest => dest.CanAcceptMore, opt => opt.Ignore()); // Set by service

        // Domain Entity -> Summary DTO
        CreateMap<Teacher, TeacherSummaryDto>();

        // Request DTO -> Domain Entity (handled by business logic, not auto-mapping)
        // This ensures we use proper domain methods like Teacher.Create()
    }
}
