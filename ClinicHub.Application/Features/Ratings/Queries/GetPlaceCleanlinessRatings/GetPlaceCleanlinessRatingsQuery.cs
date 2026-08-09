using ClinicHub.Application.Features.Ratings.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Ratings.Queries.GetPlaceCleanlinessRatings
{
    public class GetPlaceCleanlinessRatingsQuery : IRequest<List<RatingDto>>
    {
        public Guid ClinicId { get; set; }
    }
}
