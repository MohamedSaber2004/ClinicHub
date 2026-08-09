using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Ratings.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Ratings.Commands.SubmitVisitRatings
{
    public class SubmitVisitRatingsCommandHandler : IRequestHandler<SubmitVisitRatingsCommand, List<RatingDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public SubmitVisitRatingsCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<List<RatingDto>> Handle(SubmitVisitRatingsCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            Guid? clinicId = request.ClinicId;

            if (request.DoctorId != null)
            {
                var doctor = await RatingValidationHelper.GetValidatedDoctorAsync(_unitOfWork, userId, request.DoctorId.Value, cancellationToken);

                clinicId = doctor.ClinicId ?? request.ClinicId;

                var existingDoctor = await _unitOfWork.RatingRepository.GetUserRatingForDoctorAsync(userId, request.DoctorId.Value);
                if (existingDoctor != null)
                    throw new BadRequestException(LocalizationKeys.RatingMessages.AlreadyRated.Value);
            }

            if (clinicId == null)
                throw new BadRequestException(LocalizationKeys.RatingMessages.TargetRequired.Value);

            await RatingValidationHelper.ValidateClinicAsync(_unitOfWork, userId, clinicId.Value, cancellationToken);

            var existingClinic = await _unitOfWork.RatingRepository.GetUserRatingForClinicAsync(userId, clinicId.Value, RatingType.Clinic);
            if (existingClinic != null)
                throw new BadRequestException(LocalizationKeys.RatingMessages.AlreadyRated.Value);

            var existingReception = await _unitOfWork.RatingRepository.GetUserRatingForClinicAsync(userId, clinicId.Value, RatingType.Reception);
            if (existingReception != null)
                throw new BadRequestException(LocalizationKeys.RatingMessages.AlreadyRated.Value);

            var existingCleanliness = await _unitOfWork.RatingRepository.GetUserRatingForClinicAsync(userId, clinicId.Value, RatingType.PlaceCleanliness);
            if (existingCleanliness != null)
                throw new BadRequestException(LocalizationKeys.RatingMessages.AlreadyRated.Value);

            var ratings = new List<Rating>();

            if (request.DoctorId != null && request.DoctorValue.HasValue)
                ratings.Add(new Rating(userId, RatingType.Doctor, request.DoctorId, null, request.DoctorValue.Value, request.Review));

            ratings.Add(new Rating(userId, RatingType.Clinic, null, clinicId, request.ClinicValue, request.Review));
            ratings.Add(new Rating(userId, RatingType.Reception, null, clinicId, request.ReceptionValue, request.Review));
            ratings.Add(new Rating(userId, RatingType.PlaceCleanliness, null, clinicId, request.CleanlinessValue, request.Review));

            await _unitOfWork.RatingRepository.AddRangeAsync(ratings);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                throw new BadRequestException(LocalizationKeys.RatingMessages.AlreadyRated.Value);
            }

            return _mapper.Map<List<RatingDto>>(ratings);
        }

        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            return message.Contains("unique key", StringComparison.OrdinalIgnoreCase)
                || message.Contains("unique index", StringComparison.OrdinalIgnoreCase)
                || message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase);
        }
    }
}
