using AutoMapper;
using ClinicHub.Application.Features.Clinics.DTOs;
using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Features.Clinics
{
    public class ClinicProfile : Profile
    {
        public ClinicProfile()
        {
            CreateMap<Clinic, ClinicSettingsDto>()
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Location.Y))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Location.X))
                .ForMember(dest => dest.SpecializationName, opt => opt.MapFrom(src => src.Specialization.Name))
                .ForMember(dest => dest.SpecializationNameAr, opt => opt.MapFrom(src => src.Specialization.ArName));

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
                                // Never throw on unexpected stored values (e.g. lowercase or
                                // localized day names sent by mobile at registration): fall back
                                // to the raw value so one bad row can't break the whole search.
                                DayOfWeek = WorkingDayDto.ToDayLabel(d),
                                StartTime = src.WorkingHoursStart!.Value,
                                EndTime = src.WorkingHoursEnd!.Value
                            }).ToList()
                        : null));

            CreateMap<Clinic, ClinicDetailsDto>()
                .ForMember(dest => dest.Lat, opt => opt.MapFrom(src => src.Location.Y))
                .ForMember(dest => dest.Lng, opt => opt.MapFrom(src => src.Location.X))
                .ForMember(dest => dest.SpecializationName, opt => opt.MapFrom(src => src.Specialization.Name))
                .ForMember(dest => dest.SpecializationNameAr, opt => opt.MapFrom(src => src.Specialization.ArName))
                .ForMember(dest => dest.WorkingDays, opt => opt.MapFrom(src =>
                    src.WorkingDays != null && src.WorkingHoursStart.HasValue && src.WorkingHoursEnd.HasValue
                        ? src.WorkingDays.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .Select(d => new WorkingDayDto
                            {
                                DayOfWeek = WorkingDayDto.ToDayLabel(d),
                                StartTime = src.WorkingHoursStart!.Value,
                                EndTime = src.WorkingHoursEnd!.Value
                            }).ToList()
                        : null));
        }
    }
}
