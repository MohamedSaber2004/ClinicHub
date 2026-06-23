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
                .ForMember(dest => dest.AppointmentDate, opt => opt.MapFrom(src => src.AppointmentDate.ToString("yyyy-MM-dd")))
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime.ToString(@"hh\:mm")))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime.ToString(@"hh\:mm")))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Doctor != null ? src.Doctor.ConsultationFee : 0m))
                .ForMember(dest => dest.Currency, opt => opt.Ignore())
                .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Doctor != null && src.Doctor.User != null ? src.Doctor.User.FullName : null))
                .ForMember(dest => dest.ClinicName, opt => opt.MapFrom(src => src.Clinic != null ? src.Clinic.Name : null))
                .ForMember(dest => dest.ReceiptUrl, opt => opt.MapFrom(src => src.BookingReference != null ? $"https://receipts.example.com/{src.BookingReference}" : null));
        }
    }
}
