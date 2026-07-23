using AutoMapper;
using ClinicHub.Application.Features.Subscriptions.DTOs;
using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Features.Subscriptions
{
    public class SubscriptionProfile : Profile
    {
        public SubscriptionProfile()
        {
            CreateMap<Subscription, SubscriptionDto>()
                .ForMember(dest => dest.ClinicName, opt => opt.MapFrom(src => src.Clinic.Name))
                .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src => src.Plan.Name))
                .ForMember(dest => dest.Permissions,
                    opt => opt.MapFrom(src => src.Plan != null
                        ? src.Plan.Permissions.Select(p => p.Permission.ToString()).ToList()
                        : null));
        }
    }
}
