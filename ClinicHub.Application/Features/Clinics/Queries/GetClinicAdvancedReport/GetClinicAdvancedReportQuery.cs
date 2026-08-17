using ClinicHub.Application.Features.Clinics.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Clinics.Queries.GetClinicAdvancedReport
{
    public class GetClinicAdvancedReportQuery : IRequest<ClinicAdvancedReportDto>
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}