using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClinicHub.Application.Features.Plans.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Plans.Queries.GetActivePlans
{
    public class GetActivePlansQueryHandler : IRequestHandler<GetActivePlansQuery, List<PlanDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetActivePlansQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<PlanDto>> Handle(GetActivePlansQuery request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.GetRepository<Plan, Guid>()
                .GetAllAsync(p => p.IsActive && !p.IsDeleted)
                .OrderBy(p => p.SortOrder)
                .ProjectTo<PlanDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
