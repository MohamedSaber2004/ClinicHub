using AutoMapper;
using ClinicHub.Application.Features.Availability.Commands.CreateNewAvailability;
using ClinicHub.Application.Features.Availability.Commands.DeleteAvailability;
using ClinicHub.Application.Features.Availability.Commands.UpdateExistingAvailability;
using ClinicHub.Application.Features.Availability.DTOs;
using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Features.Availability
{
    public class AvailabilityProfile : Profile
    {
        public AvailabilityProfile()
        {
            CreateMap<DoctorAvailability, AvailabilityDto>();
            CreateMap<CreateNewAvailabilityCommand, DoctorAvailability>();
            CreateMap<UpdateExistingAvailabilityCommand, DoctorAvailability>();
        }
    }
}
