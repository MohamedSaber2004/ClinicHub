using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.ClinicPayments.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.ClinicPayments.Queries.GetAppointmentPayments;

public class GetAppointmentPaymentsQuery : IRequest<PagginatedResult<AppointmentPaymentDto>>
{
    public int? Status { get; set; }
    public int? Method { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
