using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
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
        private readonly IBackgroundJobScheduler _jobScheduler;

        public VerifyBookingPaymentCommandHandler(IUnitOfWork unitOfWork, IBackgroundJobScheduler jobScheduler)
        {
            _unitOfWork = unitOfWork;
            _jobScheduler = jobScheduler;
        }

        public async Task<BookingPaymentResponseDto> Handle(VerifyBookingPaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _unitOfWork.PaymentRepository.GetByIdAsync(request.PaymentId);

            if (payment == null)
                throw new NotFoundException(LocalizationKeys.PaymentMessages.NotFound.Value);

            if (payment.Status == PaymentStatus.Paid)
            {
                var apt = payment.AppointmentId.HasValue
                    ? await _unitOfWork.AppointmentRepository.GetByIdAsync(payment.AppointmentId.Value)
                    : null;
                return BuildResponse(payment, apt);
            }

            if (payment.Status is PaymentStatus.Processing or PaymentStatus.Pending)
            {
                payment.MarkAsPaid(request.TransactionId, payment.PaymentMethod ?? "cash");

                if (!payment.AppointmentId.HasValue)
                {
                    await _unitOfWork.SaveChangesAsync();
                    return BuildResponse(payment, null);
                }

                var appointment = await _unitOfWork.AppointmentRepository
                    .GetAllAsync(a => a.Id == payment.AppointmentId.Value)
                    .Include(a => a.Doctor)
                    .FirstOrDefaultAsync(cancellationToken);

                if (appointment == null)
                    throw new NotFoundException(LocalizationKeys.AppointmentMessages.AppointmentNotFound.Value);

                appointment.Confirm(payment.Id);

                if (payment.PaidAt.HasValue)
                {
                    var bookingConfig = await _unitOfWork.BookingConfigurationRepository.GetByClinicIdAsync(appointment.ClinicId);
                    var windowMinutes = bookingConfig?.CancellationWindowMinutes ?? 120;
                    await _jobScheduler.ScheduleCancellationWindowCloseAsync(appointment.Id, payment.PaidAt.Value.AddMinutes(windowMinutes));
                }

                await _jobScheduler.ScheduleNoShowMarkingAsync(appointment.Id, appointment.AppointmentDate.Add(appointment.EndTime).AddMinutes(30));

                await _unitOfWork.SaveChangesAsync();

                var response = BuildResponse(payment, appointment);
                response.CompletedAt = DateTime.Now;
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
