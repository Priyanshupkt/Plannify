using AutoMapper;
using Plannify.Application.DTOs;
using Plannify.Domain.Entities;

namespace Plannify.Application.Mappings;

/// <summary>
/// AutoMapper profile for Timetable entity mappings
/// </summary>
public class TimetableMappingProfile : Profile
{
    public TimetableMappingProfile()
    {
        // Map Timetable to TimetableDto (full detail view)
        CreateMap<Timetable, TimetableDto>()
            .ForMember(dest => dest.SlotCount, opt => opt.MapFrom(src => src.TimetableSlots.Count))
            .ForMember(dest => dest.Slots, opt => opt.MapFrom(src => src.TimetableSlots))
            .ForMember(dest => dest.SemesterCode, opt => opt.Ignore()); // Will be populated by service if needed

        // Map Timetable to TimetableSummaryDto (minimal info for lists)
        CreateMap<Timetable, TimetableSummaryDto>()
            .ForMember(dest => dest.SlotCount, opt => opt.MapFrom(src => src.TimetableSlots.Count));

        // Map TimetableSlot to TimetableSlotSummaryDto
        CreateMap<TimetableSlot, TimetableSlotSummaryDto>()
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime.ToString("HH:mm")))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime.ToString("HH:mm")));

        // Map TimetableStatistics to TimetableStatisticsDto
        CreateMap<TimetableStatistics, TimetableStatisticsDto>()
            .ForMember(dest => dest.CompletionPercentage, opt => opt.MapFrom(src => src.GetCompletionPercentage()));
    }
}
