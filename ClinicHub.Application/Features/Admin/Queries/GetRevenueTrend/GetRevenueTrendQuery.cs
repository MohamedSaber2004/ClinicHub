using ClinicHub.Application.Features.Admin.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Admin.Queries.GetRevenueTrend
{
    public record GetRevenueTrendQuery(string Granularity = "day", DateTime? FromDate = null, DateTime? ToDate = null)
        : IRequest<List<RevenueTrendPointDto>>;
}
