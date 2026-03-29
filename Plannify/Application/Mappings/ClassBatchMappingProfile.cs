using AutoMapper;
using Plannify.Application.DTOs;
using Plannify.Domain.Entities;

namespace Plannify.Application.Mappings;

/// <summary>
/// AutoMapper profile for ClassBatch entity to DTO mappings
/// </summary>
public class ClassBatchMappingProfile : Profile
{
    public ClassBatchMappingProfile()
    {
        // Domain Entity -> Response DTO
        CreateMap<ClassBatch, ClassBatchDto>();

        // Domain Entity -> Summary DTO
        CreateMap<ClassBatch, ClassBatchSummaryDto>();

        // Request DTO -> Domain Entity (handled by business logic, not auto-mapping)
        // This ensures we use proper domain methods like ClassBatch.Create()
    }
}
