using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.StaffDashboard.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.StaffDashboard.Queries.GetStaffQueue
{
    public class GetStaffQueueQuery : IRequest<List<StaffQueueItemDto>>
    {
    }
}
