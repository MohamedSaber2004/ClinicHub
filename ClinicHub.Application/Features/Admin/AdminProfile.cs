using AutoMapper;
using ClinicHub.Application.Features.Admin.DTOs;
using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Features.Admin
{
    public class AdminProfile : Profile
    {
        public AdminProfile()
        {
            CreateMap<SupportTicket, SupportTicketDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName));

            CreateMap<AuditLog, AuditLogDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : null));

            CreateMap<Clinic, ClinicLookupDto>();
        }
    }
}
