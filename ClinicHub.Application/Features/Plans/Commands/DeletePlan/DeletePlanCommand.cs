using MediatR;

namespace ClinicHub.Application.Features.Plans.Commands.DeletePlan
{
    public class DeletePlanCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }
}
