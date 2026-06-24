using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Features.Payment.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Payment.Commands.VerifyBookingPayment
{
    public class VerifyBookingPaymentCommandHandler : IRequestHandler<VerifyBookingPaymentCommand, BookingPaymentResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public VerifyBookingPaymentCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BookingPaymentResponseDto> Handle(VerifyBookingPaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _unitOfWork.PaymentRepository.GetByIdAsync(request.PaymentId);

            if (payment == null)
                throw new NotFoundException(LocalizationKeys.PaymentMessages.NotFound.Value);

            if (payment.Status == PaymentStatus.Paid)
            {
                var apt = await _unitOfWork.AppointmentRepository.GetByIdAsync(payment.AppointmentId);
                return BuildResponse(payment, apt);
            }

            if (payment.Status == PaymentStatus.Processing)
            {
                payment.MarkAsPaid(request.TransactionId, payment.PaymentMethod ?? "cash");

                var appointment = await _unitOfWork.AppointmentRepository
                    .GetAllAsync(a => a.Id == payment.AppointmentId)
                    .Include(a => a.Doctor)
                    .FirstOrDefaultAsync(cancellationToken);

                if (appointment == null)
                    throw new NotFoundException(LocalizationKeys.AppointmentMessages.AppointmentNotFound.Value);

                appointment.Confirm(payment.Id);

                await _unitOfWork.SaveChangesAsync();

                var response = BuildResponse(payment, appointment);
                response.CompletedAt = DateTime.UtcNow;
                return response;
            }

            throw new BadRequestException(LocalizationKeys.PaymentMessages.PaymentFailed.Value);
        }

        private static BookingPaymentResponseDto BuildResponse(Domain.Entities.Payment payment, Domain.Entities.Appointment? appointment)
        {
            return new BookingPaymentResponseDto
            {
                PaymentId = payment.Id,
                ReservationId = payment.AppointmentId,
                Amount = payment.Amount,
                Currency = payment.Currency,
                Status = payment.Status,
                TransactionId = payment.TransactionId ?? payment.PaymobTransactionId,
                FailureReason = payment.FailureReason,
                CreatedAt = payment.CreatedAt,
                CompletedAt = payment.PaidAt,
                ReceiptUrl = null
            };
        }
    }
}
