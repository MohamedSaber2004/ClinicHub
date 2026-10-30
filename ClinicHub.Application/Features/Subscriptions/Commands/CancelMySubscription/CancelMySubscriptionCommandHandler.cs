using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Subscriptions.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Subscriptions.Commands.CancelMySubscription
{
    /// <summary>
    /// Thin wrapper over the shared cancellation engine. The owner can only
    /// self-cancel an online-paid subscription; the engine enforces that.
    /// </summary>
    public class CancelMySubscriptionCommandHandler : IRequestHandler<CancelMySubscriptionCommand, CancelSubscriptionResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ISubscriptionCancellationService _cancellationService;

        public CancelMySubscriptionCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ISubscriptionCancellationService cancellationService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _cancellationService = cancellationService;
        }

        public async Task<CancelSubscriptionResponseDto> Handle(CancelMySubscriptionCommand request, CancellationToken cancellationToken)
        {
            var clinicId = _currentUserService.CurrentClinicId;
            if (clinicId == null)
                throw new BadRequestException(LocalizationKeys.ExceptionMessages.BadRequest.Value);

            var subscriptionId = await _unitOfWork.GetRepository<Subscription, Guid>()
                .GetAllAsync(s => s.ClinicId == clinicId && s.Status == SubscriptionStatus.Active)
                .AsNoTracking()
                .Select(s => (Guid?)s.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (subscriptionId == null)
                throw new NotFoundException(LocalizationKeys.SubscriptionMessages.NoActiveSubscription.Value);

            return await _cancellationService.CancelAsync(
                subscriptionId.Value,
                clinicId,
                SubscriptionCancellationSource.ClinicOwner,
                _currentUserService.UserId,
                refundAmountOverride: null,
                request.Reason,
                requireOnlinePayment: true,
                cancellationToken);
        }
    }
}
