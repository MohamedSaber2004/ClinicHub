using ClinicHub.Application.Features.Ratings.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Ratings.Queries.GetDoctorRatings
{
    public class GetDoctorRatingsQuery : IRequest<List<RatingDto>>
    {
        public Guid DoctorId { get; set; }
    }
}
