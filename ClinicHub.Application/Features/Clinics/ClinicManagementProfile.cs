using AutoMapper;
using ClinicHub.Application.Features.Clinics.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;

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
                .ForMember(dest => dest.SpecializationNameAr, opt => opt.MapFrom(src => src.Specialization.ArName))
                .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.ClinicAdmin != null ? src.ClinicAdmin.FullName : null))
                .ForMember(dest => dest.OwnerEmail, opt => opt.MapFrom(src => src.ClinicAdmin != null ? src.ClinicAdmin.Email : null))
                .ForMember(dest => dest.OwnerPhone, opt => opt.MapFrom(src => src.ClinicAdmin != null ? src.ClinicAdmin.PhoneNumber : null))
                .ForMember(dest => dest.SubscriptionStatus, opt => opt.MapFrom(src =>
                    src.Subscriptions != null && src.Subscriptions.Any()
                        ? src.Subscriptions.OrderByDescending(s => s.EndDate).Select(s => (SubscriptionStatus?)s.Status).FirstOrDefault()
                        : null))
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
