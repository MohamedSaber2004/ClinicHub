using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClinicHub.Application.Common.Extensions;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Subscriptions.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Subscriptions.Queries.GetAllSubscriptions
{
    public class GetAllSubscriptionsQueryHandler : IRequestHandler<GetAllSubscriptionsQuery, PagginatedResult<SubscriptionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllSubscriptionsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagginatedResult<SubscriptionDto>> Handle(GetAllSubscriptionsQuery request, CancellationToken cancellationToken)
        {
            var query = _unitOfWork.GetRepository<Subscription, Guid>()
                .GetAllWithIncluding(s => true, s => s.Clinic)
                .AsQueryable();

            if (request.Status.HasValue)
                query = query.Where(s => s.Status == request.Status.Value);

            if (request.Plan.HasValue)
                query = query.Where(s => s.Plan == request.Plan.Value);

            if (request.ClinicId.HasValue)
                query = query.Where(s => s.ClinicId == request.ClinicId.Value);

            query = query.OrderByDescending(s => s.CreatedAt);

            return await query
                .ProjectTo<SubscriptionDto>(_mapper.ConfigurationProvider)
                .AsPagginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
        }
    }
}
