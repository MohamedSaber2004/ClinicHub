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
using Microsoft.Extensions.Localization;
using NetTopologySuite.Geometries;

namespace ClinicHub.Application.Features.Clinics.Commands.CreateClinic
{
    public class CreateClinicCommandHandler : IRequestHandler<CreateClinicCommand, ClinicManagementDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ICurrentUserService _currentUserService;

        public CreateClinicCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IStringLocalizer<Messages> localizer,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizer = localizer;
            _userManager = userManager;
            _emailService = emailService;
            _currentUserService = currentUserService;
        }

        public async Task<ClinicManagementDto> Handle(CreateClinicCommand request, CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var clinic = new Clinic
                {
                    Name = request.NameAr,
                    NameAr = request.NameAr,
                    Description = request.Description,
                    ArDescription = request.ArDescription,
                    Address = request.Address,
                    AddressAr = request.AddressAr,
                    Phone = request.Phone,
                    Email = request.Email,
                    Website = request.Website,
                    Logo = request.Logo,
                    WorkingHours = request.WorkingHours,
                    WorkingHoursStart = request.WorkingHoursStart,
                    WorkingHoursEnd = request.WorkingHoursEnd,
                    WorkingDays = request.WorkingDays != null ? string.Join(",", request.WorkingDays) : null,
                    SpecializationId = request.SpecializationId,
                    Location = request.Lat.HasValue && request.Lng.HasValue
                        ? new Point(request.Lng.Value, request.Lat.Value) { SRID = 4326 }
                        : new Point(0, 0) { SRID = 4326 },
                    IsRegistered = true,
                    Status = ClinicStatus.Active
                };

                var tempPassword = GenerateRandomPassword(12);

                var user = ApplicationUser.Create(request.OwnerName, request.OwnerEmail, request.OwnerPhone ?? string.Empty, null, null);
                user.EmailConfirmed = true;

                var createResult = await _userManager.CreateAsync(user, tempPassword);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    throw new BadRequestException(errors);
                }

                var roleResult = await _userManager.AddToRoleAsync(user, UserType.ClinicOwner.ToString());
                if (!roleResult.Succeeded)
                {
                    var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    throw new BadRequestException(roleErrors);
                }

                clinic.ClinicAdminId = user.Id;

                var createdBy = _currentUserService.IsAuthenticated
                    ? _currentUserService.UserId.ToString()
                    : "system";
                clinic.MarkAsCreated(createdBy);

                user.AssignToClinic(clinic.Id);

                await _unitOfWork.ClinicRepository.AddAsync(clinic);

                var doctor = new Doctor(
                    userId: user.Id,
                    clinicId: clinic.Id,
                    specializationId: request.DoctorSpecializationId,
                    bio: request.Bio ?? string.Empty,
                    yearsOfExperience: request.YearsOfExperience);
                doctor.MarkAsCreated(createdBy);
                await _unitOfWork.DoctorRepository.AddAsync(doctor);

                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitAsync();

                await _emailService.SendClinicCredentialsAsync(request.OwnerEmail, request.Name, request.OwnerEmail, tempPassword, cancellationToken);

                var clinicDto = _mapper.Map<ClinicManagementDto>(clinic);
                return clinicDto;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        private static string GenerateRandomPassword(int length)
        {
            const string upper = "ABCDEFGHJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijkmnopqrstuvwxyz";
            const string digits = "23456789";
            const string special = "!@#$%^&*";
            var allChars = upper + lower + digits + special;
            var random = new Random();
            var password = new char[length];

            password[0] = upper[random.Next(upper.Length)];
            password[1] = lower[random.Next(lower.Length)];
            password[2] = digits[random.Next(digits.Length)];
            password[3] = special[random.Next(special.Length)];

            for (int i = 4; i < length; i++)
            {
                password[i] = allChars[random.Next(allChars.Length)];
            }

            return new string(password.OrderBy(_ => random.Next()).ToArray());
        }
    }
}
