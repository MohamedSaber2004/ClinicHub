using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClinicHub.Application.Common.Extensions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Advertisements.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Advertisements.Queries.GetMyClinicAdvertisements
{
    public class GetMyClinicAdvertisementsQueryHandler : IRequestHandler<GetMyClinicAdvertisementsQuery, PagginatedResult<AdvertisementDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public GetMyClinicAdvertisementsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<PagginatedResult<AdvertisementDto>> Handle(GetMyClinicAdvertisementsQuery request, CancellationToken cancellationToken)
        {
            var clinicId = _currentUserService.CurrentClinicId;
            if (clinicId == null)
                return new PagginatedResult<AdvertisementDto>(Array.Empty<AdvertisementDto>(), 0);

            return await _unitOfWork.GetRepository<Advertisement, Guid>()
                .GetAllWithIncluding(a => a.ClinicId == clinicId, a => a.Clinic)
                .OrderByDescending(a => a.CreatedAt)
                .ProjectTo<AdvertisementDto>(_mapper.ConfigurationProvider)
                .AsPagginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
        }
    }
}
