using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Users.Commands.DeleteUser
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IClinicHubContext _context;
        private readonly ICurrentUserService _currentUserService;

        public DeleteUserCommandHandler(UserManager<ApplicationUser> userManager, IClinicHubContext context, ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.Id.ToString());

            if (user == null || user.IsDeleted)
            {
                throw new NotFoundException(nameof(ApplicationUser), request.Id);
            }

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BadRequestException(errors);
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Any(r => r == nameof(UserType.Doctor) || r == nameof(UserType.ClinicOwner)))
            {
                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.UserId == user.Id && !d.IsDeleted, cancellationToken);

                if (doctor is not null)
                {
                    doctor.MarkAsDeleted(_currentUserService.UserId.ToString());
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
