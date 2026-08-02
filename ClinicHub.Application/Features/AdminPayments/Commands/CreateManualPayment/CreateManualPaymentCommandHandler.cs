using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Features.AdminPayments.DTOs;
using ClinicHub.Application.Features.Ads;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.AdminPayments.Commands.CreateManualPayment;

public class CreateManualPaymentCommandHandler : IRequestHandler<CreateManualPaymentCommand, AdminPaymentDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<Messages> _localizer;

    public CreateManualPaymentCommandHandler(IUnitOfWork unitOfWork, IStringLocalizer<Messages> localizer)
    {
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    public async Task<AdminPaymentDto> Handle(CreateManualPaymentCommand request, CancellationToken cancellationToken)
    {
        if (request.Type != PaymentType.Subscription && request.Type != PaymentType.Ads)
            throw new BadRequestException(_localizer[LocalizationKeys.PaymentMessages.ManualTypeUnsupported.Value]);

        var clinic = await _unitOfWork.ClinicRepository.FindByKeyAsync(request.PayerId, cancellationToken);
        if (clinic == null)
            throw new NotFoundException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

        if (request.Type == PaymentType.Ads && !await AdsOrderProcessor.IsEligibleForAdsAsync(_unitOfWork, clinic.Id, cancellationToken))
            throw new ForbiddenException(_localizer[LocalizationKeys.PaymentMessages.AdsNotEligible.Value]);

        var userId = await ResolveClinicUserAsync(clinic, cancellationToken);

        var payment = new ClinicHub.Domain.Entities.Payment(request.Type, userId, clinic.Id, request.Amount);
        payment.SetManualReference(request.RefNumber, request.Notes);
        payment.MarkAsManuallyPaid(PaymentMethodMapper.ToDbString(request.Method), request.RefNumber);

        if (request.Type == PaymentType.Ads)
        {
            var pendingAd = await _unitOfWork.GetRepository<Advertisement, Guid>()
                .GetAllAsync(a => a.ClinicId == clinic.Id && a.Status == AdvertisementStatus.PendingPayment)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (pendingAd != null)
            {
                pendingAd.Activate(DateTime.UtcNow, pendingAd.DurationDays);
                pendingAd.PaymentId = payment.Id;
                _unitOfWork.GetRepository<Advertisement, Guid>().Update(pendingAd);
            }
        }

        await _unitOfWork.PaymentRepository.AddAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        return new AdminPaymentDto
        {
            Id = payment.Id,
            Code = payment.Code,
            Type = payment.Type,
            Payer = clinic.Name,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Method = request.Method,
            Status = PaymentStatus.Paid,
            Date = payment.CreatedAt,
            RefNumber = payment.RefNumber
        };
    }

    private async Task<Guid> ResolveClinicUserAsync(Clinic clinic, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.GetRepository<ApplicationUser, Guid>()
            .GetAllAsync(u => u.ClinicId == clinic.Id && !u.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (user != null)
            return user.Id;

        if (clinic.ClinicAdminId.HasValue)
            return clinic.ClinicAdminId.Value;

        throw new BadRequestException(_localizer[LocalizationKeys.PaymentMessages.PayerUserNotFound.Value]);
    }
}
