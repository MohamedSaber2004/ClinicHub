using ClinicHub.Application.Features.AdminPayments.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.AdminPayments.Queries.GetAdminPaymentDetail;

public class GetAdminPaymentDetailQuery : IRequest<AdminPaymentDetailDto>
{
    public Guid PaymentId { get; set; }
}
