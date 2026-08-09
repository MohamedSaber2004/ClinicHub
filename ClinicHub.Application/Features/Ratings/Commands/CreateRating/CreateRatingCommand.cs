using ClinicHub.Application.Features.Ratings.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Ratings.Commands.CreateRating
{
    public class CreateRatingCommand : IRequest<RatingDto>
    {
        public RatingType? Type { get; set; }
        public Guid? DoctorId { get; set; }
        public Guid? ClinicId { get; set; }
        public int Value { get; set; }
        public string? Review { get; set; }
    }
}
