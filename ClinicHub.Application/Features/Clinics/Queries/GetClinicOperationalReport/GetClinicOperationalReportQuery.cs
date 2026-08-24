using ClinicHub.Application.Features.Clinics.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Clinics.Queries.GetClinicOperationalReport
{
    public class GetClinicOperationalReportQuery : IRequest<ClinicOperationalReportDto>
    {
        /// <summary>today | week | month | last30</summary>
        public string? Period { get; set; } = "week";
        public Guid? DoctorId { get; set; }
    }
}
