using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Admin.Commands.ApproveUserVerification
{
    public sealed class ApproveUserVerificationCommandHandler : IRequestHandler<ApproveUserVerificationCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public ApproveUserVerificationCommandHandler(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<bool> Handle(ApproveUserVerificationCommand request, CancellationToken cancellationToken)
        {
            var verification = await _unitOfWork.UserVerificationRepository
                .GetAllAsync(v => v.UserId == request.UserId && !v.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (verification == null)
                throw new NotFoundException(_localizer[LocalizationKeys.ExceptionMessages.NotFound.Value]);

            if (verification.Status != VerificationStatus.Pending)
                throw new BadRequestException(_localizer[LocalizationKeys.AuthMessages.AlreadyReviewed.Value]);

            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                throw new NotFoundException(_localizer[LocalizationKeys.ExceptionMessages.NotFound.Value]);

            verification.Approve(_currentUserService.UserId);
            user.IsActive = true;

            var roleName = verification.RequestedRole.ToString();
            var existingRoles = await _userManager.GetRolesAsync(user);
            if (existingRoles.Count > 0)
                await _userManager.RemoveFromRolesAsync(user, existingRoles);

            await _userManager.AddToRoleAsync(user, roleName);

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
