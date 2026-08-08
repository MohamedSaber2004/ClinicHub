using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Appointments.DTOs;
using ClinicHub.Application.Features.Payment.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
    private readonly ILogger<AppointmentAcceptanceService> _logger;

    public AppointmentAcceptanceService(
        IUnitOfWork unitOfWork,
        IPaymobService paymobService,
        IFcmService fcmService,
        IBackgroundJobScheduler jobScheduler,
        ILogger<AppointmentAcceptanceService> logger)
    {
        _unitOfWork = unitOfWork;
        _paymobService = paymobService;
        _fcmService = fcmService;
        _jobScheduler = jobScheduler;
        _logger = logger;
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

        // Notify the doctor and the clinic owner that the appointment was approved
        // (patient-facing confirmation is sent above; this keeps the doctor's
        // appointment list and the owner's overview in sync with the acceptance).
        await NotifyDoctorAndOwnerAsync(appointment, cancellationToken);

        // Commit the notification rows added by the sends above. The payment and the
        // acceptance were already committed above (line 83) because the no-show job
        // and the doctor/owner notification re-query require the row to exist.
        await _unitOfWork.SaveChangesAsync();

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

    private async Task NotifyDoctorAndOwnerAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        try
        {
            var loaded = await _unitOfWork.AppointmentRepository
                .GetFirstWithIncluding(a => a.Id == appointment.Id, a => a.Doctor, a => a.Clinic!)
                .FirstOrDefaultAsync(cancellationToken);

            if (loaded == null)
                return;

            var patientUser = await _unitOfWork.GetRepository<ApplicationUser, Guid>()
                .GetByIdAsync(appointment.BookedByUserId);

            var parameters = new Dictionary<string, object>
            {
                ["patientName"] = patientUser?.FullName ?? "",
                ["clinicName"] = loaded.Clinic?.Name ?? "",
                ["doctorName"] = loaded.Doctor?.User?.FullName ?? "",
                ["date"] = appointment.AppointmentDate.ToString("yyyy-MM-dd"),
                ["time"] = $"{appointment.StartTime:hh\\:mm} - {appointment.EndTime:hh\\:mm}",
                ["appointmentId"] = appointment.Id.ToString()
            };

            var recipients = new HashSet<Guid>();

            if (loaded.Doctor != null)
                recipients.Add(loaded.Doctor.UserId);

            if (loaded.Clinic?.ClinicAdminId.HasValue == true)
                recipients.Add(loaded.Clinic.ClinicAdminId.Value);

            foreach (var userId in recipients)
            {
                await _fcmService.SendToUserAsync(userId, NotificationType.AppointmentAccepted, parameters);
            }
        }
        catch (Exception ex)
        {
            // Never block the acceptance flow on a push failure — the in-app
            // notification record and dispatch are best-effort.
            _logger.LogError(ex, "Failed to send appointment-accepted notifications for appointment {AppointmentId}.", appointment.Id);
        }
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
