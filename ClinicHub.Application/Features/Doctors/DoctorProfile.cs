using AutoMapper;
using ClinicHub.Application.Features.Doctors.Commands.CreateDoctor;
using ClinicHub.Application.Features.Doctors.DTOs;
using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Features.Doctors
{
    public class DoctorProfile : Profile
    {
        public DoctorProfile()
        {
            CreateMap<CreateDoctorCommand, Doctor>();

            CreateMap<Doctor, DoctorDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : null))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : null))
                .ForMember(dest => dest.ClinicName, opt => opt.MapFrom(src => src.Clinic != null ? src.Clinic.Name : null))
                .ForMember(dest => dest.SpecializationName, opt => opt.MapFrom(src => src.Specialization != null ? src.Specialization.ArName : null));
        }
    }
}
