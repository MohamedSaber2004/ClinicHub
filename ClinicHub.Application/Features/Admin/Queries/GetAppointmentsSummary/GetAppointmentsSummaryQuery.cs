using ClinicHub.Application.Features.Admin.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Admin.Queries.GetAppointmentsSummary
{
    public record GetAppointmentsSummaryQuery(string Granularity = "day", DateTime? FromDate = null, DateTime? ToDate = null)
        : IRequest<List<AppointmentsSummaryPointDto>>;
}
