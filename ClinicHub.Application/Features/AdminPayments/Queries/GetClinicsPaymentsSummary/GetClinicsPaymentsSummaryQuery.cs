using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.AdminPayments.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.AdminPayments.Queries.GetClinicsPaymentsSummary;

public class GetClinicsPaymentsSummaryQuery : IRequest<PagginatedResult<ClinicPaymentsSummaryDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SearchTerm { get; set; }
}
