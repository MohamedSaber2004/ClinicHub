using MediatR;

namespace ClinicHub.Application.Features.Users.Queries.GetUserAdminOverview
{
    public class GetUserAdminOverviewQuery : IRequest<AdminUserOverviewDto>
    {
        public Guid UserId { get; set; }
    }
}
