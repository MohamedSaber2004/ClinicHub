using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Features.Plans.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Plans.Commands.UpdatePlan
{
    public class UpdatePlanCommandHandler : IRequestHandler<UpdatePlanCommand, PlanDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdatePlanCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PlanDto> Handle(UpdatePlanCommand request, CancellationToken cancellationToken)
        {
            var plan = await _unitOfWork.GetRepository<Plan, Guid>().FindByKeyAsync(request.Id);
            if (plan == null)
                throw new NotFoundException(LocalizationKeys.PlanMessages.NotFound.Value);

            plan.Name = request.Name;
            plan.NameAr = request.NameAr;
            plan.Description = request.Description;
            plan.DescriptionAr = request.DescriptionAr;
            plan.PriceMonthly = request.PriceMonthly;
            plan.PriceYearly = request.PriceYearly;
            plan.MaxDoctors = request.MaxDoctors;
            plan.MaxStaff = request.MaxStaff;
            plan.Features = request.Features;
            plan.IsActive = request.IsActive;
            plan.SortOrder = request.SortOrder;

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PlanDto>(plan);
        }
    }
}
