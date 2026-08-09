using ClinicHub.Application.Common;
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicHub.Application.Features.Auth.Commands.LoginWeb
{
    public sealed class LoginWebCommandHandler : IRequestHandler<LoginWebCommand, AuthResponseDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly JwtSettings _jwtSettings;
        private readonly EmailSettings _emailSettings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFcmService _fcmService;
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly ILogger<LoginWebCommandHandler> _logger;

        public LoginWebCommandHandler(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IJwtTokenService jwtTokenService,
            IOptions<JwtSettings> jwtSettings,
            IOptions<EmailSettings> emailSettings,
            IUnitOfWork unitOfWork,
            IFcmService fcmService,
            IStringLocalizer<Messages> localizer,
            ILogger<LoginWebCommandHandler> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _jwtTokenService = jwtTokenService;
            _jwtSettings = jwtSettings.Value;
            _emailSettings = emailSettings.Value;
            _unitOfWork = unitOfWork;
            _fcmService = fcmService;
            _localizer = localizer;
            _logger = logger;
        }

        public async Task<AuthResponseDto> Handle(LoginWebCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
                throw new UnAuthorizedException(_localizer[LocalizationKeys.AuthMessages.InvalidCredentials.Value]);

            if (user.IsDeleted)
                throw new ForbiddenException(_localizer[LocalizationKeys.AuthMessages.AccountDeleted.Value]);

            if (!user.IsActive)
                throw new ForbiddenException(_localizer[LocalizationKeys.AuthMessages.AccountPendingApproval.Value]);

            var hasPendingVerification = await _unitOfWork.UserVerificationRepository
                .GetAllAsync(v => v.UserId == user.Id && !v.IsDeleted && v.Status == Domain.Enums.VerificationStatus.Pending)
                .AnyAsync(cancellationToken);

            if (hasPendingVerification)
                throw new ForbiddenException(_localizer[LocalizationKeys.AuthMessages.AccountPendingApproval.Value]);

            var roles = await _userManager.GetRolesAsync(user);

            var doctorId = await _unitOfWork.DoctorRepository
                .GetAllAsync(d => d.UserId == user.Id && !d.IsDeleted)
                .Select(d => (Guid?)d.Id)
                .FirstOrDefaultAsync(cancellationToken);

            var clinicId = user.ClinicId;
            if (!clinicId.HasValue)
            {
                clinicId = await _unitOfWork.ClinicRepository
                    .GetAllAsync(c => c.ClinicAdminId == user.Id && !c.IsDeleted)
                    .Select(c => (Guid?)c.Id)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (!clinicId.HasValue)
            {
                clinicId = await _unitOfWork.DoctorRepository
                    .GetAllAsync(d => d.UserId == user.Id && d.ClinicId != null && !d.IsDeleted)
                    .Select(d => (Guid?)d.ClinicId)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            var hasActiveSubscription = clinicId.HasValue
                && await _unitOfWork.GetRepository<Subscription, Guid>()
                    .ExistsAsync(s => s.ClinicId == clinicId.Value && s.Status == SubscriptionStatus.Active && s.EndDate > DateTime.Now, cancellationToken);

            var accessToken = _jwtTokenService.GenerateAccessToken(user, roles, clinicId, hasActiveSubscription);

            var existingToken = await _unitOfWork.UserRefreshTokenRepository
                .GetFirstAsync(x => x.UserId == user.Id && !x.IsRevoked && x.ExpiryDate > DateTime.Now, cancellationToken);

            string refreshToken;
            if (existingToken != null)
            {
                refreshToken = existingToken.Token;
            }
            else
            {
                var expiredTokens = await _unitOfWork.UserRefreshTokenRepository
                    .GetAllAsync(x => x.UserId == user.Id && (x.IsRevoked || x.ExpiryDate <= DateTime.Now))
                    .ToListAsync(cancellationToken);

                foreach (var token in expiredTokens)
                    token.Revoke();

                refreshToken = _jwtTokenService.GenerateRefreshToken(user);
                var userRefreshToken = UserRefreshToken.Create(user.Id, refreshToken, DateTime.Now.AddDays(_jwtSettings.RefreshTokenExpiryDays));
                await _unitOfWork.UserRefreshTokenRepository.AddAsync(userRefreshToken);
                await _unitOfWork.SaveChangesAsync();
            }

            var fcmTokenIsEmpty = string.IsNullOrWhiteSpace(request.FcmToken);
            if (!fcmTokenIsEmpty && request.DevicePlatform.HasValue)
            {
                _logger.LogInformation("Registering FCM token for user {UserId} on platform {Platform} (token length: {TokenLength}).",
                    user.Id, request.DevicePlatform.Value, request.FcmToken!.Length);
                await _fcmService.RegisterTokenAsync(user.Id, request.FcmToken, request.DevicePlatform.Value);
                _logger.LogInformation("FCM token registered successfully for user {UserId} on platform {Platform}.", user.Id, request.DevicePlatform.Value);
            }
            else
            {
                _logger.LogWarning("FCM token NOT registered for user {UserId}. FcmTokenEmpty={FcmTokenEmpty}, DevicePlatformProvided={DevicePlatformProvided} (value: {DevicePlatformValue}).",
                    user.Id, fcmTokenIsEmpty, request.DevicePlatform.HasValue, request.DevicePlatform?.ToString() ?? "null");
            }

            var isFreelanceDoctor = await _unitOfWork.DoctorRepository
                .GetAllAsync(d => d.UserId == user.Id)
                .Select(d => (bool?)d.IsFreelance)
                .FirstOrDefaultAsync(cancellationToken) ?? false;

            var authData = new AuthResponseDto(accessToken, refreshToken, user.FullName, user.Email!, UserTypeHelper.GetPrimaryRole(roles), user.Id, clinicId, doctorId, user.ProfilePictureUrl, isFreelanceDoctor);

            var isDashboardUser = roles.Any(r => r == nameof(UserType.ClinicOwner) || r == nameof(UserType.Staff) || r == nameof(UserType.Doctor));
            if (isDashboardUser && clinicId.HasValue && !hasActiveSubscription)
                throw new ForbiddenException(_localizer[LocalizationKeys.SubscriptionMessages.LoginRequiresSubscription.Value], authData);

            return authData;
        }
    }
}
