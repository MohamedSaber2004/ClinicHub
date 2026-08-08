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
using Microsoft.Extensions.Logging;

namespace ClinicHub.Application.Features.Admin.Commands.ApproveClinic
{
    public class ApproveClinicCommandHandler : IRequestHandler<ApproveClinicCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUser;
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly IFcmService _fcmService;
        private readonly ILogger<ApproveClinicCommandHandler> _logger;

        public ApproveClinicCommandHandler(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUser,
            IStringLocalizer<Messages> localizer,
            IFcmService fcmService,
            ILogger<ApproveClinicCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _currentUser = currentUser;
            _localizer = localizer;
            _fcmService = fcmService;
            _logger = logger;
        }

        public async Task<bool> Handle(ApproveClinicCommand request, CancellationToken cancellationToken)
        {
            var clinic = await _unitOfWork.ClinicRepository.FindByKeyAsync(request.ClinicId);
            if (clinic == null)
                throw new NotFoundException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

            if (clinic.Status != ClinicStatus.PendingApproval)
                throw new BadRequestException("Clinic is not pending approval.");

            clinic.Status = ClinicStatus.Active;
            clinic.Active();

            if (clinic.ClinicAdminId.HasValue)
            {
                var admin = await _userManager.FindByIdAsync(clinic.ClinicAdminId.Value.ToString());
                if (admin != null)
                {
                    admin.IsActive = true;
                    await _userManager.UpdateAsync(admin);
                }

                var pendingVerification = await _unitOfWork.UserVerificationRepository
                    .GetAllAsync(v => v.UserId == clinic.ClinicAdminId.Value && !v.IsDeleted && v.Status == VerificationStatus.Pending)
                    .FirstOrDefaultAsync(cancellationToken);

                if (pendingVerification != null)
                {
                    pendingVerification.Approve(_currentUser.UserId);
                }
            }

            await NotifyClinicOwnerAsync(clinic);

            // Single commit: clinic state, admin activation, verification, and the
            // notification row in one transaction.
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private async Task NotifyClinicOwnerAsync(Clinic clinic)
        {
            if (!clinic.ClinicAdminId.HasValue)
                return;

            try
            {
                await _fcmService.SendToUserAsync(clinic.ClinicAdminId.Value, NotificationType.ClinicApproved, new()
                {
                    ["clinicName"] = clinic.Name,
                    ["clinicId"] = clinic.Id.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send clinic-approved notification for clinic {ClinicId}.", clinic.Id);
            }
        }
    }
}
