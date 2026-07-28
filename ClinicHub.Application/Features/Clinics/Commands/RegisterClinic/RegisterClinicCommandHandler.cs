using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Options;
using ClinicHub.Application.Features.Auth.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using NetTopologySuite.Geometries;

namespace ClinicHub.Application.Features.Clinics.Commands.RegisterClinic
{
    public class RegisterClinicCommandHandler : IRequestHandler<RegisterClinicCommand, SignupResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStringLocalizer<Messages> _localizer;

        public RegisterClinicCommandHandler(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            IStringLocalizer<Messages> localizer)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _localizer = localizer;
        }

        public async Task<SignupResult> Handle(RegisterClinicCommand request, CancellationToken cancellationToken)
        {
            var user = ApplicationUser.Create(
                request.FullName,
                request.Email,
                request.PhoneNumber,
                request.BirthDate,
                request.Gender);

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
                throw new BadRequestException(JsonLocalizationProvider.GetLocalizedString(_localizer[LocalizationKeys.ExceptionMessages.Validation.Value]));

            var roleResult = await _userManager.AddToRoleAsync(user, UserType.ClinicOwner.ToString());
            if (!roleResult.Succeeded)
                throw new BadRequestException(_localizer[LocalizationKeys.AuthMessages.RoleAssignmentFailed.Value]);

            user.IsActive = false;

            var clinic = new Clinic
            {
                Name = request.ClinicName,
                NameAr = request.ClinicNameAr,
                Description = request.ClinicDescription,
                Address = request.ClinicAddress,
                AddressAr = request.ClinicAddress,
                Phone = request.ClinicPhone,
                Email = request.ClinicEmail,
                Logo = request.Logo,
                WorkingHours = request.WorkingHours,
                WorkingHoursStart = request.WorkingHoursStart,
                WorkingHoursEnd = request.WorkingHoursEnd,
                WorkingDays = request.WorkingDays != null ? string.Join(",", request.WorkingDays) : null,
                SpecializationId = request.SpecializationId,
                Location = request.Lat.HasValue && request.Lng.HasValue
                    ? new Point(request.Lng.Value, request.Lat.Value) { SRID = 4326 }
                    : new Point(0, 0) { SRID = 4326 },
                Status = ClinicStatus.PendingApproval,
                ClinicAdminId = user.Id,
                IsRegistered = true
            };

            await _unitOfWork.ClinicRepository.AddAsync(clinic);

            user.AssignToClinic(clinic.Id);

            var verification = UserVerification.Create(
                user.Id,
                UserType.ClinicOwner,
                request.ProfessionalPracticeCardImage,
                request.TaxCardImage,
                request.UnionIdCardImage,
                request.DoctorImage,
                request.SpecializationId,
                request.Bio,
                request.YearsOfExperience);

            await _unitOfWork.UserVerificationRepository.AddAsync(verification);

            var doctor = new Doctor(
                user.Id,
                clinic.Id,
                request.SpecializationId,
                request.Bio ?? string.Empty,
                request.YearsOfExperience ?? 0);

            await _unitOfWork.DoctorRepository.AddAsync(doctor);

            user.UpdateProfilePicture(request.DoctorImage);

            await _unitOfWork.SaveChangesAsync();

            return SignupResult.Pending(new SignupResponseDto(
                user.Id,
                _localizer[LocalizationKeys.AuthMessages.SignupPendingApproval.Value],
                IsPendingApproval: true));
        }
    }
}
