using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Features.Ratings.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Ratings.Queries.GetReceptionRatings
{
    public class GetReceptionRatingsQueryHandler : IRequestHandler<GetReceptionRatingsQuery, List<RatingDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetReceptionRatingsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<RatingDto>> Handle(GetReceptionRatingsQuery request, CancellationToken cancellationToken)
        {
            var clinicExists = await _unitOfWork.ClinicRepository.ExistsAsync(c => c.Id == request.ClinicId, cancellationToken);
            if (!clinicExists)
                throw new NotFoundException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

            var ratings = await _unitOfWork.RatingRepository
                .GetAllAsync(r => r.Type == RatingType.Reception && r.ClinicId == request.ClinicId)
                .Include(r => r.User)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<RatingDto>>(ratings);
        }
    }
}
