using ClinicHub.Application.Features.Users.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Admin.Queries.GetPendingVerifications
{
    public record GetPendingVerificationsQuery : IRequest<IReadOnlyList<UserVerificationDto>>;
}
