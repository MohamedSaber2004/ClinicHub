using AutoMapper;
using ClinicHub.Application.Features.Ratings.DTOs;
using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Features.Ratings
{
    public class RatingProfile : Profile
    {
        public RatingProfile()
        {
            CreateMap<Rating, RatingDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : null));
        }
    }
}
