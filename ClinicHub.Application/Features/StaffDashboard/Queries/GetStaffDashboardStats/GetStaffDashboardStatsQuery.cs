using ClinicHub.Application.Features.StaffDashboard.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.StaffDashboard.Queries.GetStaffDashboardStats
{
    public record GetStaffDashboardStatsQuery : IRequest<StaffDashboardStatsDto>;
}
