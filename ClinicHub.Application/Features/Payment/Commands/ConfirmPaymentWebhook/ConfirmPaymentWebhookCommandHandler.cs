using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Application.Features.Payment.Commands.ConfirmPaymentWebhook;

public class ConfirmPaymentWebhookCommandHandler : IRequestHandler<ConfirmPaymentWebhookCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymobService _paymobService;
    private readonly ILogger<ConfirmPaymentWebhookCommandHandler> _logger;
    private readonly IFcmService _fcmService;
    private readonly IBackgroundJobScheduler _jobScheduler;
    private readonly UserManager<ApplicationUser> _userManager;

    public ConfirmPaymentWebhookCommandHandler(IUnitOfWork unitOfWork, IPaymobService paymobService, ILogger<ConfirmPaymentWebhookCommandHandler> logger, IFcmService fcmService, IBackgroundJobScheduler jobScheduler, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _paymobService = paymobService;
        _logger = logger;
        _fcmService = fcmService;
        _jobScheduler = jobScheduler;
        _userManager = userManager;
    }

    public async Task<bool> Handle(ConfirmPaymentWebhookCommand request, CancellationToken cancellationToken)
    {
        if (!string.Equals(request.Type, "TRANSACTION", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(request.Hmac))
        {
            _logger.LogWarning("Paymob webhook rejected: empty HMAC.");
            return false;
        }

        var transaction = request.Transaction;
        if (transaction == null || transaction.Order?.Id == 0)
        {
            _logger.LogWarning("Paymob webhook rejected: missing transaction or order id.");
            return false;
        }

        var transactionData = TransactionToDictionary(transaction);

        var isValid = await _paymobService.ValidateWebhookAsync(request.Hmac, transactionData);
        if (!isValid)
        {
            _logger.LogWarning("Paymob webhook rejected: HMAC validation failed for order {OrderId} transaction {TransactionId}.", transaction.Order.Id, transaction.Id);
            return false;
        }


        var orderId = transaction.Order.Id.ToString();
        var payment = await _unitOfWork.PaymentRepository.GetAllAsync(x => x.PaymobOrderId == orderId).FirstOrDefaultAsync(cancellationToken);
        if (payment == null)
        {
            _logger.LogWarning("Paymob webhook: no payment found for order {OrderId} transaction {TransactionId}.", orderId, transaction.Id);
            return false;
        }


        // Idempotency: skip only terminal states. Failed is deliberately NOT skipped â€”
        // the patient can retry the same checkout (same Paymob order) after a failed attempt.
        if (payment.Status is PaymentStatus.Paid or PaymentStatus.Refunded)
        {
            return true;
        }

        if (transaction.Success)
        {
            payment.MarkAsPaid(transaction.Id.ToString(), transaction.SourceData?.SubType ?? "Unknown");

            if (payment.Type == PaymentType.Appointment && payment.AppointmentId.HasValue)
            {
                var appointment = await _unitOfWork.AppointmentRepository
                    .GetAllAsync(x => x.Id == payment.AppointmentId.Value)
                    .Include(a => a.Clinic!)
                    .FirstOrDefaultAsync(cancellationToken);
                appointment?.Confirm(payment.Id);

                if (appointment is not null)
                {
                    try
                    {
                        await _fcmService.SendToUserAsync(appointment.BookedByUserId, NotificationType.PaymentConfirmation, new()
                        {
                            ["amount"] = $"{payment.Amount:N2} EGP",
                            ["appointmentId"] = appointment.Id.ToString()
                        });

                        await NotifyClinicOwnerAndSuperAdminsAsync(appointment, payment);

                        var bookingConfig = await _unitOfWork.BookingConfigurationRepository.GetByClinicIdAsync(appointment.ClinicId);
                        var windowMinutes = bookingConfig?.CancellationWindowMinutes ?? 120;
                        if (payment.PaidAt.HasValue)
                            await _jobScheduler.ScheduleCancellationWindowCloseAsync(appointment.Id, payment.PaidAt.Value.AddMinutes(windowMinutes));

                        await _jobScheduler.ScheduleNoShowMarkingAsync(appointment.Id, appointment.AppointmentDate.Add(appointment.EndTime).AddMinutes(30));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Non-critical side effects (FCM/scheduling) failed after payment {PaymentId} was marked paid; payment will still be confirmed.", payment.Id);
                    }
                }
            }
            else if (payment.Type == PaymentType.Ads)
            {
                var advertisement = await _unitOfWork.GetRepository<Advertisement, Guid>()
                    .GetAllAsync(a => a.PaymentId == payment.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (advertisement != null && advertisement.Status == AdvertisementStatus.PendingPayment)
                {
                    advertisement.Activate(DateTime.Now, advertisement.DurationDays);
                    _unitOfWork.GetRepository<Advertisement, Guid>().Update(advertisement);
                    await _jobScheduler.ScheduleAdExpirationAsync(advertisement.Id, advertisement.EndDate);
                }
            }
            else if (payment.Type == PaymentType.Subscription && payment.PlanId.HasValue && payment.SubscriptionPeriod.HasValue)
            {
                if (payment.SubscriptionId.HasValue)
                {
                    _logger.LogWarning("Payment {PaymentId} already has subscription {SubscriptionId}. Skipping duplicate.", payment.Id, payment.SubscriptionId);
                    return true;
                }

                var existingActiveSubs = await _unitOfWork.GetRepository<Subscription, Guid>()
                    .GetAllAsync(s => s.ClinicId == payment.ClinicId && s.Status == SubscriptionStatus.Active)
                    .ToListAsync(cancellationToken);

                foreach (var activeSub in existingActiveSubs)
                {
                    activeSub.Status = SubscriptionStatus.Revoked;
                    activeSub.Notes = "Revoked due to new subscription payment confirmation.";
                }

                var period = payment.SubscriptionPeriod.Value;
                var now = DateTime.Now;
                var endDate = period == SubscriptionPlan.Yearly ? now.AddYears(1) : now.AddMonths(1);

                var subscription = new Subscription
                {
                    ClinicId = payment.ClinicId,
                    PlanId = payment.PlanId.Value,
                    Period = period,
                    StartDate = now,
                    EndDate = endDate,
                    Amount = payment.Amount,
                    Status = SubscriptionStatus.Active,
                    PaidAt = now,
                    PaymentId = payment.Id
                };

                await _unitOfWork.GetRepository<Subscription, Guid>().AddAsync(subscription);
                payment.LinkToSubscription(subscription.Id);
                await _jobScheduler.ScheduleSubscriptionExpirationAsync(subscription.Id, subscription.EndDate);
            }
            else
            {
                _logger.LogWarning("Paid transaction {TransactionId} for payment {PaymentId} (type {PaymentType}) has no matching action.", transaction.Id, payment.Id, payment.Type);
            }
        }
        else
        {
            payment.MarkAsFailed();
            _logger.LogWarning("Paymob webhook: transaction {TransactionId} for order {OrderId} was NOT successful.", transaction.Id, orderId);
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private async Task NotifyClinicOwnerAndSuperAdminsAsync(Appointment appointment, ClinicHub.Domain.Entities.Payment payment)
    {
        var amount = $"{payment.Amount:N2} EGP";

        if (appointment.Clinic?.ClinicAdminId.HasValue == true)
        {
            await _fcmService.SendToUserAsync(appointment.Clinic.ClinicAdminId.Value, NotificationType.PaymentReceived, new()
            {
                ["amount"] = amount,
                ["patientName"] = appointment.PatientFullName ?? "",
                ["clinicName"] = appointment.Clinic.Name,
                ["appointmentId"] = appointment.Id.ToString()
            });
        }

        // The current payment was only marked Paid in memory and is committed later by the
        // handler's SaveChangesAsync, so the DB sum below does not include it yet. Add it
        // manually so the notification reports the total AFTER this deposit. No double-count:
        // the idempotency guard above rejects payments already stored as Paid/Refunded.
        var totalRevenue = (await _unitOfWork.PaymentRepository
            .GetAllAsync(p => p.Status == PaymentStatus.Paid && p.Type == PaymentType.Appointment)
            .SumAsync(p => (decimal?)p.Amount) ?? 0m) + payment.Amount;

        var superAdmins = await _userManager.GetUsersInRoleAsync(UserType.SuperAdmin.ToString());
        foreach (var admin in superAdmins.Where(a => !a.IsDeleted))
        {
            await _fcmService.SendToUserAsync(admin.Id, NotificationType.RevenueIncreased, new()
            {
                ["amount"] = amount,
                ["clinicName"] = appointment.Clinic?.Name ?? "",
                ["totalRevenue"] = $"{totalRevenue:N2} EGP",
                ["appointmentId"] = appointment.Id.ToString()
            });
        }
    }

    private static Dictionary<string, string> TransactionToDictionary(PaymobTransaction transaction)
    {
        return new Dictionary<string, string>
        {
            { "amount_cents", transaction.AmountCents.ToString() },
            { "created_at", transaction.CreatedAt },
            { "currency", transaction.Currency },
            { "error_occured", transaction.ErrorOccurred.ToString().ToLower() },
            { "has_parent_transaction", transaction.HasParentTransaction.ToString().ToLower() },
            { "id", transaction.Id.ToString() },
            { "integration_id", transaction.IntegrationId.ToString() },
            { "is_3d_secure", transaction.Is3DSecure.ToString().ToLower() },
            { "is_auth", transaction.IsAuth.ToString().ToLower() },
            { "is_capture", transaction.IsCapture.ToString().ToLower() },
            { "is_refunded", transaction.IsRefunded.ToString().ToLower() },
            { "is_standalone_payment", transaction.IsStandalonePayment.ToString().ToLower() },
            { "is_voided", transaction.IsVoided.ToString().ToLower() },
            { "order.id", transaction.Order?.Id.ToString() ?? "" },
            { "owner", (transaction.Owner != 0 ? transaction.Owner : transaction.ProfileId).ToString() }, 
            { "pending", transaction.Pending.ToString().ToLower() },
            { "source_data.pan", transaction.SourceData?.Pan ?? "" },
            { "source_data.sub_type", transaction.SourceData?.SubType ?? "" },
            { "source_data.type", transaction.SourceData?.Type ?? "" },
            { "success", transaction.Success.ToString().ToLower() }
        };
    }
}
