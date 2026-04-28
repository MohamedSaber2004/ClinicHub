using AutoMapper;
using ClinicHub.Application.Features.Clinics.DTOs;
using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Features.Clinics
{
    public class ClinicProfile : Profile
    {
        public ClinicProfile()
        {
            CreateMap<Clinic, ClinicDto>()
                .ForMember(dest => dest.Lat, opt => opt.MapFrom(src => src.Location.Y))
                .ForMember(dest => dest.Lng, opt => opt.MapFrom(src => src.Location.X))
                .ForMember(dest => dest.SpecializationName, opt => opt.MapFrom(src => src.Specialization.Name));
        }
    }
}
