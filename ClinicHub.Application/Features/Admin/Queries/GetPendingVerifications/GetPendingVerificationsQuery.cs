using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Users.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Admin.Queries.GetPendingVerifications
{
    public record GetPendingVerificationsQuery(int PageNumber = 1, int PageSize = 20) : IRequest<PagginatedResult<UserVerificationDto>>;
}
