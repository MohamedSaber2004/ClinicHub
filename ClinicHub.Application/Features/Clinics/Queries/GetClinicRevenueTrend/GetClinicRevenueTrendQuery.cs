using ClinicHub.Application.Features.Admin.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Clinics.Queries.GetClinicRevenueTrend
{
    public class GetClinicRevenueTrendQuery : IRequest<List<RevenueTrendPointDto>>
    {
        public string? Granularity { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
