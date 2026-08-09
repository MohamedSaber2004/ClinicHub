using ClinicHub.Application.Features.Ratings.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Ratings.Queries.GetReceptionRatings
{
    public class GetReceptionRatingsQuery : IRequest<List<RatingDto>>
    {
        public Guid ClinicId { get; set; }
    }
}
