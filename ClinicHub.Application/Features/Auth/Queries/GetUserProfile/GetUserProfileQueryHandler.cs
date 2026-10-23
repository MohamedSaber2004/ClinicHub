using ClinicHub.Application.Common;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Auth.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Auth.Queries.GetUserProfile
{
    public sealed class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public GetUserProfileQueryHandler(
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
        }

        public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());
            var roles = await _userManager.GetRolesAsync(user!);

            var isFreelanceDoctor = await _unitOfWork.DoctorRepository
                .GetAllAsync(d => d.UserId == user!.Id)
                .Select(d => (bool?)d.IsFreelance)
                .FirstOrDefaultAsync(cancellationToken) ?? false;

            bool? isCompleteProfile = null;

            if (roles.Contains(UserType.ClinicOwner.ToString()))
            {
                var clinicId = user!.ClinicId;

                if (!clinicId.HasValue)
                {
                    clinicId = await _unitOfWork.ClinicRepository
                        .GetAllAsync(c => c.ClinicAdminId == user.Id && !c.IsDeleted)
                        .Select(c => (Guid?)c.Id)
                        .FirstOrDefaultAsync(cancellationToken);
                }

                if (!clinicId.HasValue)
                {
                    clinicId = await _unitOfWork.DoctorRepository
                        .GetAllAsync(d => d.UserId == user.Id && d.ClinicId != null && !d.IsDeleted)
                        .Select(d => (Guid?)d.ClinicId)
                        .FirstOrDefaultAsync(cancellationToken);
                }

                if (!clinicId.HasValue)
                    clinicId = _currentUserService.CurrentClinicId;

                if (!clinicId.HasValue)
                {
                    isCompleteProfile = false;
                }
                else
                {
                    var hasBookingConfiguration = await _unitOfWork.BookingConfigurationRepository
                        .ExistsAsync(bc => bc.ClinicId == clinicId.Value && !bc.IsDeleted, cancellationToken);

                    var hasClinicAvailability = await _unitOfWork.DoctorAvailabilityRepository
                        .ExistsAsync(a => a.ClinicId == clinicId.Value && !a.IsDeleted, cancellationToken);

                    isCompleteProfile = hasBookingConfiguration && hasClinicAvailability;
                }
            }

            return new UserProfileDto(
                user!.Id,
                user.FullName,
                user.Email!,
                user.Gender,
                user.PhoneNumber ?? string.Empty,
                DateOnly.FromDateTime(user.BirthDate ?? DateTime.MinValue),
                user.ProfilePictureUrl,
                user.Language,
                UserTypeHelper.GetPrimaryRole(roles),
                isFreelanceDoctor,
                isCompleteProfile);
        }
    }
}
