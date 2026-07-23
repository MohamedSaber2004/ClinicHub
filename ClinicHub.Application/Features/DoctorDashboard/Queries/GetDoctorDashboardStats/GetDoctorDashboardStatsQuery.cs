using ClinicHub.Application.Features.DoctorDashboard.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.DoctorDashboard.Queries.GetDoctorDashboardStats
{
    public record GetDoctorDashboardStatsQuery : IRequest<DoctorDashboardStatsDto>;
}
