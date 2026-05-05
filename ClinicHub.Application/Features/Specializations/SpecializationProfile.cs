using AutoMapper;
using ClinicHub.Application.Features.Specializations.Commands.CreateSpecialization;
using ClinicHub.Application.Features.Specializations.Commands.UpdateSpecialization;
using ClinicHub.Application.Features.Specializations.DTOs;
using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Features.Specializations
{
    public class SpecializationProfile : Profile
    {
        public SpecializationProfile()
        {
            CreateMap<Specialization, SpecializationDto>();
            CreateMap<CreateSpecializationCommand, Specialization>();
            CreateMap<UpdateSpecializationCommand, Specialization>();
        }
    }
}
