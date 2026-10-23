using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Subscriptions.DTOs;
using ClinicHub.Application.Features.Subscriptions.Services;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Subscriptions.Queries.GetMyCancellationQuote
{
    public class GetMyCancellationQuoteQueryHandler : IRequestHandler<GetMyCancellationQuoteQuery, SubscriptionCancellationQuoteDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetMyCancellationQuoteQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<SubscriptionCancellationQuoteDto> Handle(GetMyCancellationQuoteQuery request, CancellationToken cancellationToken)
        {
            var clinicId = _currentUserService.CurrentClinicId;
            if (clinicId == null)
                return Ineligible(null, LocalizationKeys.ExceptionMessages.BadRequest.Value);

            var subscription = await _unitOfWork.GetRepository<Subscription, Guid>()
                .GetAllWithIncluding(
                    s => s.ClinicId == clinicId && s.Status == SubscriptionStatus.Active,
                    s => s.Payment,
                    s => s.Plan)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (subscription == null)
                return Ineligible(null, LocalizationKeys.SubscriptionMessages.NoActiveSubscription.Value);

            if (subscription.EndDate <= DateTime.Now)
                return Ineligible(subscription, LocalizationKeys.SubscriptionMessages.ExpiredCannotCancel.Value);

            var payment = subscription.Payment;
            if (payment == null)
                return Ineligible(subscription, LocalizationKeys.SubscriptionMessages.NoPaymentLinked.Value);

            if (payment.Status is not (PaymentStatus.Paid or PaymentStatus.PartiallyRefunded))
                return Ineligible(subscription, LocalizationKeys.SubscriptionMessages.NoPaymentLinked.Value);

            // Online check: only Paymob transactions carry a numeric PaymobTransactionId.
            if (!long.TryParse(payment.PaymobTransactionId, out _))
                return Ineligible(subscription, LocalizationKeys.SubscriptionMessages.OnlyOnlineCanCancel.Value);

            var quote = SubscriptionRefundCalculator.Calculate(
                payment.Amount,
                payment.RefundedAmount,
                subscription.StartDate,
                subscription.EndDate,
                DateTime.Now);

            return new SubscriptionCancellationQuoteDto
            {
                SubscriptionId = subscription.Id,
                Eligible = true,
                Reason = null,
                PlanName = subscription.Plan?.Name,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                TotalDays = quote.TotalDays,
                UsedDays = quote.UsedDays,
                AmountPaid = payment.Amount,
                ChargeAmount = quote.ChargeAmount,
                RefundAmount = quote.RefundAmount,
                Currency = payment.Currency,
                RefundStatus = payment.RefundStatus
            };
        }

        private static SubscriptionCancellationQuoteDto Ineligible(Subscription? subscription, string reasonKey) => new()
        {
            SubscriptionId = subscription?.Id ?? Guid.Empty,
            Eligible = false,
            Reason = reasonKey,
            PlanName = subscription?.Plan?.Name,
            StartDate = subscription?.StartDate ?? default,
            EndDate = subscription?.EndDate ?? default,
            RefundStatus = PaymentRefundStatus.None
        };
    }
}
