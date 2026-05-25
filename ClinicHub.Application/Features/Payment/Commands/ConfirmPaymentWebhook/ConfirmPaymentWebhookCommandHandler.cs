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

        var isValid = await _paymobService.ValidateWebhookAsync(request.Hmac, request.TransactionData);
        if (!isValid)
            return false;

        // Paymob can send order ID under different keys depending on the source (Callback vs Webhook)
        string? orderId = GetFirstValue(request.TransactionData, "order", "order_id", "order.id");
        
        if (string.IsNullOrEmpty(orderId))
            return false;

        var payment = await _unitOfWork.PaymentRepository.GetByPaymobOrderIdAsync(orderId);
        if (payment == null)
            return false;

        // Idempotency: skip if already processed
        if (payment.Status != PaymentStatus.Pending)
            return true;

        request.TransactionData.TryGetValue("success", out var successStr);
        request.TransactionData.TryGetValue("id", out var transactionId);
        request.TransactionData.TryGetValue("source_data_sub_type", out var method);
        request.TransactionData.TryGetValue("error_occured", out var errorOccured);

        bool isSuccess = successStr?.ToLower() == "true";

        if (isSuccess)
        {
            payment.MarkAsPaid(transactionId ?? "N/A", method ?? "Unknown");

            var appointment = await _unitOfWork.AppointmentRepository.GetByIdAsync(payment.AppointmentId);
            appointment?.Confirm();
        }
        else
        {
            _logger.LogWarning("Payment failed for Payment {PaymentId}, Order {PaymobOrder}. Success: {Success}, Error Occurred: {Error}", 
                payment.Id, payment.PaymobOrderId, successStr, errorOccured);
            payment.MarkAsFailed();
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static string? GetFirstValue(IDictionary<string, string> data, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (data.TryGetValue(key, out var value))
                return value;
        }
        return null;
    }
}
