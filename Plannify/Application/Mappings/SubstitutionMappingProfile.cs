using AutoMapper;
using Plannify.Application.DTOs;
using Plannify.Domain.Entities;

namespace Plannify.Application.Mappings;

/// <summary>
/// AutoMapper profile for Substitution entity mappings
/// </summary>
public class SubstitutionMappingProfile : Profile
{
    public SubstitutionMappingProfile()
    {
        // Map Substitution domain entity to the main DTO
        CreateMap<Substitution, SubstitutionDto>()
            .ForMember(dest => dest.IsUrgent, opt => opt.MapFrom(src => src.IsUrgent()));

        // Map Substitution to summary DTO
        CreateMap<Substitution, SubstitutionSummaryDto>();

        // Map request DTO to domain entity (for Create)
        CreateMap<CreateSubstitutionRequest, Substitution>()
            .ConstructUsing(src => new Substitution()); // Use reflection constructor

        // Map request DTO to domain entity (for Update)
        CreateMap<UpdateSubstitutionRequest, Substitution>();

        // Map request DTO to domain entity (for ChangeSubstitute)
        CreateMap<ChangeSubstituteRequest, Substitution>();
    }
}
