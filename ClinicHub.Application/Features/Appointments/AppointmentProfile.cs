using AutoMapper;
using ClinicHub.Application.Features.Appointments.Commands.CreateAppointment;
using ClinicHub.Application.Features.Appointments.DTOs;
using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Features.Appointments
{
    public class AppointmentProfile : Profile
    {
        public AppointmentProfile()
        {
            CreateMap<CreateAppointmentCommand, Appointment>();

            CreateMap<Appointment, AppointmentDto>()
                .ForMember(dest => dest.AppointmentDate, opt => opt.MapFrom(src => src.AppointmentDate.ToString("dd/MM/yyyy hh:mm tt")))
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime.ToString(@"hh\:mm")))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime.ToString(@"hh\:mm")));
        }
    }
}
