using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Features.AdminPayments.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.AdminPayments.Queries.GetAdminPaymentDetail;

public class GetAdminPaymentDetailQueryHandler : IRequestHandler<GetAdminPaymentDetailQuery, AdminPaymentDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<Messages> _localizer;

    public GetAdminPaymentDetailQueryHandler(IUnitOfWork unitOfWork, IStringLocalizer<Messages> localizer)
    {
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    public async Task<AdminPaymentDetailDto> Handle(GetAdminPaymentDetailQuery request, CancellationToken cancellationToken)
    {
        var payment = await _unitOfWork.PaymentRepository
            .GetAllAsync(p => p.Id == request.PaymentId)
            .Include(p => p.Clinic)
            .Include(p => p.Appointment)
                .ThenInclude(a => a!.BookedByUser)
            .FirstOrDefaultAsync(cancellationToken);

        if (payment == null)
            throw new NotFoundException(LocalizationKeys.PaymentMessages.NotFound.Value);

        var isAppointment = payment.Type == PaymentType.Appointment && payment.Appointment != null;

        var itemName = payment.Type switch
        {
            PaymentType.Appointment => _localizer[LocalizationKeys.PaymentMessages.ItemAppointment.Value],
            PaymentType.Subscription => BuildSubscriptionItemName(payment),
            PaymentType.Ads => _localizer[LocalizationKeys.PaymentMessages.ItemAds.Value],
            _ => _localizer[LocalizationKeys.PaymentMessages.ItemAppointment.Value]
        };

        return new AdminPaymentDetailDto
        {
            Id = payment.Id,
            Code = payment.Code,
            Type = payment.Type,
            Payer = isAppointment ? payment.Appointment!.PatientFullName
                : payment.Clinic?.Name ?? payment.UserId.ToString(),
            PayerType = isAppointment ? "Patient" : "Clinic",
            PayerEmail = isAppointment ? payment.Appointment!.BookedByUser?.Email : payment.Clinic?.Email,
            PayerPhone = payment.Clinic?.Phone,
            ItemName = itemName,
            Amount = payment.Amount,
            Method = PaymentMethodMapper.ToEnum(payment.PaymentMethod),
            TransactionId = payment.PaymobTransactionId ?? payment.TransactionId,
            RefNumber = payment.RefNumber,
            Status = PaymentMethodMapper.ToUiStatus(payment.Status),
            Date = payment.CreatedAt,
            Notes = payment.Notes,
            Timeline = BuildTimeline(payment)
        };
    }

    private string BuildSubscriptionItemName(ClinicHub.Domain.Entities.Payment payment)
    {
        var periodLabel = payment.SubscriptionPeriod == SubscriptionPlan.Yearly
            ? _localizer[LocalizationKeys.PaymentMessages.SubscriptionPeriodYearly.Value]
            : _localizer[LocalizationKeys.PaymentMessages.SubscriptionPeriodMonthly.Value];

        var year = (payment.PaidAt ?? payment.CreatedAt).Year;
        return $"{periodLabel} - {year}";
    }

    private List<PaymentTimelineEntryDto> BuildTimeline(ClinicHub.Domain.Entities.Payment payment)
    {
        var timeline = new List<PaymentTimelineEntryDto>
        {
            new()
            {
                Date = payment.CreatedAt,
                Text = _localizer[LocalizationKeys.PaymentMessages.TimelineCreated.Value],
                Marker = "info"
            }
        };

        if (payment.PaidAt.HasValue)
        {
            timeline.Add(new PaymentTimelineEntryDto
            {
                Date = payment.PaidAt.Value,
                Text = _localizer[LocalizationKeys.PaymentMessages.TimelinePaid.Value],
                Marker = "success"
            });
        }

        if (payment.RefundedAt.HasValue)
        {
            timeline.Add(new PaymentTimelineEntryDto
            {
                Date = payment.RefundedAt.Value,
                Text = _localizer[LocalizationKeys.PaymentMessages.TimelineRefunded.Value],
                Marker = "danger"
            });
        }

        return timeline.OrderByDescending(t => t.Date).ToList();
    }
}
