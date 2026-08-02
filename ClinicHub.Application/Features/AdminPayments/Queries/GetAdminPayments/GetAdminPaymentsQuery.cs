using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.AdminPayments.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.AdminPayments.Queries.GetAdminPayments;

public class GetAdminPaymentsQuery : IRequest<PagginatedResult<AdminPaymentDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public PaymentType? Type { get; set; }
    public PaymentStatus? Status { get; set; }
    public PaymentMethod? Method { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SearchTerm { get; set; }
}
