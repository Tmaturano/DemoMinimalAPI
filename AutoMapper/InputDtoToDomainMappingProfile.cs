using AutoMapper;
using DemoMinimalAPI.DTOs;
using DemoMinimalAPI.Models;

namespace DemoMinimalAPI.AutoMapper
{
    public class InputDtoToDomainMappingProfile : Profile
    {
        public InputDtoToDomainMappingProfile()
        {
            CreateMap<SupplierInputDto, Supplier>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Document, opt => opt.MapFrom(src => src.Document))
                .ForMember(dest => dest.Active, opt => opt.MapFrom(src => src.Active));
        }
    }
}
