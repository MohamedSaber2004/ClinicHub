using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ClinicHub.Application.Features.Auth.Commands.UpdateProfile
{
    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUserService;

        public UpdateProfileCommandHandler(
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _currentUserService = currentUserService;
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

            if (request.ProfileImageUrl != null)
                user.UpdateProfilePicture(request.ProfileImageUrl);

            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded;
        }
    }
}
