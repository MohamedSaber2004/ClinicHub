using AutoMapper;
using ClinicHub.Application.Features.Clinics.DTOs;
using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Features.Clinics
{
    public class ClinicManagementProfile : Profile
    {
        public ClinicManagementProfile()
        {
            CreateMap<Clinic, ClinicManagementDto>()
                .ForMember(dest => dest.Lat, opt => opt.MapFrom(src => src.Location != null ? src.Location.Y : (double?)null))
                .ForMember(dest => dest.Lng, opt => opt.MapFrom(src => src.Location != null ? src.Location.X : (double?)null))
                .ForMember(dest => dest.SpecializationName, opt => opt.MapFrom(src => src.Specialization.Name))
                .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.ClinicAdmin != null ? src.ClinicAdmin.FullName : null))
                .ForMember(dest => dest.OwnerEmail, opt => opt.MapFrom(src => src.ClinicAdmin != null ? src.ClinicAdmin.Email : null))
                .ForMember(dest => dest.OwnerPhone, opt => opt.MapFrom(src => src.ClinicAdmin != null ? src.ClinicAdmin.PhoneNumber : null))
                .ForMember(dest => dest.WorkingDays, opt => opt.MapFrom(src =>
                    src.WorkingDays != null
                        ? src.WorkingDays.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .Select(d => Enum.Parse<DayOfWeek>(d)).ToList()
                        : null));
        }
    }
}
