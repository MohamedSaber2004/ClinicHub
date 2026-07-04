using FluentValidation;

namespace ClinicHub.Application.Features.Availability.Queries.GetAvailableSlots
{
    public class GetAvailableSlotsQueryValidator: AbstractValidator<GetAvailableSlotsQuery>
    {
        public GetAvailableSlotsQueryValidator()
        {
            RuleFor(x => x.DoctorId)
                .NotEmpty();

            RuleFor(x => x.ClinicId)
                .NotEmpty();
        }
    }
}
