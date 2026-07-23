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

namespace ClinicHub.Application.Features.Auth.Commands.Login
{
    public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly JwtSettings _jwtSettings;
        private readonly EmailSettings _emailSettings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFcmService _fcmService;
        private readonly IStringLocalizer<Messages> _localizer;

        public LoginCommandHandler(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IJwtTokenService jwtTokenService,
            IOptions<JwtSettings> jwtSettings,
            IOptions<EmailSettings> emailSettings,
            IUnitOfWork unitOfWork,
            IFcmService fcmService,
            IStringLocalizer<Messages> localizer)
        {
            _userManager = userManager;
            _emailService = emailService;
            _jwtTokenService = jwtTokenService;
            _jwtSettings = jwtSettings.Value;
            _emailSettings = emailSettings.Value;
            _unitOfWork = unitOfWork;
            _fcmService = fcmService;
            _localizer = localizer;
        }

        public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
                throw new UnAuthorizedException(_localizer[LocalizationKeys.AuthMessages.InvalidCredentials.Value]);

            if (!user.IsActive)
                throw new ForbiddenException(_localizer[LocalizationKeys.AuthMessages.AccountPendingApproval.Value]);

            var hasPendingVerification = await _unitOfWork.UserVerificationRepository
                .GetAllAsync(v => v.UserId == user.Id && !v.IsDeleted && v.Status == Domain.Enums.VerificationStatus.Pending)
                .AnyAsync(cancellationToken);

            if (hasPendingVerification)
                throw new ForbiddenException(_localizer[LocalizationKeys.AuthMessages.AccountPendingApproval.Value]);

            var roles = await _userManager.GetRolesAsync(user);
            var clinicId = await _unitOfWork.ClinicRepository
                .GetAllAsync(c => c.ClinicAdminId == user.Id && !c.IsDeleted)
                .Select(c => (Guid?)c.Id)
                .FirstOrDefaultAsync(cancellationToken);
            var hasActiveSubscription = clinicId.HasValue
                && await _unitOfWork.GetRepository<Subscription, Guid>()
                    .ExistsAsync(s => s.ClinicId == clinicId.Value && s.Status == SubscriptionStatus.Active && s.EndDate > DateTime.UtcNow, cancellationToken);
            var accessToken = _jwtTokenService.GenerateAccessToken(user, roles, clinicId, hasActiveSubscription);

            var existingToken = await _unitOfWork.UserRefreshTokenRepository
                .GetFirstAsync(x => x.UserId == user.Id && !x.IsRevoked && x.ExpiryDate > DateTime.UtcNow, cancellationToken);

            string refreshToken;
            if (existingToken != null)
            {
                refreshToken = existingToken.Token;
            }
            else
            {
                var expiredTokens = await _unitOfWork.UserRefreshTokenRepository
                    .GetAllAsync(x => x.UserId == user.Id && (x.IsRevoked || x.ExpiryDate <= DateTime.UtcNow))
                    .ToListAsync(cancellationToken);

                foreach (var token in expiredTokens)
                    token.Revoke();

                refreshToken = _jwtTokenService.GenerateRefreshToken(user);
                var userRefreshToken = UserRefreshToken.Create(user.Id, refreshToken, DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays));
                await _unitOfWork.UserRefreshTokenRepository.AddAsync(userRefreshToken);
                await _unitOfWork.SaveChangesAsync();
            }

            if (!string.IsNullOrEmpty(request.FcmToken) && request.DevicePlatform.HasValue)
                await _fcmService.RegisterTokenAsync(user.Id, request.FcmToken, request.DevicePlatform.Value);

            var isFreelanceDoctor = await _unitOfWork.DoctorRepository
                .GetAllAsync(d => d.UserId == user.Id)
                .Select(d => (bool?)d.IsFreelance)
                .FirstOrDefaultAsync(cancellationToken) ?? false;

            return new AuthResponseDto(accessToken, refreshToken, user.FullName, user.Email!, roles.FirstOrDefault(), user.Id, clinicId, user.ProfilePictureUrl, isFreelanceDoctor);
        }
    }
}
