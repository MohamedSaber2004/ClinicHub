using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.ClinicStaff.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.ClinicStaff.Queries.GetClinicStaff
{
    public class GetClinicStaffQuery : IRequest<PagginatedResult<StaffDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
