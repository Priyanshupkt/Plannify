using AutoMapper;
using Plannify.Application.DTOs;
using Plannify.Domain.Entities;

namespace Plannify.Application.Mappings;

/// <summary>
/// AutoMapper profile for TimetableSlot entity to DTO mappings
/// </summary>
public class TimetableSlotMappingProfile : Profile
{
    public TimetableSlotMappingProfile()
    {
        // Domain Entity -> Response DTO
        CreateMap<TimetableSlot, TimetableSlotDto>();

        // Domain Entity -> Summary DTO
        CreateMap<TimetableSlot, TimetableSlotSummaryDto>();

        // Request DTO -> Domain Entity (handled by business logic, not auto-mapping)
        // This ensures we use proper domain methods like TimetableSlot.Create()
    }
}
