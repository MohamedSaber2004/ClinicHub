using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Users.Commands.AddUser
{
    public class AddUserCommandHandler : IRequestHandler<AddUserCommand, Guid>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly IUnitOfWork _unitOfWork;

        public AddUserCommandHandler(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IStringLocalizer<Messages> localizer,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _localizer = localizer;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(AddUserCommand request, CancellationToken cancellationToken)
        {
            var user = ApplicationUser.Create(
                request.FullName, 
                request.Email, 
                request.PhoneNumber, 
                request.BirthDate,
                request.Gender);

            if (request.ClinicId.HasValue)
            {
                var clinic = await _unitOfWork.ClinicRepository.GetByIdAsync(request.ClinicId.Value);
                if (clinic == null)
                    throw new NotFoundException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

                user.AssignToClinic(request.ClinicId.Value);
            }

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BadRequestException(errors);
            }

            if (request.Role != UserType.None)
            {
                var roleName = request.Role.ToString();
                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (roleExists)
                {
                    await _userManager.AddToRoleAsync(user, roleName);
                }
            }

            if (request.ClinicId.HasValue && request.SpecializationId.HasValue &&
                (request.Role == UserType.Doctor || request.Role == UserType.ClinicOwner))
            {
                var existingDoctor = await _unitOfWork.DoctorRepository
                    .GetAllAsync(d => d.UserId == user.Id && d.ClinicId == request.ClinicId.Value)
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingDoctor == null)
                {
                    var doctor = new Doctor(
                        user.Id,
                        request.ClinicId.Value,
                        request.SpecializationId.Value,
                        request.Bio ?? string.Empty,
                        request.YearsOfExperience ?? 0);

                    await _unitOfWork.DoctorRepository.AddAsync(doctor);
                    await _unitOfWork.SaveChangesAsync();
                }
            }

            return user.Id;
        }
    }
}
