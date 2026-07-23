using ClinicHub.Application.Features.Admin.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Admin.Queries.GetAdminDashboardStats
{
    public record GetAdminDashboardStatsQuery : IRequest<AdminDashboardStatsDto>;
}
