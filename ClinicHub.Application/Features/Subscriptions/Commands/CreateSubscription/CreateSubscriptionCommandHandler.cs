using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Subscriptions.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Subscriptions.Commands.CreateSubscription
{
    public class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, SubscriptionDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IBackgroundJobScheduler _jobScheduler;

        public CreateSubscriptionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IBackgroundJobScheduler jobScheduler)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _jobScheduler = jobScheduler;
        }

        public async Task<SubscriptionDto> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
        {
            var clinic = await _unitOfWork.ClinicRepository.FindByKeyAsync(request.ClinicId);
            if (clinic == null)
                throw new NotFoundException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

            var plan = await _unitOfWork.GetRepository<Plan, Guid>().FindByKeyAsync(request.PlanId);
            if (plan == null)
                throw new NotFoundException(LocalizationKeys.PlanMessages.NotFound.Value);

            var existingActiveSubs = await _unitOfWork.GetRepository<Subscription, Guid>()
                .GetAllAsync(s => s.ClinicId == request.ClinicId && s.Status == SubscriptionStatus.Active)
                .ToListAsync(cancellationToken);

            foreach (var activeSub in existingActiveSubs)
            {
                activeSub.Status = SubscriptionStatus.Revoked;
                activeSub.Notes = "Revoked due to new manual subscription creation.";
            }

            var subscription = new Subscription
            {
                ClinicId = request.ClinicId,
                PlanId = request.PlanId,
                Period = request.Period,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Amount = request.Amount,
                Status = SubscriptionStatus.Active,
                PaidAt = DateTime.UtcNow,
                PaymentId = request.PaymentId,
                Notes = request.Notes
            };

            await _unitOfWork.GetRepository<Subscription, Guid>().AddAsync(subscription);

            if (request.PaymentId.HasValue)
            {
                var payment = await _unitOfWork.PaymentRepository.GetByIdAsync(request.PaymentId.Value);
                if (payment != null)
                {
                    payment.LinkToSubscription(subscription.Id);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            await _jobScheduler.ScheduleSubscriptionExpirationAsync(subscription.Id, subscription.EndDate);

            var dto = _mapper.Map<SubscriptionDto>(subscription);
            dto.PlanName = plan.Name;
            return dto;
        }
    }
}
