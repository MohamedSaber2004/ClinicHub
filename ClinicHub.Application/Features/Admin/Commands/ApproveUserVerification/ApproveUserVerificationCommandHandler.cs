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
        private readonly IEmailService _emailService;
        private readonly IStringLocalizer<Messages> _localizer;

        public ApproveUserVerificationCommandHandler(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService,
            IEmailService emailService,
            IStringLocalizer<Messages> localizer)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _currentUserService = currentUserService;
            _emailService = emailService;
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

            if (verification.RequestedRole == UserType.ClinicOwner)
            {
                var clinic = await _unitOfWork.ClinicRepository
                    .GetAllAsync(c => (c.ClinicAdminId == user.Id || (user.ClinicId.HasValue && c.Id == user.ClinicId.Value)) && !c.IsDeleted && c.Status == ClinicStatus.PendingApproval)
                    .FirstOrDefaultAsync(cancellationToken);

                if (clinic != null)
                {
                    clinic.Status = ClinicStatus.Active;
                    clinic.Active();
                }
            }

            if (verification is { SpecializationId: not null, RequestedRole: UserType.Doctor or UserType.ClinicOwner })
            {
                var existingDoctor = await _unitOfWork.DoctorRepository
                    .GetAllAsync(d => d.UserId == user.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingDoctor == null)
                {
                    var doctor = new Doctor(
                        user.Id,
                        verification.SpecializationId.Value,
                        verification.Bio ?? string.Empty,
                        verification.YearsOfExperience ?? 0);

                    await _unitOfWork.DoctorRepository.AddAsync(doctor);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            await _emailService.SendVerificationApprovedAsync(
                user.Email!, user.FullName, user.Id.ToString(), roleName, cancellationToken);

            return true;
        }
    }
}
