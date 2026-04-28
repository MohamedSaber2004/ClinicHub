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
using Microsoft.Extensions.Options;

namespace ClinicHub.Application.Features.Auth.Commands.Signup
{
    public sealed class SignupCommandHandler : IRequestHandler<SignupCommand, AuthResponseDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly JwtSettings _jwtSettings;
        private readonly EmailSettings _emailSettings;
        private readonly IUnitOfWork _unitOfWork;

        public SignupCommandHandler(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IJwtTokenService jwtTokenService,
            IOptions<JwtSettings> jwtSettings,
            IOptions<EmailSettings> emailSettings,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _emailService = emailService;
            _jwtTokenService = jwtTokenService;
            _jwtSettings = jwtSettings.Value;
            _emailSettings = emailSettings.Value;
            _unitOfWork = unitOfWork;
        }

        public async Task<AuthResponseDto> Handle(SignupCommand request, CancellationToken cancellationToken)
        {
            var user = ApplicationUser.Create(
                request.FullName, 
                request.Email, 
                request.PhoneNumber, 
                request.BirthDate,
                request.Gender);

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                throw new UnAuthorizedException(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ExceptionMessages.Validation.Value));
            }

            await _userManager.AddToRoleAsync(user, UserType.User.ToString());

            var verificationCode = new Random().Next(100000, 999999).ToString();
            user.SetVerificationCode(verificationCode, DateTime.UtcNow.AddMinutes(_emailSettings.VerificationCodeExpiryMinutes));
            
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);
            var refreshToken = _jwtTokenService.GenerateRefreshToken(user);

            var userRefreshToken = UserRefreshToken.Create(user.Id, refreshToken, DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays));
            await _unitOfWork.UserRefreshTokenRepository.AddAsync(userRefreshToken);
            await _unitOfWork.SaveChangesAsync();

            await _emailService.SendVerificationEmailAsync(user.Email!, user.FullName, verificationCode, cancellationToken);

            return new AuthResponseDto(accessToken, refreshToken, user.FullName, user.Email!, user.Id, verificationCode);
        }
    }
}
