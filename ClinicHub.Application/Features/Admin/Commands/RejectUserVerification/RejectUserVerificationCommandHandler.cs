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

namespace ClinicHub.Application.Features.Admin.Commands.RejectUserVerification
{
    public sealed class RejectUserVerificationCommandHandler : IRequestHandler<RejectUserVerificationCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public RejectUserVerificationCommandHandler(
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

        public async Task<bool> Handle(RejectUserVerificationCommand request, CancellationToken cancellationToken)
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

            verification.Reject(_currentUserService.UserId, request.Notes);
            user.IsActive = false;

            if (verification.RequestedRole == UserType.ClinicOwner)
            {
                var clinic = await _unitOfWork.ClinicRepository
                    .GetAllAsync(c => (c.ClinicAdminId == user.Id || (user.ClinicId.HasValue && c.Id == user.ClinicId.Value)) && !c.IsDeleted && c.Status == ClinicStatus.PendingApproval)
                    .FirstOrDefaultAsync(cancellationToken);

                if (clinic != null)
                {
                    clinic.Status = ClinicStatus.Inactive;
                    clinic.Deactive();
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
