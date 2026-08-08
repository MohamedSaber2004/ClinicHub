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

namespace ClinicHub.Application.Features.Admin.Commands.RejectClinic
{
    public class RejectClinicCommandHandler : IRequestHandler<RejectClinicCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly IFcmService _fcmService;
        private readonly ILogger<RejectClinicCommandHandler> _logger;

        public RejectClinicCommandHandler(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IStringLocalizer<Messages> localizer,
            IFcmService fcmService,
            ILogger<RejectClinicCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _localizer = localizer;
            _fcmService = fcmService;
            _logger = logger;
        }

        public async Task<bool> Handle(RejectClinicCommand request, CancellationToken cancellationToken)
        {
            var clinic = await _unitOfWork.ClinicRepository.FindByKeyAsync(request.ClinicId);
            if (clinic == null)
                throw new NotFoundException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

            if (clinic.Status != ClinicStatus.PendingApproval)
                throw new BadRequestException("Clinic is not pending approval.");

            clinic.Status = ClinicStatus.Inactive;
            clinic.Deactive();

            if (clinic.ClinicAdminId.HasValue)
            {
                var admin = await _userManager.FindByIdAsync(clinic.ClinicAdminId.Value.ToString());
                if (admin != null)
                {
                    admin.IsActive = false;
                    await _userManager.UpdateAsync(admin);
                }

                var pendingVerification = await _unitOfWork.UserVerificationRepository
                    .GetAllAsync(v => v.UserId == clinic.ClinicAdminId.Value && !v.IsDeleted && v.Status == VerificationStatus.Pending)
                    .FirstOrDefaultAsync(cancellationToken);

                if (pendingVerification != null)
                {
                    pendingVerification.Reject(Guid.Empty, request.Reason ?? "Clinic registration rejected.");
                }
            }

            await _unitOfWork.SaveChangesAsync();

            await NotifyClinicOwnerAsync(clinic, request.Reason ?? "Clinic registration rejected.");

            return true;
        }

        private async Task NotifyClinicOwnerAsync(Clinic clinic, string reason)
        {
            if (!clinic.ClinicAdminId.HasValue)
                return;

            try
            {
                await _fcmService.SendToUserAsync(clinic.ClinicAdminId.Value, NotificationType.ClinicRejected, new()
                {
                    ["clinicName"] = clinic.Name,
                    ["reason"] = reason
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send clinic-rejected notification for clinic {ClinicId}.", clinic.Id);
            }
        }
    }
}
