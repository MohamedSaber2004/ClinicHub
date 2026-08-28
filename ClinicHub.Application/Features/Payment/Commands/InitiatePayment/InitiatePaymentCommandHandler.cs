using ClinicHub.Application.Common;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.AdminPayments;
using ClinicHub.Application.Features.Payment.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Payment.Commands.InitiatePayment;

public class InitiatePaymentCommandHandler : IRequestHandler<InitiatePaymentCommand, InitiatePaymentResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymobService _paymobService;
    private readonly ICurrentUserService _currentUser;

    public InitiatePaymentCommandHandler(
        IUnitOfWork unitOfWork, 
        IPaymobService paymobService, 
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _paymobService = paymobService;
        _currentUser = currentUser;
    }

    public async Task<InitiatePaymentResponseDto> Handle(InitiatePaymentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _unitOfWork.AppointmentRepository.GetByIdAsync(request.AppointmentId);
        
        if (appointment == null)
            throw new NotFoundException(LocalizationKeys.PaymentMessages.AppointmentNotFound.Value);
        
        var currentUserId = _currentUser.UserId;

        if (appointment.BookedByUserId != currentUserId)
            throw new UnauthorizedAccessException(LocalizationKeys.PaymentMessages.Unauthorized.Value);
        
        if (appointment.Status != AppointmentStatus.Pending && appointment.Status != AppointmentStatus.Reserved)
            throw new BadRequestException(LocalizationKeys.PaymentMessages.AppointmentNotPending.Value);

        var doctor = await _unitOfWork.DoctorRepository.GetByIdAsync(appointment.DoctorId);
        var bookingConfig = await _unitOfWork.BookingConfigurationRepository.GetByClinicIdAsync(appointment.ClinicId);

        if (bookingConfig == null)
            throw new BadRequestException(LocalizationKeys.BookingMessages.BookingConfigNotFound.Value);

        var platformFeePercent = await GetPlatformFeePercentAsync(cancellationToken);

        // Patient pays the clinic fee plus the platform fee percentage on top.
        var amount = AppointmentPricingCalculator.CalculateTotal(bookingConfig.ConsultationFee, platformFeePercent);
        var user = await _unitOfWork.GetRepository<ApplicationUser, Guid>().GetByIdAsync(currentUserId);

        var billing = CreateBillingData(user);

        // Appointment now supports both Paymob wallet and card, same as subscriptions/ads.
        // PaymentMethod: "wallet"/null => PaymobWallet (Vodafone Cash etc.), "card"/"creditcard"/"credit_card" => PaymobCreditCard.
        // Null stays wallet for backward compat (old mobile clients omitted the field).
        var resolvedMethod = PaymentMethodMapper.ToEnum(request.PaymentMethod);
        WalletPaymentResultDto payResult;
        if (resolvedMethod == PaymentMethod.PaymobCreditCard)
            payResult = await _paymobService.InitiateCheckoutPaymentAsync(amount, "EGP", billing, cancellationToken, request.ReturnUrl);
        else
            payResult = await _paymobService.InitiateWalletPaymentAsync(amount, "EGP", billing, billing.PhoneNumber, cancellationToken, request.ReturnUrl);

        var payment = await _unitOfWork.PaymentRepository.GetByAppointmentIdAsync(request.AppointmentId);
        
        if (payment != null)
        {
            if (payment.Status == PaymentStatus.Paid)
            {
                throw new BadRequestException(LocalizationKeys.PaymentMessages.AlreadyPaid.Value);
            }

            // Update existing payment with new Paymob Order ID + refresh redirect/method
            payment.PaymobOrderId = payResult.OrderId;
            payment.MarkAsProcessing(payResult.RedirectUrl, PaymentMethodMapper.ToDbString(resolvedMethod));
        }
        else
        {
            payment = new ClinicHub.Domain.Entities.Payment(PaymentType.Appointment, currentUserId, appointment.ClinicId, amount)
            {
                PaymobOrderId = payResult.OrderId
            };
            payment.LinkToAppointment(request.AppointmentId);
            payment.MarkAsProcessing(payResult.RedirectUrl, PaymentMethodMapper.ToDbString(resolvedMethod));
            await _unitOfWork.PaymentRepository.AddAsync(payment);
        }

        await _unitOfWork.SaveChangesAsync();

        return new InitiatePaymentResponseDto
        {
            PaymentKey = payResult.PaymentKey,
            RedirectUrl = payResult.RedirectUrl,
            PaymentId = payment.Id
        };
    }

    private async Task<decimal> GetPlatformFeePercentAsync(CancellationToken cancellationToken)
    {
        var setting = await _unitOfWork.GetRepository<PlatformSetting, Guid>()
            .GetAllAsync(s => !s.IsDeleted)
            .OrderBy(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return setting?.AppointmentFeePercent ?? 0m;
    }

    private static PaymentBillingData CreateBillingData(ApplicationUser? user)
    {
        var names = user?.FullName?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? new[] { "Clinic", "User" };
        var firstName = names[0];
        var lastName = names.Length > 1 ? string.Join(" ", names.Skip(1)) : "User";

        return new PaymentBillingData
        {
            FirstName = firstName,
            LastName = lastName,
            Email = string.IsNullOrWhiteSpace(user?.Email) ? "patient@clinichub.com" : user.Email,
            PhoneNumber = string.IsNullOrWhiteSpace(user?.PhoneNumber) ? "01000000000" : user.PhoneNumber,
            City = "Cairo",
            Country = "EG",
            Street = "NA",
            Building = "NA",
            Apartment = "NA",
            Floor = "NA",
            PostalCode = "NA",
            State = "Cairo"
        };
    }
}
