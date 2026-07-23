using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Subscriptions.Commands.CancelMySubscription
{
    public class CancelMySubscriptionCommandHandler : IRequestHandler<CancelMySubscriptionCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CancelMySubscriptionCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(CancelMySubscriptionCommand request, CancellationToken cancellationToken)
        {
            var clinicId = _currentUserService.CurrentClinicId;
            if (clinicId == null)
                throw new BadRequestException(LocalizationKeys.ExceptionMessages.BadRequest.Value);

            var subscription = await _unitOfWork.GetRepository<Subscription, Guid>()
                .GetFirstAsync(s => s.ClinicId == clinicId && s.Status == SubscriptionStatus.Active, cancellationToken);

            if (subscription == null)
                throw new NotFoundException("No active subscription found.");

            subscription.Status = SubscriptionStatus.Cancelled;
            _unitOfWork.GetRepository<Subscription, Guid>().Update(subscription);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
