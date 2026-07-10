using AutoMapper;
using ClinicHub.Application.Features.Users.DTOs;
using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Features.Users
{
    public class UserVerificationProfile : Profile
    {
        public UserVerificationProfile()
        {
            CreateMap<UserVerification, UserVerificationDto>()
                .ForMember(d => d.UserFullName, opt => opt.MapFrom(s => s.User.FullName))
                .ForMember(d => d.UserEmail, opt => opt.MapFrom(s => s.User.Email))
                .ForMember(d => d.UserPhoneNumber, opt => opt.MapFrom(s => s.User.PhoneNumber))
                .ForMember(d => d.ReviewedByFullName, opt => opt.MapFrom(s => s.ReviewedBy != null ? s.ReviewedBy.FullName : null));
        }
    }
}
