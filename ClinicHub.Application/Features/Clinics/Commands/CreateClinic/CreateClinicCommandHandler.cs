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
            var dto = request.Dto;

            var clinic = new Clinic
            {
                Name = dto.Name,
                NameAr = dto.NameAr,
                Description = dto.Description,
                ArDescription = dto.ArDescription,
                Address = dto.Address,
                AddressAr = dto.AddressAr,
                Phone = dto.Phone,
                Email = dto.Email,
                Website = dto.Website,
                Logo = dto.Logo,
                WorkingHours = dto.WorkingHours,
                WorkingHoursStart = dto.WorkingHoursStart,
                WorkingHoursEnd = dto.WorkingHoursEnd,
                WorkingDays = dto.WorkingDays != null ? string.Join(",", dto.WorkingDays) : null,
                SpecializationId = dto.SpecializationId,
                Location = new Point(0, 0) { SRID = 4326 },
                IsRegistered = true,
                Status = ClinicStatus.Active
            };

            var tempPassword = GenerateRandomPassword(12);

            var user = ApplicationUser.Create(dto.OwnerName, dto.OwnerEmail, dto.OwnerPhone ?? string.Empty, null, null);
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

            await _unitOfWork.ClinicRepository.AddAsync(clinic);
            await _unitOfWork.SaveChangesAsync();

            await _emailService.SendClinicCredentialsAsync(dto.OwnerEmail, dto.Name, dto.OwnerEmail, tempPassword, cancellationToken);

            var clinicDto = _mapper.Map<ClinicManagementDto>(clinic);
            return clinicDto;
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
