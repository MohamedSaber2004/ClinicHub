using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Users.Commands.EditUser
{
    public class EditUserCommandHandler : IRequestHandler<EditUserCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IClinicHubContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public EditUserCommandHandler(
            UserManager<ApplicationUser> userManager,
            IClinicHubContext context,
            IStringLocalizer<Messages> localizer)
        {
            _userManager = userManager;
            _context = context;
            _localizer = localizer;
        }

        public async Task<bool> Handle(EditUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

            if (user == null)
                throw new NotFoundException(nameof(ApplicationUser), request.Id);

            user.UpdateProfile(request.FullName!, request.PhoneNumber!, request.BirthDate, request.Gender);

            if (request.IsActive.HasValue)
            {
                user.IsActive = request.IsActive.Value;
                user.IsDeleted = !request.IsActive.Value;
            }
            
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BadRequestException(errors);
            }

            if (request.IsActive.HasValue)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Any(r => r == nameof(UserType.Doctor) || r == nameof(UserType.ClinicOwner)))
                {
                    var doctor = await _context.Doctors
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(d => d.UserId == user.Id, cancellationToken);

                    if (doctor is not null)
                    {
                        if (request.IsActive.Value)
                            doctor.Active();
                        else
                            doctor.Deactive();

                        await _context.SaveChangesAsync(cancellationToken);
                    }
                }
            }

            return true;
        }
    }
}
