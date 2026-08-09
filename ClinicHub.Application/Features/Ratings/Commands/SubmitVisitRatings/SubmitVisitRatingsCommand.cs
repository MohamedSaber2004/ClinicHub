using ClinicHub.Application.Features.Ratings.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Ratings.Commands.SubmitVisitRatings
{
    public class SubmitVisitRatingsCommand : IRequest<List<RatingDto>>
    {
        public Guid? DoctorId { get; set; }
        public Guid? ClinicId { get; set; }
        public int? DoctorValue { get; set; }
        public int ClinicValue { get; set; }
        public int CleanlinessValue { get; set; }
        public string? Review { get; set; }
    }
}
