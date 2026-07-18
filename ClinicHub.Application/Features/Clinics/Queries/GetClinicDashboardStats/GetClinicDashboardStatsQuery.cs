using ClinicHub.Application.Features.Clinics.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Clinics.Queries.GetClinicDashboardStats
{
    public record GetClinicDashboardStatsQuery : IRequest<ClinicDashboardStatsDto>;
}
