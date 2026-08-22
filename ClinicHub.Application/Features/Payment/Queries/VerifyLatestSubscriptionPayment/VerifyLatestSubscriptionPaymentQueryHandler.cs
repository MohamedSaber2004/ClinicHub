using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Payment.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Payment.Queries.VerifyLatestSubscriptionPayment;

public class VerifyLatestSubscriptionPaymentQueryHandler
    : IRequestHandler<VerifyLatestSubscriptionPaymentQuery, VerifySubscriptionPaymentResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IPaymobService _paymobService;
    private readonly ISubscriptionPaymentCompleter _subscriptionPaymentCompleter;

    public VerifyLatestSubscriptionPaymentQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IPaymobService paymobService,
        ISubscriptionPaymentCompleter subscriptionPaymentCompleter)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _paymobService = paymobService;
        _subscriptionPaymentCompleter = subscriptionPaymentCompleter;
    }

    public async Task<VerifySubscriptionPaymentResponseDto> Handle(VerifyLatestSubscriptionPaymentQuery request, CancellationToken cancellationToken)
    {
        var clinicId = _currentUser.CurrentClinicId;
        if (!clinicId.HasValue)
            throw new ForbiddenException("Clinic not found.");

        var response = new VerifySubscriptionPaymentResponseDto();

        // Report the clinic's current active subscription (if any) regardless of payment state.
        var activeSub = await _unitOfWork.GetRepository<Subscription, Guid>()
            .GetAllAsync(s => s.ClinicId == clinicId.Value && s.Status == SubscriptionStatus.Active && s.EndDate > DateTime.Now)
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (activeSub != null)
        {
            response.SubscriptionActive = true;
            response.EndDate = activeSub.EndDate;

            if (activeSub.PlanId.HasValue)
            {
                var plan = await _unitOfWork.GetRepository<Plan, Guid>().FindByKeyAsync(activeSub.PlanId.Value);
                response.PlanName = plan?.Name;
            }
        }

        // Latest subscription-type payment of this clinic (last 3 days, covers a full checkout session).
        var since = DateTime.Now.AddDays(-3);
        var payment = await _unitOfWork.GetRepository<Domain.Entities.Payment, Guid>()
            .GetAllAsync(p => p.ClinicId == clinicId.Value
                           && p.Type == PaymentType.Subscription
                           && p.CreatedAt >= since)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (payment == null)
        {
            response.Status = "none";
            return response;
        }

        switch (payment.Status)
        {
            case PaymentStatus.Paid:
            case PaymentStatus.Refunded:
                response.Status = "paid";
                return response;

            case PaymentStatus.Failed:
                response.Status = "failed";
                return response;
        }

        // Payment is still Pending/Processing — ask Paymob directly as the webhook may be delayed.
        var orderStatus = await _paymobService.GetOrderPaymentStatusAsync(payment.PaymobOrderId ?? "", cancellationToken);
        if (!orderStatus.Found || !orderStatus.Paid)
        {
            response.Status = "pending";
            return response;
        }

        // Paymob confirms money was captured — mark paid and activate idempotently.
        payment.MarkAsPaid($"inquiry-{payment.PaymobOrderId}", "paymob");
        await _subscriptionPaymentCompleter.ActivateFromPaymentAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync();

        response.Status = "paid";
        response.SubscriptionActive = true;
        response.EndDate = DateTime.Now.AddMonths(payment.SubscriptionPeriod == SubscriptionPlan.Yearly ? 12 : 1);

        if (payment.PlanId.HasValue)
        {
            var plan = await _unitOfWork.GetRepository<Plan, Guid>().FindByKeyAsync(payment.PlanId.Value);
            response.PlanName = plan?.Name;
        }

        return response;
    }
}
