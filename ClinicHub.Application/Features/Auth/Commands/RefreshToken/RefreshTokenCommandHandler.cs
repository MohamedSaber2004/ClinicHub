using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Auth.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using ClinicHub.Application.Common.Options;

namespace ClinicHub.Application.Features.Auth.Commands.RefreshToken
{
    public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponseDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly JwtSettings _jwtSettings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStringLocalizer<Messages> _localizer;

        public RefreshTokenCommandHandler(
            UserManager<ApplicationUser> userManager,
            IJwtTokenService jwtTokenService,
            IOptions<JwtSettings> jwtSettings,
            IUnitOfWork unitOfWork,
            IStringLocalizer<Messages> localizer)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _jwtSettings = jwtSettings.Value;
            _unitOfWork = unitOfWork;
            _localizer = localizer;
        }

        public async Task<RefreshTokenResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var tokenEntity = await _unitOfWork.UserRefreshTokenRepository
                .GetAllWithIncluding(t => t.Token == request.RefreshToken, t => t.User)
                .FirstOrDefaultAsync(cancellationToken);

            var user = tokenEntity!.User;
            if (!user.IsActive)
                throw new UnAuthorizedException(_localizer[LocalizationKeys.AuthMessages.AccountPendingApproval.Value]);

            tokenEntity.Revoke();

            var roles = await _userManager.GetRolesAsync(user);
            var clinicId = user.ClinicId;
            if (!clinicId.HasValue)
            {
                clinicId = await _unitOfWork.ClinicRepository
                    .GetAllAsync(c => c.ClinicAdminId == user.Id && !c.IsDeleted)
                    .Select(c => (Guid?)c.Id)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            var isDashboardUser = roles.Any(r => r == nameof(UserType.ClinicOwner) || r == nameof(UserType.Staff) || r == nameof(UserType.Doctor));
            if (isDashboardUser && clinicId.HasValue)
            {
                var hasActiveSub = await _unitOfWork.GetRepository<Subscription, Guid>()
                    .ExistsAsync(s => s.ClinicId == clinicId.Value && s.Status == SubscriptionStatus.Active && s.EndDate > DateTime.UtcNow, cancellationToken);

                if (!hasActiveSub)
                    throw new ForbiddenException(_localizer[LocalizationKeys.SubscriptionMessages.LoginRequiresSubscription.Value]);
            }

            var hasActiveSubscription = clinicId.HasValue
                && await _unitOfWork.GetRepository<Subscription, Guid>()
                    .ExistsAsync(s => s.ClinicId == clinicId.Value && s.Status == SubscriptionStatus.Active && s.EndDate > DateTime.UtcNow, cancellationToken);
            var newAccessToken = _jwtTokenService.GenerateAccessToken(user, roles, clinicId, hasActiveSubscription);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken(user);

            var newTokenEntity = UserRefreshToken.Create(user.Id, newRefreshToken, DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays));
            await _unitOfWork.UserRefreshTokenRepository.AddAsync(newTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            return new RefreshTokenResponseDto(newAccessToken, newRefreshToken);
        }
    }
}
