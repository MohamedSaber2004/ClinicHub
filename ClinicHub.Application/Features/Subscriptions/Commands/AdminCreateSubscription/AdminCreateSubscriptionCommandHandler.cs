using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.AdminPayments;
using ClinicHub.Application.Features.Subscriptions.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Subscriptions.Commands.AdminCreateSubscription
{
    public class AdminCreateSubscriptionCommandHandler : IRequestHandler<AdminCreateSubscriptionCommand, SubscriptionDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IBackgroundJobScheduler _jobScheduler;

        public AdminCreateSubscriptionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IBackgroundJobScheduler jobScheduler)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _jobScheduler = jobScheduler;
        }

        public async Task<SubscriptionDto> Handle(AdminCreateSubscriptionCommand request, CancellationToken cancellationToken)
        {
            var clinic = await _unitOfWork.ClinicRepository.FindByKeyAsync(request.ClinicId, cancellationToken);
            if (clinic == null)
                throw new NotFoundException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

            var plan = await _unitOfWork.GetRepository<Plan, Guid>().FindByKeyAsync(request.PlanId, cancellationToken);
            if (plan == null)
                throw new NotFoundException(LocalizationKeys.PlanMessages.NotFound.Value);

            if (!plan.IsActive)
                throw new NotFoundException(LocalizationKeys.PlanMessages.NotActive.Value);

            var startDate = request.StartDate?.Date ?? DateTime.UtcNow.Date;
            var amount = request.Amount ?? (request.Period == SubscriptionPlan.Yearly ? plan.PriceYearly : plan.PriceMonthly);
            var now = DateTime.UtcNow;

            var activeSubs = await _unitOfWork.GetRepository<Subscription, Guid>()
                .GetAllAsync(s => s.ClinicId == request.ClinicId && s.Status == SubscriptionStatus.Active)
                .ToListAsync(cancellationToken);

            var validActive = activeSubs.FirstOrDefault(s => s.EndDate > now);
            if (validActive != null)
                throw new BadRequestException(LocalizationKeys.SubscriptionMessages.AlreadyActive.Value);

            foreach (var staleSub in activeSubs)
            {
                staleSub.Status = SubscriptionStatus.Expired;
            }

            var subscription = new Subscription
            {
                ClinicId = request.ClinicId,
                PlanId = plan.Id,
                Period = request.Period,
                StartDate = startDate,
                EndDate = request.Period == SubscriptionPlan.Yearly
                    ? startDate.AddYears(1)
                    : startDate.AddMonths(1),
                Amount = amount,
                Status = SubscriptionStatus.Active,
                PaidAt = now
            };

            await _unitOfWork.GetRepository<Subscription, Guid>().AddAsync(subscription);

            var payment = await CreatePaymentRecordAsync(clinic, plan, amount, request.Period, cancellationToken);
            if (payment != null)
            {
                subscription.PaymentId = payment.Id;
                payment.LinkToSubscription(subscription.Id);
            }

            await _unitOfWork.SaveChangesAsync();

            await _jobScheduler.ScheduleSubscriptionExpirationAsync(subscription.Id, subscription.EndDate);

            var saved = await _unitOfWork.GetRepository<Subscription, Guid>()
                .GetAllAsync(s => s.Id == subscription.Id)
                .Include(s => s.Clinic)
                .Include(s => s.Plan)
                    .ThenInclude(p => p!.Permissions)
                .FirstOrDefaultAsync(cancellationToken);

            return _mapper.Map<SubscriptionDto>(saved);
        }

        private async Task<ClinicHub.Domain.Entities.Payment?> CreatePaymentRecordAsync(Clinic clinic, Plan plan, decimal amount, SubscriptionPlan period, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.GetRepository<ApplicationUser, Guid>()
                .GetAllAsync(u => u.ClinicId == clinic.Id && !u.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            var userId = user?.Id ?? clinic.ClinicAdminId;
            if (!userId.HasValue)
                return null;

            var payment = new ClinicHub.Domain.Entities.Payment(PaymentType.Subscription, userId.Value, clinic.Id, amount)
            {
                PlanId = plan.Id,
                SubscriptionPeriod = period
            };
            payment.SetManualReference(null, "Granted by superadmin.");
            payment.MarkAsManuallyPaid(PaymentMethodMapper.ToDbString(PaymentMethod.Cash));

            await _unitOfWork.PaymentRepository.AddAsync(payment);
            return payment;
        }
    }
}
