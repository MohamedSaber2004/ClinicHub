using AutoMapper;
using ClinicHub.Application.Features.Advertisements.DTOs;
using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Features.Advertisements
{
    public class AdvertisementProfile : Profile
    {
        public AdvertisementProfile()
        {
            CreateMap<Advertisement, AdvertisementDto>()
                .ForMember(dest => dest.ClinicName, opt => opt.MapFrom(src => src.Clinic != null ? src.Clinic.Name : null));
        }
    }
}
