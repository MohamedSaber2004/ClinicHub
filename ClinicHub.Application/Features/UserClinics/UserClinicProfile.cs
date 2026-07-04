using AutoMapper;
using ClinicHub.Application.Features.UserClinics.DTOs;
using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Features.UserClinics
{
    public class UserClinicProfile : Profile
    {
        public UserClinicProfile()
        {
            CreateMap<UserClinic, FollowedClinicDto>()
                .ForMember(dest => dest.ClinicId, opt => opt.MapFrom(src => src.ClinicId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Clinic.Name))
                .ForMember(dest => dest.NameAr, opt => opt.MapFrom(src => src.Clinic.NameAr))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Clinic.Address))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Clinic.Phone))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Clinic.ImageUrl))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Clinic.Rating))
                .ForMember(dest => dest.FollowedAt, opt => opt.MapFrom(src => src.FollowedAt));

            CreateMap<UserClinic, ClinicFollowerDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.FollowedAt, opt => opt.MapFrom(src => src.FollowedAt));
        }
    }
}
