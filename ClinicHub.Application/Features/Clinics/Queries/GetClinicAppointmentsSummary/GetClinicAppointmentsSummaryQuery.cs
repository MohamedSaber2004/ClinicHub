using ClinicHub.Application.Features.Admin.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Clinics.Queries.GetClinicAppointmentsSummary
{
    public class GetClinicAppointmentsSummaryQuery : IRequest<List<AppointmentsSummaryPointDto>>
    {
        public string? Granularity { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
