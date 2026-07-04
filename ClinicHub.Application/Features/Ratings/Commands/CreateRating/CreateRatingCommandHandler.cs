using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Ratings.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Ratings.Commands.CreateRating
{
    public class CreateRatingCommandHandler : IRequestHandler<CreateRatingCommand, RatingDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public CreateRatingCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<RatingDto> Handle(CreateRatingCommand request, CancellationToken cancellationToken)
        {
            if (request.DoctorId == null && request.ClinicId == null)
                throw new BadRequestException(LocalizationKeys.RatingMessages.TargetRequired.Value);

            if (request.DoctorId != null && request.ClinicId != null)
                throw new BadRequestException(LocalizationKeys.RatingMessages.SingleTargetRequired.Value);

            var userId = _currentUserService.UserId;

            Guid? clinicId = request.ClinicId;

            if (request.DoctorId != null)
            {
                var doctor = await _unitOfWork.DoctorRepository.GetByIdAsync(request.DoctorId.Value);
                if (doctor == null)
                    throw new NotFoundException(LocalizationKeys.DoctorMessages.NotFound.Value);

                var existing = await _unitOfWork.RatingRepository.GetUserRatingForDoctorAsync(userId, request.DoctorId.Value);
                if (existing != null)
                    throw new BadRequestException(LocalizationKeys.RatingMessages.AlreadyRated.Value);

                clinicId = doctor.ClinicId;
            }

            if (request.ClinicId != null)
            {
                var clinicExists = await _unitOfWork.ClinicRepository.ExistsAsync(c => c.Id == request.ClinicId.Value, cancellationToken);
                if (!clinicExists)
                    throw new NotFoundException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

                var existing = await _unitOfWork.RatingRepository.GetUserRatingForClinicAsync(userId, request.ClinicId.Value);
                if (existing != null)
                    throw new BadRequestException(LocalizationKeys.RatingMessages.AlreadyRated.Value);
            }

            var rating = new Rating(userId, request.DoctorId, clinicId, request.Value, request.Review);

            await _unitOfWork.RatingRepository.AddAsync(rating);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<RatingDto>(rating);
        }
    }
}
