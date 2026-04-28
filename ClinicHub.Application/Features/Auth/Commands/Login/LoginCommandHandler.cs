using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Auth.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
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

        public LoginCommandHandler(
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

        public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            
            // Generate Access Token
            var roles = await _userManager.GetRolesAsync(user!);
            var accessToken = _jwtTokenService.GenerateAccessToken(user!, roles);

            // Check for existing valid refresh token
            var existingToken = await _unitOfWork.UserRefreshTokenRepository
                .GetFirstAsync(x => x.UserId == user!.Id && !x.IsRevoked && x.ExpiryDate > DateTime.UtcNow, cancellationToken);

            string refreshToken;
            if (existingToken != null)
            {
                refreshToken = existingToken.Token;
            }
            else
            {
                refreshToken = _jwtTokenService.GenerateRefreshToken(user!);
                var userRefreshToken = UserRefreshToken.Create(user!.Id, refreshToken, DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays));
                await _unitOfWork.UserRefreshTokenRepository.AddAsync(userRefreshToken);
                await _unitOfWork.SaveChangesAsync();
            }

            return new AuthResponseDto(accessToken, refreshToken, user!.FullName, user.Email!, user.Id, null);
        }
    }
}
