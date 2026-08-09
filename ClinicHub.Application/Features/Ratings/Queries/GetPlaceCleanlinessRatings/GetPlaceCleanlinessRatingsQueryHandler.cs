using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Features.Ratings.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Ratings.Queries.GetPlaceCleanlinessRatings
{
    public class GetPlaceCleanlinessRatingsQueryHandler : IRequestHandler<GetPlaceCleanlinessRatingsQuery, List<RatingDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetPlaceCleanlinessRatingsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<RatingDto>> Handle(GetPlaceCleanlinessRatingsQuery request, CancellationToken cancellationToken)
        {
            var clinicExists = await _unitOfWork.ClinicRepository.ExistsAsync(c => c.Id == request.ClinicId, cancellationToken);
            if (!clinicExists)
                throw new NotFoundException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

            var ratings = await _unitOfWork.RatingRepository
                .GetAllAsync(r => r.Type == RatingType.PlaceCleanliness && r.ClinicId == request.ClinicId)
                .Include(r => r.User)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<RatingDto>>(ratings);
        }
    }
}
