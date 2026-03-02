using AutoMapper;
using TicoAutos.Domain.Entities;
using TicoAutos.Application.DTOs.Vehicles;

namespace TicoAutos.Application.Mappings;

public class MappingProfile : Profile
{

    public MappingProfile()
    {
        /// Entity to DTO (Para el GET/Respuesta)    
        CreateMap<Vehicle, VehicleResponseDto>();


        /// DTO to Entity for Create (Para el POST/Creación)
        CreateMap<CreateVehicleRequest, Vehicle>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Questions, opt => opt.Ignore())
            .ForMember(dest => dest.IsSold, opt => opt.MapFrom(src => false));
    }
}