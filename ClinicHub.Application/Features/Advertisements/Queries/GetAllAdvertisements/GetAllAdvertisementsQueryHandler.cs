using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClinicHub.Application.Common.Extensions;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Advertisements.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Advertisements.Queries.GetAllAdvertisements
{
    public class GetAllAdvertisementsQueryHandler : IRequestHandler<GetAllAdvertisementsQuery, PagginatedResult<AdvertisementDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllAdvertisementsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagginatedResult<AdvertisementDto>> Handle(GetAllAdvertisementsQuery request, CancellationToken cancellationToken)
        {
            var query = _unitOfWork.GetRepository<Advertisement, Guid>()
                .GetAllWithIncluding(a => true, a => a.Clinic)
                .AsQueryable();

            if (request.Status.HasValue)
                query = query.Where(a => a.Status == request.Status.Value);

            if (request.ClinicId.HasValue)
                query = query.Where(a => a.ClinicId == request.ClinicId.Value);

            query = query.OrderByDescending(a => a.CreatedAt);

            return await query
                .ProjectTo<AdvertisementDto>(_mapper.ConfigurationProvider)
                .AsPagginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
        }
    }
}
