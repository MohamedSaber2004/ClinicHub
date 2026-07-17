using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Auth.Commands.UpdateProfile
{
    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProfileCommandHandler(
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());

            if (user == null)
                return false;

            if (request.FullName != null)
                user.UpdateFullName(request.FullName);

            if (request.PhoneNumber != null)
                user.UpdatePhoneNumber(request.PhoneNumber);

            if (request.BirthDate.HasValue)
                user.UpdateBirthDate(request.BirthDate.Value);

            if (request.Gender.HasValue)
                user.UpdateGender(request.Gender.Value);

            var verification = await _unitOfWork.UserVerificationRepository
                .GetAllAsync(v => v.UserId == user.Id && !v.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (request.ProfileImageUrl != null)
            {
                user.UpdateProfilePicture(request.ProfileImageUrl);

                if (verification != null)
                    verification.UpdateDoctorImage(request.ProfileImageUrl);
            }
            else if (verification?.DoctorImage != null)
            {
                user.UpdateProfilePicture(verification.DoctorImage);
            }

            var result = await _userManager.UpdateAsync(user);

            if (verification != null)
                await _unitOfWork.SaveChangesAsync();

            return result.Succeeded;
        }
    }
}
