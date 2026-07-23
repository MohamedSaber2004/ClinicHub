using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Payment.DTOs;
using ClinicHub.Application.Features.Subscriptions.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Subscriptions.Commands.InitiateSubscriptionPayment
{
    public class InitiateSubscriptionPaymentCommandHandler : IRequestHandler<InitiateSubscriptionPaymentCommand, InitiateSubscriptionPaymentResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IPaymobService _paymobService;
        private readonly IStringLocalizer<Messages> _localizer;

        public InitiateSubscriptionPaymentCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IPaymobService paymobService,
            IStringLocalizer<Messages> localizer)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _paymobService = paymobService;
            _localizer = localizer;
        }

        public async Task<InitiateSubscriptionPaymentResponseDto> Handle(InitiateSubscriptionPaymentCommand request, CancellationToken cancellationToken)
        {
            var plan = await _unitOfWork.GetRepository<Plan, Guid>().FindByKeyAsync(request.PlanId);
            if (plan == null)
                throw new NotFoundException(LocalizationKeys.PlanMessages.NotFound.Value);

            if (!plan.IsActive)
                throw new BadRequestException(_localizer[LocalizationKeys.PlanMessages.NotActive.Value]);

            var clinicId = _currentUser.CurrentClinicId;
            if (!clinicId.HasValue)
                throw new ForbiddenException("Clinic not found.");

            var clinic = await _unitOfWork.ClinicRepository.FindByKeyAsync(clinicId.Value);
            if (clinic == null || clinic.Status != ClinicStatus.Active)
                throw new BadRequestException("Clinic must be active to subscribe.");


            var user = await _unitOfWork.GetRepository<ApplicationUser, Guid>().FindByKeyAsync(_currentUser.UserId);
            var names = user?.FullName?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? new[] { clinic.Name ?? "Clinic", "Owner" };
            var firstName = names[0];
            var lastName = names.Length > 1 ? string.Join(" ", names.Skip(1)) : "Owner";

            var amount = request.Period == SubscriptionPlan.Yearly ? plan.PriceYearly : plan.PriceMonthly;
            var currency = "EGP";

            var billing = new PaymentBillingData
            {
                FirstName = firstName,
                LastName = lastName,
                Email = string.IsNullOrWhiteSpace(user?.Email) ? clinic.Email ?? "clinic@clinichub.com" : user.Email,
                PhoneNumber = string.IsNullOrWhiteSpace(clinic.Phone) ? "01000000000" : clinic.Phone,
                City = "Egypt",
                Country = "EG",
                Street = clinic.Address ?? clinic.Name,
                Building = "NA",
                Apartment = "NA",
                Floor = "NA",
                PostalCode = "NA",
                State = "Egypt"
            };

            var walletResult = await _paymobService.InitiateWalletPaymentAsync(amount, currency, billing, billing.PhoneNumber, cancellationToken, request.ReturnUrl);

            var payment = new Domain.Entities.Payment(null, _currentUser.UserId, clinicId.Value, amount, currency)
            {
                PaymobOrderId = walletResult.OrderId,
                PlanId = request.PlanId,
                SubscriptionPeriod = request.Period
            };
            payment.MarkAsProcessing(walletResult.RedirectUrl, "paymob");

            await _unitOfWork.PaymentRepository.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            return new InitiateSubscriptionPaymentResponseDto
            {
                PaymentId = payment.Id,
                PaymobRedirectUrl = walletResult.RedirectUrl,
                PaymobPaymentKey = walletResult.PaymentKey,
                PlanId = plan.Id,
                PlanName = plan.Name,
                Period = request.Period,
                Amount = amount,
                Currency = currency
            };
        }
    }
}
