using AutoMapper;
using ClinicHub.Application.Features.Plans.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Plans.Commands.CreatePlan
{
    public class CreatePlanCommandHandler : IRequestHandler<CreatePlanCommand, PlanDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreatePlanCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PlanDto> Handle(CreatePlanCommand request, CancellationToken cancellationToken)
        {
            var plan = new Plan
            {
                Name = request.Name,
                NameAr = request.NameAr,
                Description = request.Description,
                DescriptionAr = request.DescriptionAr,
                PriceMonthly = request.PriceMonthly,
                PriceYearly = request.PriceYearly,
                MaxDoctors = request.MaxDoctors,
                MaxStaff = request.MaxStaff,
                Features = request.Features,
                IsActive = request.IsActive,
                SortOrder = request.SortOrder
            };

            await _unitOfWork.GetRepository<Plan, Guid>().AddAsync(plan);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PlanDto>(plan);
        }
    }
}
