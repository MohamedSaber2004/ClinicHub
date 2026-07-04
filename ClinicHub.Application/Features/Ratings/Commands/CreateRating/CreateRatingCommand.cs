using ClinicHub.Application.Features.Ratings.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Ratings.Commands.CreateRating
{
    public class CreateRatingCommand : IRequest<RatingDto>
    {
        public Guid? DoctorId { get; set; }
        public Guid? ClinicId { get; set; }
        public int Value { get; set; }
        public string? Review { get; set; }
    }
}
