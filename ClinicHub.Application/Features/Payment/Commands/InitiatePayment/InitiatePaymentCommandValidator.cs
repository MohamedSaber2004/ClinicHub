using FluentValidation;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using ClinicHub.Application.Localization;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Payment.Commands.InitiatePayment;

public class InitiatePaymentCommandValidator : AbstractValidator<InitiatePaymentCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public InitiatePaymentCommandValidator(
        IUnitOfWork unitOfWork, 
        ICurrentUserService currentUser,
        IStringLocalizer<Messages> localizer)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;

        RuleFor(x => x.AppointmentId)
            .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value, localizer["Appointment"]])
            .MustAsync(async (id, ct) => await _unitOfWork.AppointmentRepository.ExistsByKeyAsync(id, ct))
                .WithMessage(localizer[LocalizationKeys.PaymentMessages.AppointmentNotFound.Value])
            .MustAsync(async (id, ct) =>
            {
                var appt = await _unitOfWork.AppointmentRepository.GetByIdAsync(id);
                return appt != null && appt.BookedByUserId == _currentUser.UserId;
            }).WithMessage(localizer[LocalizationKeys.PaymentMessages.Unauthorized.Value])
            .MustAsync(async (id, ct) =>
            {
                var appt = await _unitOfWork.AppointmentRepository.GetByIdAsync(id);
                return appt != null && (appt.Status == AppointmentStatus.Pending || appt.Status == AppointmentStatus.Reserved);
            }).WithMessage(localizer[LocalizationKeys.PaymentMessages.AppointmentNotPending.Value])
            .MustAsync(async (id, ct) =>
            {
                var payment = await _unitOfWork.PaymentRepository.GetByAppointmentIdAsync(id);
                // Allow if: no payment exists OR payment is not Paid
                return payment == null || payment.Status != PaymentStatus.Paid;
            }).WithMessage(localizer[LocalizationKeys.PaymentMessages.AlreadyPaid.Value]);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage(localizer[LocalizationKeys.PaymentMessages.PhoneNumberRequired.Value])
            .Matches(@"^01[0125][0-9]{8}$").WithMessage(localizer[LocalizationKeys.PaymentMessages.InvalidPhoneNumber.Value]);
    }
}
