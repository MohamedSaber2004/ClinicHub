using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Payment.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Payment.Commands.InitiateBookingPayment
{
    public class InitiateBookingPaymentCommandHandler : IRequestHandler<InitiateBookingPaymentCommand, BookingPaymentResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public InitiateBookingPaymentCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
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

            var payment = new Domain.Entities.Payment(request.ReservationId, _currentUser.UserId, appointment.ClinicId, request.Amount, request.Currency);
            payment.MarkAsProcessing(paymentMethod: "cash");

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
    }
}
