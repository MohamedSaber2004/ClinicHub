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
            var type = request.Type ?? (request.DoctorId != null ? RatingType.Doctor : RatingType.Clinic);

            if (type == RatingType.Doctor && request.DoctorId == null)
                throw new BadRequestException(LocalizationKeys.RatingMessages.TargetRequired.Value);

            if (type != RatingType.Doctor && request.ClinicId == null)
                throw new BadRequestException(LocalizationKeys.RatingMessages.TargetRequired.Value);

            if (request.DoctorId != null && request.ClinicId != null)
                throw new BadRequestException(LocalizationKeys.RatingMessages.SingleTargetRequired.Value);

            var userId = _currentUserService.UserId;

            Guid? clinicId = request.ClinicId;

            if (request.DoctorId != null)
            {
                await RatingValidationHelper.GetValidatedDoctorAsync(_unitOfWork, userId, request.DoctorId.Value, cancellationToken);

                var existing = await _unitOfWork.RatingRepository.GetUserRatingForDoctorAsync(userId, request.DoctorId.Value);
                if (existing != null)
                    throw new BadRequestException(LocalizationKeys.RatingMessages.AlreadyRated.Value);

                clinicId = null;
            }

            if (request.ClinicId != null)
            {
                await RatingValidationHelper.ValidateClinicAsync(_unitOfWork, userId, request.ClinicId.Value, cancellationToken);

                var existing = await _unitOfWork.RatingRepository.GetUserRatingForClinicAsync(userId, request.ClinicId.Value, type);
                if (existing != null)
                    throw new BadRequestException(LocalizationKeys.RatingMessages.AlreadyRated.Value);
            }

            var rating = new Rating(userId, type, request.DoctorId, clinicId, request.Value, request.Review);

            await _unitOfWork.RatingRepository.AddAsync(rating);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                throw new BadRequestException(LocalizationKeys.RatingMessages.AlreadyRated.Value);
            }

            return _mapper.Map<RatingDto>(rating);
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
