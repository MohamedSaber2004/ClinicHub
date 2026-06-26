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
                .ForMember(dest => dest.SpecializationName, opt => opt.MapFrom(src => src.Specialization.Name))
                .ForMember(dest => dest.SpecializationNameAr, opt => opt.MapFrom(src => src.Specialization.ArName))
                .ForMember(dest => dest.WorkingDays, opt => opt.MapFrom(src =>
                    src.WorkingDays != null && src.WorkingHoursStart.HasValue && src.WorkingHoursEnd.HasValue
                        ? src.WorkingDays.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .Select(d => new WorkingDayDto
                            {
                                DayOfWeek = Enum.Parse<DayOfWeek>(d).ToString(),
                                StartTime = src.WorkingHoursStart!.Value,
                                EndTime = src.WorkingHoursEnd!.Value
                            }).ToList()
                        : null));
        }
    }
}
