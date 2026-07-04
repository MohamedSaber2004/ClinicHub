using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Features.Ratings.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Ratings.Queries.GetDoctorRatings
{
    public class GetDoctorRatingsQueryHandler : IRequestHandler<GetDoctorRatingsQuery, List<RatingDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetDoctorRatingsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<RatingDto>> Handle(GetDoctorRatingsQuery request, CancellationToken cancellationToken)
        {
            var doctorExists = await _unitOfWork.DoctorRepository.ExistsAsync(d => d.Id == request.DoctorId, cancellationToken);
            if (!doctorExists)
                throw new NotFoundException(LocalizationKeys.DoctorMessages.NotFound.Value);

            var ratings = await _unitOfWork.RatingRepository
                .GetAllAsync(r => r.DoctorId == request.DoctorId)
                .Include(r => r.User)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<RatingDto>>(ratings);
        }
    }
}
