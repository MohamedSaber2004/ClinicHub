using ClinicHub.Domain.Common;
using ClinicHub.Domain.Common.Interfaces;
using ClinicHub.Domain.Enums;

namespace ClinicHub.Domain.Entities;

public class Payment : BaseEntity<Guid>, IClinicScopedEntity
{
    public PaymentType Type { get; private set; } = PaymentType.Appointment;
    public string? Code { get; private set; }
    public string? RefNumber { get; private set; }
    public Guid? AppointmentId { get; private set; }
    public Appointment? Appointment { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ClinicId { get; private set; }
    Guid? IClinicScopedEntity.ClinicId => ClinicId;
    public Clinic Clinic { get; private set; } = null!;
    public Guid? SubscriptionId { get; private set; }
    public Subscription? Subscription { get; private set; }
    public Guid? PlanId { get; set; }
    public SubscriptionPlan? SubscriptionPeriod { get; set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "EGP";
    public string? PaymobOrderId { get; set; }
    public string? PaymobTransactionId { get; private set; }
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public string? PaymentMethod { get; private set; }
    public DateTime? PaidAt { get; private set; }

    public string? RedirectUrl { get; private set; }
    public string? FailureReason { get; private set; }
    public string? TransactionId { get; private set; }
    public DateTime? RefundedAt { get; private set; }
    public string? RefundReason { get; private set; }
    public string? Notes { get; private set; }

    private Payment() { }

    public Payment(PaymentType type, Guid userId, Guid clinicId, decimal amount, string currency = "EGP")
    {
        Type = type;
        UserId = userId;
        ClinicId = clinicId;
        Amount = amount;
        Currency = currency ?? "EGP";
        Code = "#P-" + Guid.NewGuid().ToString("N")[..6].ToUpper();
        RefNumber = $"PM-{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
    }

    public void LinkToSubscription(Guid subscriptionId)
    {
        SubscriptionId = subscriptionId;
    }

    public void LinkToAppointment(Guid appointmentId)
    {
        AppointmentId = appointmentId;
    }

    public void SetManualReference(string? refNumber, string? notes)
    {
        RefNumber = string.IsNullOrWhiteSpace(refNumber) ? RefNumber : refNumber;
        Notes = notes;
    }

        public void MarkAsProcessing(string? redirectUrl = null, string? paymentMethod = null)
        {
            RedirectUrl = redirectUrl;
            PaymentMethod = paymentMethod;
            Status = PaymentStatus.Processing;
        }

    public void MarkAsPaid(string transactionId, string method)
    {
        PaymobTransactionId = transactionId;
        TransactionId = transactionId;
        PaymentMethod = method;
        PaidAt = DateTime.UtcNow;
        Status = PaymentStatus.Paid;
        FailureReason = null;
    }

    public void MarkAsManuallyPaid(string method, string? reference = null)
    {
        TransactionId = reference;
        PaymentMethod = method;
        PaidAt = DateTime.UtcNow;
        Status = PaymentStatus.Paid;
        FailureReason = null;
    }

    public void MarkAsFailed(string? failureReason = null)
    {
        Status = PaymentStatus.Failed;
        FailureReason = failureReason;
    }

    public void MarkAsRefunded(string? reason = null)
    {
        Status = PaymentStatus.Refunded;
        RefundedAt = DateTime.UtcNow;
        RefundReason = reason;
        PaidAt = null;
        PaymentMethod = null;
    }
}