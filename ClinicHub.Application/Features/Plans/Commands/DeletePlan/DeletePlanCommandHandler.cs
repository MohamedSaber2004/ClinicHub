using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Plans.Commands.DeletePlan
{
    public class DeletePlanCommandHandler : IRequestHandler<DeletePlanCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeletePlanCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeletePlanCommand request, CancellationToken cancellationToken)
        {
            var plan = await _unitOfWork.GetRepository<Plan, Guid>().FindByKeyAsync(request.Id);
            if (plan == null)
                throw new NotFoundException(LocalizationKeys.PlanMessages.NotFound.Value);

            _unitOfWork.GetRepository<Plan, Guid>().Delete(plan);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
