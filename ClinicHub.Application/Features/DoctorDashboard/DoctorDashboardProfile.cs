using AutoMapper;
using ClinicHub.Application.Features.DoctorDashboard.DTOs;
using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Features.DoctorDashboard
{
    public class DoctorDashboardProfile : Profile
    {
        public DoctorDashboardProfile()
        {
            CreateMap<Appointment, DoctorAppointmentDto>()
                .ForMember(dest => dest.AppointmentDate, opt => opt.MapFrom(src => src.AppointmentDate.ToString("yyyy-MM-dd")))
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime.ToString(@"hh\:mm")))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime.ToString(@"hh\:mm")))
                .ForMember(dest => dest.BookedByUserName, opt => opt.MapFrom(src => src.BookedByUser.FullName))
                .ForMember(dest => dest.BookedByUserPhone, opt => opt.MapFrom(src => src.BookedByUser.PhoneNumber))
                .ForMember(dest => dest.ClinicName, opt => opt.MapFrom(src => src.Clinic.Name));

            CreateMap<Appointment, PatientHistoryDto>()
                .ForMember(dest => dest.AppointmentId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.AppointmentDate, opt => opt.MapFrom(src => src.AppointmentDate.ToString("yyyy-MM-dd")))
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime.ToString(@"hh\:mm")))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime.ToString(@"hh\:mm")));
        }
    }
}
