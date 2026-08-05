using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Payment.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Payment.Commands.InitiateBookingPayment
{
    public class InitiateBookingPaymentCommandHandler : IRequestHandler<InitiateBookingPaymentCommand, BookingPaymentResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IPaymobService _paymobService;

        public InitiateBookingPaymentCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IPaymobService paymobService)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _paymobService = paymobService;
        }

        public async Task<BookingPaymentResponseDto> Handle(InitiateBookingPaymentCommand request, CancellationToken cancellationToken)
        {
            var appointment = await _unitOfWork.AppointmentRepository.GetByIdAsync(request.ReservationId);

            if (appointment == null)
                throw new NotFoundException(LocalizationKeys.BookingMessages.ReservationNotFound.Value);

            if (appointment.BookedByUserId != _currentUser.UserId)
                throw new UnauthorizedAccessException(LocalizationKeys.PaymentMessages.Unauthorized.Value);

            if (appointment.IsReservationExpired())
                throw new ConflictException(LocalizationKeys.BookingMessages.ReservationExpired.Value);

            if (appointment.Status != AppointmentStatus.Reserved)
                throw new BadRequestException(LocalizationKeys.PaymentMessages.AppointmentNotPending.Value);

            var existingPayment = await _unitOfWork.PaymentRepository.GetByAppointmentIdAsync(request.ReservationId);
            if (existingPayment != null && existingPayment.Status == PaymentStatus.Paid)
                throw new BadRequestException(LocalizationKeys.PaymentMessages.AlreadyPaid.Value);

            var bookingConfig = await _unitOfWork.BookingConfigurationRepository.GetByClinicIdAsync(appointment.ClinicId);
            if (bookingConfig == null)
                throw new BadRequestException(LocalizationKeys.BookingMessages.BookingConfigNotFound.Value);

            var amount = bookingConfig.ConsultationFee;
            var currency = string.IsNullOrWhiteSpace(bookingConfig.Currency) ? "EGP" : bookingConfig.Currency;

            var patientUser = await _unitOfWork.GetRepository<ApplicationUser, Guid>().GetByIdAsync(_currentUser.UserId);
            var billing = CreateBillingData(patientUser);

            // Initiate the real Paymob hosted checkout first — if it fails nothing is persisted.
            var checkout = await _paymobService.InitiateCheckoutPaymentAsync(amount, currency, billing, cancellationToken);

            var payment = new Domain.Entities.Payment(PaymentType.Appointment, _currentUser.UserId, appointment.ClinicId, amount, currency)
            {
                PaymobOrderId = checkout.OrderId
            };
            payment.LinkToAppointment(request.ReservationId);
            payment.SetPaymobCheckout(checkout.RedirectUrl);

            await _unitOfWork.PaymentRepository.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            return new BookingPaymentResponseDto
            {
                PaymentId = payment.Id,
                ReservationId = appointment.Id,
                Amount = payment.Amount,
                Currency = payment.Currency,
                Status = payment.Status,
                TransactionId = payment.TransactionId,
                RedirectUrl = payment.RedirectUrl,
                FailureReason = payment.FailureReason,
                CreatedAt = payment.CreatedAt
            };
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
}
