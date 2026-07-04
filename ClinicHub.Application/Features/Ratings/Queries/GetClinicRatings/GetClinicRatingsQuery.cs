using ClinicHub.Application.Features.Ratings.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Ratings.Queries.GetClinicRatings
{
    public class GetClinicRatingsQuery : IRequest<List<RatingDto>>
    {
        public Guid ClinicId { get; set; }
    }
}
