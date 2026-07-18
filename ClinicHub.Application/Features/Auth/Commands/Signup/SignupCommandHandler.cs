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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace ClinicHub.Application.Features.Auth.Commands.Signup
{
    public sealed class SignupCommandHandler : IRequestHandler<SignupCommand, SignupResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly JwtSettings _jwtSettings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly IFcmService _fcmService;

        public SignupCommandHandler(
            UserManager<ApplicationUser> userManager,
            IJwtTokenService jwtTokenService,
            IOptions<JwtSettings> jwtSettings,
            IUnitOfWork unitOfWork,
            IStringLocalizer<Messages> localizer,
            IFcmService fcmService)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _jwtSettings = jwtSettings.Value;
            _unitOfWork = unitOfWork;
            _localizer = localizer;
            _fcmService = fcmService;
        }

        public async Task<SignupResult> Handle(SignupCommand request, CancellationToken cancellationToken)
        {
            var typeOfUser = request.TypeOfUser;

            var user = ApplicationUser.Create(
                request.FullName,
                request.Email,
                request.PhoneNumber,
                request.BirthDate,
                request.Gender);

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
                throw new UnAuthorizedException(JsonLocalizationProvider.GetLocalizedString(_localizer[LocalizationKeys.ExceptionMessages.Validation.Value]));

            var roleResult = await _userManager.AddToRoleAsync(user, UserType.User.ToString());
            if (!roleResult.Succeeded)
                throw new BadRequestException(_localizer[LocalizationKeys.AuthMessages.RoleAssignmentFailed.Value]);

            if (!string.IsNullOrEmpty(request.FcmToken) && request.DevicePlatform.HasValue)
                await _fcmService.RegisterTokenAsync(user.Id, request.FcmToken, request.DevicePlatform.Value);

            if (typeOfUser == TypeOfUserForRegisterFlow.User)
            {
                user.IsActive = true;

                var roles = await _userManager.GetRolesAsync(user);
                var clinicId = await _unitOfWork.ClinicRepository
                    .GetAllAsync(c => c.ClinicAdminId == user.Id && !c.IsDeleted)
                    .Select(c => (Guid?)c.Id)
                    .FirstOrDefaultAsync(cancellationToken);
                var accessToken = _jwtTokenService.GenerateAccessToken(user, roles, clinicId);
                var refreshToken = _jwtTokenService.GenerateRefreshToken(user);

                var userRefreshToken = UserRefreshToken.Create(user.Id, refreshToken, DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays));
                await _unitOfWork.UserRefreshTokenRepository.AddAsync(userRefreshToken);
                await _unitOfWork.SaveChangesAsync();

                return SignupResult.Authenticated(new AuthResponseDto(
                    accessToken, refreshToken, user.FullName, user.Email!, roles.FirstOrDefault(), user.Id, clinicId, user.ProfilePictureUrl));
            }
            else
            {
                user.IsActive = false;

                var requestedRole = typeOfUser == TypeOfUserForRegisterFlow.FreeLanceDoctor
                    ? UserType.Doctor
                    : UserType.ClinicOwner;

                if (!string.IsNullOrEmpty(request.DoctorImage))
                    user.UpdateProfilePicture(request.DoctorImage);

                var verification = UserVerification.Create(
                    user.Id,
                    requestedRole,
                    request.ProfessionalPracticeCardImage,
                    request.TaxCardImage,
                    request.UnionIdCardImage,
                    request.DoctorImage,
                    request.SpecializationId,
                    request.Bio,
                    request.YearsOfExperience);

                await _unitOfWork.UserVerificationRepository.AddAsync(verification);
                await _unitOfWork.SaveChangesAsync();

                return SignupResult.Pending(new SignupResponseDto(
                    user.Id,
                    _localizer[LocalizationKeys.AuthMessages.SignupPendingApproval.Value],
                    IsPendingApproval: true));
            }
        }
    }
}
