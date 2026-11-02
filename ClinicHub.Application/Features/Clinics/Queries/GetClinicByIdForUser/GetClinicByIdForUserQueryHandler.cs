using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Features.Clinics.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Clinics.Queries.GetClinicByIdForUser
{
    public class GetClinicByIdForUserQueryHandler : IRequestHandler<GetClinicByIdForUserQuery, ClinicManagementDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetClinicByIdForUserQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ClinicManagementDto> Handle(GetClinicByIdForUserQuery request, CancellationToken cancellationToken)
        {
            var clinic = await _unitOfWork.ClinicRepository
                .GetAllAsync(c => c.Id == request.Id && !c.IsDeleted && c.IsActive && c.Status == ClinicStatus.Active)
                .Include(c => c.Specialization)
                .Include(c => c.ClinicAdmin)
                .FirstOrDefaultAsync(cancellationToken);

            if (clinic == null)
                throw new NotFoundException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

            var dto = _mapper.Map<ClinicManagementDto>(clinic);
            dto.Rating = await _unitOfWork.RatingRepository.GetClinicAverageRatingAsync(request.Id);
            dto.ReviewsCount = await _unitOfWork.RatingRepository.GetClinicRatingsCountAsync(request.Id);

            // Same source and defaults as GetClinicSettings: one non-deleted row per clinic.
            var bookingConfig = await _unitOfWork.BookingConfigurationRepository.GetByClinicIdAsync(request.Id);
            dto.ConsultationFee = bookingConfig?.ConsultationFee ?? 0;
            dto.ExaminationFee = dto.ConsultationFee;
            dto.Currency = bookingConfig?.Currency ?? "EGP";

            var doctors = await _unitOfWork.DoctorRepository
                .GetAllAsync(d => d.ClinicId == request.Id && !d.IsDeleted)
                .Include(d => d.User)
                .Include(d => d.Specialization)
                .ToListAsync(cancellationToken);

            dto.Doctors = doctors.Select(d => new ClinicDoctorDto
            {
                Id = d.Id,
                Name = d.User.FullName,
                Image = d.User.ProfilePictureUrl,
                Phone = d.User.PhoneNumber,
                Email = d.User.Email,
                SpecializationArName = d.Specialization.ArName,
                SpecializationEnName = d.Specialization.Name,
                Bio = d.Bio,
                YearsOfExperience = d.YearsOfExperience
            }).ToList();

            return dto;
        }
    }
}
