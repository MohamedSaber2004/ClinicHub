using ClinicHub.Application.Common;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Subscriptions.Commands.RevokeSubscription
{
    public class RevokeSubscriptionCommandHandler : IRequestHandler<RevokeSubscriptionCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymobService _paymobService;

        public RevokeSubscriptionCommandHandler(
            IUnitOfWork unitOfWork,
            IPaymobService paymobService)
        {
            _unitOfWork = unitOfWork;
            _paymobService = paymobService;
        }

        public async Task<bool> Handle(RevokeSubscriptionCommand request, CancellationToken cancellationToken)
        {
            var subscription = await _unitOfWork.GetRepository<Subscription, Guid>().GetByIdAsync(request.SubscriptionId);

            if (subscription.Status != SubscriptionStatus.Active)
                throw new BadRequestException("Subscription is not active.");

            subscription.Status = SubscriptionStatus.Revoked;

            if (subscription.PaymentId.HasValue)
            {
                var paymentId = subscription.PaymentId.Value;

                await PaymentRefundGate.RunAsync(paymentId, async () =>
                {
                    // Another concurrent request may have refunded this payment already.
                    var alreadyRefunded = await _unitOfWork.PaymentRepository.GetAllAsync(p => p.Id == paymentId)
                        .AsNoTracking()
                        .AnyAsync(p => p.Status == PaymentStatus.Refunded, cancellationToken);
                    if (alreadyRefunded)
                        return;

                    var payment = await _unitOfWork.PaymentRepository.GetByIdAsync(paymentId);
                    if (payment != null && !string.IsNullOrEmpty(payment.PaymobTransactionId))
                    {
                        var refund = await _paymobService.RefundTransactionAsync(payment.PaymobTransactionId, payment.Amount, cancellationToken);
                        if (refund.Success)
                            payment.MarkAsRefunded(refund.RefundId);
                        else
                            payment.MarkAsFailed(refund.Message ?? "Refund failed");
                    }
                });
            }

            _unitOfWork.GetRepository<Subscription, Guid>().Update(subscription);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
