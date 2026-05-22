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
            CreateMap<Appointment, AppointmentDto>();
        }
    }
}
