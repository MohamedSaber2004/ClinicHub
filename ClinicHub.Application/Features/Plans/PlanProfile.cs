using AutoMapper;
using ClinicHub.Application.Features.Plans.DTOs;
using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Features.Plans
{
    public class PlanProfile : Profile
    {
        public PlanProfile()
        {
            CreateMap<Plan, PlanDto>()
                .ForMember(dest => dest.Permissions,
                    opt => opt.MapFrom(src => src.Permissions.Select(p => p.Permission.ToString()).ToList()));
        }
    }
}
