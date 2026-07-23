using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClinicHub.Application.Features.Plans.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Plans.Queries.GetAllPlans
{
    public class GetAllPlansQueryHandler : IRequestHandler<GetAllPlansQuery, List<PlanDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllPlansQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<PlanDto>> Handle(GetAllPlansQuery request, CancellationToken cancellationToken)
        {
            var query = _unitOfWork.GetRepository<Plan, Guid>()
                .GetAllAsync(p => true);

            if (request.IsActive.HasValue)
                query = query.Where(p => p.IsActive == request.IsActive.Value);

            return await query
                .OrderBy(p => p.SortOrder)
                .ProjectTo<PlanDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
