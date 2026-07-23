using AutoMapper;
using ClinicHub.Application.Features.StaffDashboard.DTOs;
using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Features.StaffDashboard
{
    public class StaffDashboardProfile : Profile
    {
        public StaffDashboardProfile()
        {
            CreateMap<Appointment, StaffAppointmentDto>()
                .ForMember(dest => dest.AppointmentDate, opt => opt.MapFrom(src => src.AppointmentDate.ToString("yyyy-MM-dd")))
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime.ToString(@"hh\:mm")))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime.ToString(@"hh\:mm")))
                .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Doctor.User.FullName))
                .ForMember(dest => dest.BookedByUserName, opt => opt.MapFrom(src => src.BookedByUser.FullName));
        }
    }
}
