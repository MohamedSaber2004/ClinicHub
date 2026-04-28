using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
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

namespace ClinicHub.Application.Features.Auth.Commands.LoginWithGoogle
{
    public sealed class LoginWithGoogleCommandHandler : IRequestHandler<LoginWithGoogleCommand, AuthResponseDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IGoogleAuth _googleAuth;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtSettings _jwtSettings;
        private readonly IStringLocalizer<Messages> _localizer;

        public LoginWithGoogleCommandHandler(
            UserManager<ApplicationUser> userManager,
            IGoogleAuth googleAuth,
            IJwtTokenService jwtTokenService,
            IUnitOfWork unitOfWork,
            IOptions<JwtSettings> jwtSettings,
            IStringLocalizer<Messages> localizer)
        {
            _userManager = userManager;
            _googleAuth = googleAuth;
            _jwtTokenService = jwtTokenService;
            _unitOfWork = unitOfWork;
            _jwtSettings = jwtSettings.Value;
            _localizer = localizer;
        }

        public async Task<AuthResponseDto> Handle(LoginWithGoogleCommand request, CancellationToken cancellationToken)
        {
            var payload = await _googleAuth.ValidateGoogleTokenAsync(request.IdToken, cancellationToken);
            if (payload == null)
                throw new UnAuthorizedException(_localizer[LocalizationKeys.AuthMessages.InvalidGoogleToken.Value]);

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.GoogleUserId == payload.Subject, cancellationToken);

            if (user == null)
            {
                var email = payload.Email;
                if (string.IsNullOrEmpty(email))
                {
                    throw new BadRequestException(_localizer[LocalizationKeys.AuthMessages.GoogleEmailRequired.Value]);
                }

                user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    user = ApplicationUser.Create(
                        payload.Name ?? payload.GivenName ?? email,
                        email,
                        string.Empty,
                        DateTime.Now.AddYears(-15),
                        Gender.other
                    );
                    user.SetGoogleUserId(payload.Subject);

                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                        throw new BadRequestException(_localizer[LocalizationKeys.AuthMessages.GoogleUserCreationFailed.Value]);

                    await _userManager.AddToRoleAsync(user, UserType.User.ToString());
                }
                else
                {
                    user.SetGoogleUserId(payload.Subject);
                    await _userManager.UpdateAsync(user);
                }
            }

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);

            var existingToken = await _unitOfWork.UserRefreshTokenRepository
                .GetFirstAsync(x => x.UserId == user.Id && !x.IsRevoked && x.ExpiryDate > DateTime.UtcNow, cancellationToken);

            string refreshToken;
            if (existingToken != null)
            {
                refreshToken = existingToken.Token;
            }
            else
            {
                refreshToken = _jwtTokenService.GenerateRefreshToken(user);
                var userRefreshToken = UserRefreshToken.Create(user.Id, refreshToken, DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays));
                await _unitOfWork.UserRefreshTokenRepository.AddAsync(userRefreshToken);
                await _unitOfWork.SaveChangesAsync();
            }

            return new AuthResponseDto(accessToken, refreshToken, user.FullName, user.Email!, user.Id, null);
        }
    }
}
