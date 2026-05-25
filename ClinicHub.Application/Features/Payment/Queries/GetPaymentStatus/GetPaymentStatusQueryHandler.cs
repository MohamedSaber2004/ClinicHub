using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Payment.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Payment.Queries.GetPaymentStatus;

public class GetPaymentStatusQueryHandler : IRequestHandler<GetPaymentStatusQuery, PaymentStatusDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetPaymentStatusQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<PaymentStatusDto> Handle(GetPaymentStatusQuery request, CancellationToken cancellationToken)
    {
        var payment = await _unitOfWork.PaymentRepository.GetByAppointmentIdAsync(request.AppointmentId);
        
        if (payment == null)
            throw new NotFoundException(LocalizationKeys.PaymentMessages.NotFound.Value);

        if (payment.UserId != _currentUser.UserId)
            throw new UnauthorizedAccessException(LocalizationKeys.PaymentMessages.Unauthorized.Value);

        return new PaymentStatusDto
        {
            PaymentId = payment.Id,
            AppointmentId = payment.AppointmentId,
            Status = payment.Status,
            Amount = payment.Amount,
            PaidAt = payment.PaidAt,
            TransactionId = payment.PaymobTransactionId
        };
    }
}
