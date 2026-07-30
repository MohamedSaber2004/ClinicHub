using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ClinicHub.Application.Features.ClinicStaff.Commands.CreateStaff
{
    public class CreateStaffCommandHandler : IRequestHandler<CreateStaffCommand, Guid>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateStaffCommandHandler(ICurrentUserService currentUserService, UserManager<ApplicationUser> userManager)
        {
            _currentUserService = currentUserService;
            _userManager = userManager;
        }

        public async Task<Guid> Handle(CreateStaffCommand request, CancellationToken cancellationToken)
        {
            var clinicId = _currentUserService.CurrentClinicId
                ?? throw new InvalidOperationException("ClinicOwner must have a clinic assigned.");

            var user = ApplicationUser.Create(
                request.FullName,
                request.Email,
                request.PhoneNumber,
                null,
                null);

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

            if (!string.IsNullOrWhiteSpace(request.Image))
                user.UpdateProfilePicture(request.Image);

            user.AssignToClinic(clinicId);
            await _userManager.UpdateAsync(user);

            await _userManager.AddToRoleAsync(user, nameof(UserType.Staff));

            return user.Id;
        }
    }
}
