using MediatR;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Payment.DTOs;
using ClinicHub.Domain.Enums;
using ClinicHub.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Localization;

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
        
        if (appointment.Status != AppointmentStatus.Pending)
            throw new BadRequestException(LocalizationKeys.PaymentMessages.AppointmentNotPending.Value);

        var doctor = await _unitOfWork.DoctorRepository.GetByIdAsync(appointment.DoctorId);
        var amount = doctor?.ConsultationFee ?? 0m;

        var user = await _unitOfWork.GetRepository<ApplicationUser, Guid>().GetByIdAsync(currentUserId);

        var billing = CreateBillingData(user, request.PhoneNumber);

        // Single orchestrated Paymob flow: Auth → Order → PaymentKey → WalletPay
        var walletResult = await _paymobService.InitiateWalletPaymentAsync(
            amount, "EGP", billing, request.PhoneNumber, cancellationToken);

        var payment = await _unitOfWork.PaymentRepository.GetByAppointmentIdAsync(request.AppointmentId);
        
        if (payment != null)
        {
            if (payment.Status == PaymentStatus.Paid)
            {
                throw new BadRequestException(LocalizationKeys.PaymentMessages.AlreadyPaid.Value);
            }

            // Update existing payment with new Paymob Order ID
            payment.PaymobOrderId = walletResult.OrderId;
        }
        else
        {
            payment = new ClinicHub.Domain.Entities.Payment(request.AppointmentId, currentUserId, amount)
            {
                PaymobOrderId = walletResult.OrderId
            };
            await _unitOfWork.PaymentRepository.AddAsync(payment);
        }

        await _unitOfWork.SaveChangesAsync();

        return new InitiatePaymentResponseDto
        {
            PaymentKey = walletResult.PaymentKey,
            RedirectUrl = walletResult.RedirectUrl,
            PaymentId = payment.Id
        };
    }

    private static PaymentBillingData CreateBillingData(ApplicationUser? user, string phoneNumber)
    {
        var names = user?.FullName?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? new[] { "Clinic", "User" };
        var firstName = names[0];
        var lastName = names.Length > 1 ? string.Join(" ", names.Skip(1)) : "User";

        return new PaymentBillingData
        {
            FirstName = firstName,
            LastName = lastName,
            Email = string.IsNullOrWhiteSpace(user?.Email) ? "patient@clinichub.com" : user.Email,
            PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? "01000000000" : phoneNumber,
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
