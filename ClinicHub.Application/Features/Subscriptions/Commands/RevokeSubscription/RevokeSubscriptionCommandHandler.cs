using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Subscriptions.Commands.RevokeSubscription
{
    public class RevokeSubscriptionCommandHandler : IRequestHandler<RevokeSubscriptionCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RevokeSubscriptionCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(RevokeSubscriptionCommand request, CancellationToken cancellationToken)
        {
            var subscription = await _unitOfWork.GetRepository<Subscription, Guid>().GetByIdAsync(request.SubscriptionId);

            if (subscription.Status != SubscriptionStatus.Active)
                throw new BadRequestException("Subscription is not active.");

            subscription.Status = SubscriptionStatus.Revoked;
            _unitOfWork.GetRepository<Subscription, Guid>().Update(subscription);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
