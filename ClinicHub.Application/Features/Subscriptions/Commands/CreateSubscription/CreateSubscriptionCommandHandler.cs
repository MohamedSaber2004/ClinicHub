using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Features.Subscriptions.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Subscriptions.Commands.CreateSubscription
{
    public class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, SubscriptionDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateSubscriptionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<SubscriptionDto> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
        {
            var clinic = await _unitOfWork.ClinicRepository.FindByKeyAsync(request.ClinicId);
            if (clinic == null)
                throw new NotFoundException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

            var plan = await _unitOfWork.GetRepository<Plan, Guid>().FindByKeyAsync(request.PlanId);
            if (plan == null)
                throw new NotFoundException(LocalizationKeys.PlanMessages.NotFound.Value);

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

            var dto = _mapper.Map<SubscriptionDto>(subscription);
            dto.PlanName = plan.Name;
            return dto;
        }
    }
}
