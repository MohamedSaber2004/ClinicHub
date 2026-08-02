using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.AdminPayments.Commands.RefundPayment;

public class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymobService _paymobService;
    private readonly IStringLocalizer<Messages> _localizer;

    public RefundPaymentCommandHandler(IUnitOfWork unitOfWork, IPaymobService paymobService, IStringLocalizer<Messages> localizer)
    {
        _unitOfWork = unitOfWork;
        _paymobService = paymobService;
        _localizer = localizer;
    }

    public async Task<bool> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _unitOfWork.PaymentRepository.GetByIdAsync(request.PaymentId);
        if (payment == null)
            throw new NotFoundException(LocalizationKeys.PaymentMessages.NotFound.Value);

        if (payment.Status == PaymentStatus.Refunded)
            throw new BadRequestException(LocalizationKeys.PaymentMessages.AlreadyRefunded.Value);

        if (!string.IsNullOrWhiteSpace(payment.PaymobTransactionId))
        {
            var refund = await _paymobService.RefundTransactionAsync(payment.PaymobTransactionId, payment.Amount, cancellationToken);
            if (!refund.Success)
                throw new BadRequestException(_localizer[LocalizationKeys.PaymentMessages.RefundFailed.Value]);

            payment.MarkAsRefunded(string.IsNullOrWhiteSpace(request.Reason) ? refund.RefundId : request.Reason);
        }
        else
        {
            payment.MarkAsRefunded(request.Reason);
        }

        if (payment.Type == PaymentType.Ads)
        {
            var advertisement = await _unitOfWork.GetRepository<Advertisement, Guid>()
                .GetAllAsync(a => a.PaymentId == payment.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (advertisement != null && advertisement.Status == AdvertisementStatus.Active)
            {
                advertisement.Deactivate();
                _unitOfWork.GetRepository<Advertisement, Guid>().Update(advertisement);
            }
        }

        // Refunding an appointment payment returns the linked appointment to a cancelled state.
        if (payment.Type == PaymentType.Appointment && payment.AppointmentId.HasValue)
        {
            var appointment = await _unitOfWork.AppointmentRepository
                .GetAllAsync(a => a.Id == payment.AppointmentId.Value)
                .FirstOrDefaultAsync(cancellationToken);

            if (appointment != null && appointment.Status == AppointmentStatus.Confirmed)
            {
                appointment.Cancel(string.IsNullOrWhiteSpace(request.Reason) ? "Payment refunded" : request.Reason);
            }
        }

        _unitOfWork.PaymentRepository.Update(payment);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}