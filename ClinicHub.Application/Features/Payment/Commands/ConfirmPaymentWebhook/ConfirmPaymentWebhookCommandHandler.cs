using MediatR;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Domain.Enums;
using System.Threading;
using System.Threading.Tasks;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Application.Features.Payment.Commands.ConfirmPaymentWebhook;

public class ConfirmPaymentWebhookCommandHandler : IRequestHandler<ConfirmPaymentWebhookCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymobService _paymobService;
    private readonly ILogger<ConfirmPaymentWebhookCommandHandler> _logger;

    public ConfirmPaymentWebhookCommandHandler(IUnitOfWork unitOfWork, IPaymobService paymobService, ILogger<ConfirmPaymentWebhookCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _paymobService = paymobService;
        _logger = logger;
    }

    public async Task<bool> Handle(ConfirmPaymentWebhookCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Hmac))
            return false;

        var transaction = request.Transaction;
        if (transaction?.Order?.Id == 0)
        {
            _logger.LogWarning("Webhook validation failed: Order ID is missing");
            return false;
        }

        // Convert transaction object to dictionary for HMAC validation
        var transactionData = TransactionToDictionary(transaction);

        var isValid = await _paymobService.ValidateWebhookAsync(request.Hmac, transactionData);
        if (!isValid)
        {
            _logger.LogWarning("Paymob HMAC validation failed for Order {OrderId}", transaction.Order.Id);
            return false;
        }

        var orderId = transaction.Order.Id.ToString();
        var payment = await _unitOfWork.PaymentRepository.GetByPaymobOrderIdAsync(orderId);
        if (payment == null)
        {
            _logger.LogWarning("Payment not found for Paymob Order {OrderId}", orderId);
            return false;
        }

        // Idempotency: skip if already processed
        if (payment.Status != PaymentStatus.Pending)
        {
            _logger.LogInformation("Payment {PaymentId} already processed with status {Status}", payment.Id, payment.Status);
            return true;
        }

        if (transaction.Success)
        {
            payment.MarkAsPaid(transaction.Id.ToString(), transaction.SourceData?.SubType ?? "Unknown");

            var appointment = await _unitOfWork.AppointmentRepository.GetByIdAsync(payment.AppointmentId);
            appointment?.Confirm();

            _logger.LogInformation("Payment {PaymentId} marked as paid. Transaction: {TransactionId}", payment.Id, transaction.Id);
        }
        else
        {
            _logger.LogWarning("Payment failed for Payment {PaymentId}, Order {PaymobOrder}. Success: {Success}, Error Occurred: {Error}", 
                payment.Id, payment.PaymobOrderId, transaction.Success, transaction.ErrorOccurred);
            payment.MarkAsFailed();
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
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
            { "owner", transaction.Id.ToString() }, // Paymob uses transaction ID as owner in some contexts
            { "pending", transaction.Pending.ToString().ToLower() },
            { "source_data.pan", transaction.SourceData?.Pan ?? "" },
            { "source_data.sub_type", transaction.SourceData?.SubType ?? "" },
            { "source_data.type", transaction.SourceData?.Type ?? "" },
            { "success", transaction.Success.ToString().ToLower() }
        };
    }
}
