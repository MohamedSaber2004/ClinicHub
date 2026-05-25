using FluentValidation;

namespace ClinicHub.Application.Features.Payment.Queries.GetPaymentStatus;

public class GetPaymentStatusQueryValidator : AbstractValidator<GetPaymentStatusQuery>
{
    public GetPaymentStatusQueryValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty();
    }
}
