using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Appointments.DTOs;
using ClinicHub.Application.Features.Payment.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;

namespace ClinicHub.Application.Common.Services;

/// <summary>
/// Shared implementation of the appointment acceptance flow.
/// Authorization must be performed by the caller before invoking <see cref="AcceptAsync"/>.
/// </summary>
public class AppointmentAcceptanceService : IAppointmentAcceptanceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymobService _paymobService;
    private readonly IFcmService _fcmService;
    private readonly IBackgroundJobScheduler _jobScheduler;

    public AppointmentAcceptanceService(
        IUnitOfWork unitOfWork,
        IPaymobService paymobService,
        IFcmService fcmService,
        IBackgroundJobScheduler jobScheduler)
    {
        _unitOfWork = unitOfWork;
        _paymobService = paymobService;
        _fcmService = fcmService;
        _jobScheduler = jobScheduler;
    }

    public async Task<AppointmentAcceptanceResultDto> AcceptAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        // Guard: accept only from a pending request (requested / reserved hold).
        if (appointment.Status != AppointmentStatus.Pending && appointment.Status != AppointmentStatus.Reserved)
            throw new BadRequestException(LocalizationKeys.AppointmentMessages.CannotRespondAppointment.Value);

        // Guard: double accept — a paid/refunded payment already exists for this request.
        var existingPayment = await _unitOfWork.PaymentRepository.GetByAppointmentIdAsync(appointment.Id);
        if (existingPayment is not null && existingPayment.Status is PaymentStatus.Paid or PaymentStatus.Refunded)
            throw new ConflictException(LocalizationKeys.PaymentMessages.AlreadyAcceptedPayment.Value);

        // Clinic fee: required for the payment record — fail gracefully with a localized error.
        var bookingConfig = await _unitOfWork.BookingConfigurationRepository.GetByClinicIdAsync(appointment.ClinicId);
        if (bookingConfig is null || bookingConfig.ConsultationFee <= 0)
            throw new BadRequestException(LocalizationKeys.BookingMessages.FeeNotConfigured.Value);

        var amount = bookingConfig.ConsultationFee;
        var currency = string.IsNullOrWhiteSpace(bookingConfig.Currency) ? "EGP" : bookingConfig.Currency;

        var patientUser = await _unitOfWork.GetRepository<ApplicationUser, Guid>().GetByIdAsync(appointment.BookedByUserId);
        var billing = CreateBillingData(patientUser);

        // Initiate the Paymob hosted checkout first — if it fails nothing is persisted.
        var checkout = await _paymobService.InitiateCheckoutPaymentAsync(amount, currency, billing, cancellationToken);

        if (existingPayment is null)
        {
            existingPayment = new Payment(PaymentType.Appointment, appointment.BookedByUserId, appointment.ClinicId, amount, currency)
            {
                PaymobOrderId = checkout.OrderId
            };
            existingPayment.LinkToAppointment(appointment.Id);
            await _unitOfWork.PaymentRepository.AddAsync(existingPayment);
        }
        else
        {
            // Refresh an earlier unpaid checkout (e.g. a leftover pre-accept payment record).
            existingPayment.PaymobOrderId = checkout.OrderId;
        }

        existingPayment.SetPaymobCheckout(checkout.RedirectUrl);
        appointment.Accept();

        await _unitOfWork.SaveChangesAsync();

        // Auto-mark as no-show after the appointment ends (plus a grace period) if never attended.
        await _jobScheduler.ScheduleNoShowMarkingAsync(appointment.Id, appointment.AppointmentDate.Add(appointment.EndTime).AddMinutes(30));

        // Notify the patient that the request was accepted and payment is awaited.
        await _fcmService.SendToUserAsync(appointment.BookedByUserId, NotificationType.AppointmentConfirmation, new()
        {
            ["clinicName"] = appointment.Clinic?.Name ?? "",
            ["date"] = appointment.AppointmentDate.ToString("yyyy-MM-dd"),
            ["appointmentId"] = appointment.Id.ToString(),
            ["paymentUrl"] = checkout.RedirectUrl
        });

        return new AppointmentAcceptanceResultDto
        {
            AppointmentId = appointment.Id,
            Status = AppointmentStatus.Accepted,
            PaymentId = existingPayment.Id,
            Amount = amount,
            Currency = currency,
            PaymobRedirectUrl = checkout.RedirectUrl,
            PaymobPaymentKey = checkout.PaymentKey
        };
    }

    private static PaymentBillingData CreateBillingData(ApplicationUser? user)
    {
        var names = user?.FullName?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? new[] { "Clinic", "User" };
        var firstName = names[0];
        var lastName = names.Length > 1 ? string.Join(" ", names.Skip(1)) : "User";

        return new PaymentBillingData
        {
            FirstName = firstName,
            LastName = lastName,
            Email = string.IsNullOrWhiteSpace(user?.Email) ? "patient@clinichub.com" : user.Email,
            PhoneNumber = string.IsNullOrWhiteSpace(user?.PhoneNumber) ? "01000000000" : user.PhoneNumber,
            City = "Cairo",
            Country = "EG",
            Street = "NA",
            Building = "NA",
            Apartment = "NA",
            Floor = "NA",
            PostalCode = "NA",
            State = "Cairo"
        };
    }
}
