using ClinicHub.Application.Features.ClinicStaff.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.ClinicStaff.Queries.GetClinicStaffById
{
    public record GetClinicStaffByIdQuery(Guid Id) : IRequest<StaffDto>;
}
