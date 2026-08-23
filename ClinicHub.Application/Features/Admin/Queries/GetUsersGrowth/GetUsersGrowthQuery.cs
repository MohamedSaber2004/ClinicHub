using ClinicHub.Application.Features.Admin.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Admin.Queries.GetUsersGrowth
{
    public record GetUsersGrowthQuery(string Granularity = "day", DateTime? FromDate = null, DateTime? ToDate = null)
        : IRequest<List<UsersGrowthPointDto>>;
}
