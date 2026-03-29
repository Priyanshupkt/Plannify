using AutoMapper;
using Plannify.Application.DTOs;
using DomainRoom = Plannify.Domain.Entities.Room;

namespace Plannify.Application.Mappings;

/// <summary>
/// AutoMapper profile for Room entity to DTO mappings
/// </summary>
public class RoomMappingProfile : Profile
{
    public RoomMappingProfile()
    {
        // Domain Entity -> Response DTO
        CreateMap<DomainRoom, RoomDto>();

        // Domain Entity -> Summary DTO
        CreateMap<DomainRoom, RoomSummaryDto>();
    }
}
