using ClinicHub.Application.Features.AdminPayments.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.AdminPayments.Queries.GetAdminPaymentStats;

public class GetAdminPaymentStatsQuery : IRequest<AdminPaymentStatsDto>
{
    public PaymentType? Type { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
