using AutoMapper;
using TicoAutos.Domain.Entities;
using TicoAutos.Application.DTOs.Vehicles;

namespace TicoAutos.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        /// Entity to DTO (GET/Response)
        CreateMap<Vehicle, VehicleResponseDto>()
            .ForMember(dest => dest.OwnerName,
                opt => opt.MapFrom(src => src.Owner != null ? src.Owner.Name : "Vendedor"));

        /// DTO to Entity for Create (POST)
        CreateMap<CreateVehicleRequest, Vehicle>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Questions, opt => opt.Ignore())
            .ForMember(dest => dest.Owner, opt => opt.Ignore())
            .ForMember(dest => dest.IsSold, opt => opt.MapFrom(src => false));

        /// DTO to Entity for Update (PUT)
        CreateMap<UpdateVehicleRequest, Vehicle>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Questions, opt => opt.Ignore())
            .ForMember(dest => dest.Owner, opt => opt.Ignore());
    }
}