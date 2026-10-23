using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Clinics.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NetTopologySuite.Geometries;

namespace ClinicHub.Application.Features.Clinics.Commands.SetupClinic
{
    public sealed class SetupClinicCommandHandler : IRequestHandler<SetupClinicCommand, ClinicManagementDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public SetupClinicCommandHandler(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<ClinicManagementDto> Handle(SetupClinicCommand request, CancellationToken cancellationToken)
        {

            var user = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());
            if (user == null)
                throw new NotFoundException(_localizer[LocalizationKeys.ExceptionMessages.NotFound.Value]);

            var createdBy = _currentUserService.IsAuthenticated
                ? _currentUserService.UserId.ToString()
                : "system";

            // A clinic owner coming from the register -> admin-approval flow ALREADY owns
            // a clinic. Creating another one here orphans the approved record (ghost
            // duplicate on maps, login resolving a different clinic than the dashboard
            // uses -> "clinic not found"). Complete the existing clinic in place instead.
            var existingClinic = await FindOwnerClinicAsync(user, cancellationToken);

            if (existingClinic != null)
            {
                var locationPoint = new Point(request.Lng, request.Lat) { SRID = 4326 };

                existingClinic.UpdateDetails(
                    request.Name,
                    request.Name,
                    request.Description,
                    request.Description,
                    request.Address,
                    request.Address,
                    request.Phone,
                    request.Email,
                    request.Website,
                    request.Logo,
                    request.WorkingHours,
                    request.SpecializationId,
                    createdBy,
                    request.WorkingHoursStart,
                    request.WorkingHoursEnd,
                    request.WorkingDays != null ? string.Join(",", request.WorkingDays) : null,
                    locationPoint);

                existingClinic.IsSetupComplete = true;

                _unitOfWork.ClinicRepository.Update(existingClinic);

                await EnsureDoctorAndRoleAsync(user, existingClinic.Id, request.SpecializationId, createdBy, cancellationToken);

                user.AssignToClinic(existingClinic.Id);

                await _unitOfWork.SaveChangesAsync();

                return _mapper.Map<ClinicManagementDto>(existingClinic);
            }

            var clinic = new Clinic
            {
                Name = request.Name,
                NameAr = request.Name,
                Description = request.Description,
                ArDescription = request.Description,
                Address = request.Address,
                AddressAr = request.Address,
                Phone = request.Phone,
                Email = request.Email,
                Website = request.Website,
                Logo = request.Logo,
                WorkingHours = request.WorkingHours,
                WorkingHoursStart = request.WorkingHoursStart,
                WorkingHoursEnd = request.WorkingHoursEnd,
                WorkingDays = request.WorkingDays != null ? string.Join(",", request.WorkingDays) : null,
                SpecializationId = request.SpecializationId,
                Location = new Point(request.Lng, request.Lat) { SRID = 4326 },
                IsRegistered = true,
                Status = ClinicStatus.Active,
                ClinicAdminId = user.Id
            };
            clinic.MarkAsCreated(createdBy);

            await _unitOfWork.ClinicRepository.AddAsync(clinic);

            await EnsureDoctorAndRoleAsync(user, clinic.Id, request.SpecializationId, createdBy, cancellationToken);

            clinic.IsSetupComplete = true;

            user.AssignToClinic(clinic.Id);

            await _unitOfWork.SaveChangesAsync();

            var clinicDto = _mapper.Map<ClinicManagementDto>(clinic);
            return clinicDto;
        }

        private async Task<Clinic?> FindOwnerClinicAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            // Prefer the clinic already linked to the user, then any non-deleted
            // clinic they administer (the register -> approval flow sets both).
            if (user.ClinicId.HasValue)
            {
                var linked = await _unitOfWork.ClinicRepository
                    .GetAllAsync(c => c.Id == user.ClinicId.Value && !c.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);
                if (linked != null)
                    return linked;
            }

            return await _unitOfWork.ClinicRepository
                .GetAllAsync(c => c.ClinicAdminId == user.Id && !c.IsDeleted)
                .OrderBy(c => c.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task EnsureDoctorAndRoleAsync(ApplicationUser user, Guid clinicId, Guid specializationId, string createdBy, CancellationToken cancellationToken)
        {
            var existingDoctor = await _unitOfWork.DoctorRepository
                .GetAllAsync(d => d.UserId == user.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingDoctor == null)
            {
                var doctor = new Doctor(
                    user.Id,
                    clinicId,
                    specializationId,
                    string.Empty,
                    0);
                doctor.MarkAsCreated(createdBy);
                await _unitOfWork.DoctorRepository.AddAsync(doctor);
            }
            else
            {
                existingDoctor.AssignToClinic(clinicId);
            }

            // The clinic owner is also a doctor (الطبيب المسؤول): grant the Doctor role
            // so the owner can use the doctor dashboard.
            var existingRoles = await _userManager.GetRolesAsync(user);
            if (!existingRoles.Contains(nameof(UserType.Doctor)))
            {
                var doctorRoleResult = await _userManager.AddToRoleAsync(user, nameof(UserType.Doctor));
                if (!doctorRoleResult.Succeeded)
                    throw new BadRequestException(_localizer[LocalizationKeys.AuthMessages.RoleAssignmentFailed.Value]);
            }
        }
    }
}
