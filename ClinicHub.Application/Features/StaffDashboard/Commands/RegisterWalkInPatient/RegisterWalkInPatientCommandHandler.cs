using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.StaffDashboard.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.StaffDashboard.Commands.RegisterWalkInPatient
{
    public class RegisterWalkInPatientCommandHandler : IRequestHandler<RegisterWalkInPatientCommand, RegisterPatientResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly UserManager<ApplicationUser> _userManager;

        public RegisterWalkInPatientCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _userManager = userManager;
        }

        public async Task<RegisterPatientResponseDto> Handle(RegisterWalkInPatientCommand request, CancellationToken cancellationToken)
        {
            var clinicId = _currentUserService.CurrentClinicId ?? request.ClinicId;

            var existingUser = await _userManager.FindByEmailAsync(request.Email ?? "");
            var isNewUser = false;

            if (existingUser == null && !string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                existingUser = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber, cancellationToken);
            }

            ApplicationUser user;
            if (existingUser == null)
            {
                var birthDate = request.Age.HasValue
                    ? (DateTime?)new DateTime(DateTime.Today.Year - request.Age.Value, 1, 1)
                    : null;

                user = ApplicationUser.Create(
                    request.FullName,
                    request.Email ?? $"walkin_{Guid.NewGuid():N}@clinic.com",
                    request.PhoneNumber,
                    birthDate,
                    request.Gender);

                var result = await _userManager.CreateAsync(user, "WalkIn@123");
                if (!result.Succeeded)
                    throw new Exception("Failed to create walk-in patient user.");

                await _userManager.AddToRoleAsync(user, nameof(UserType.User));
                isNewUser = true;
            }
            else
            {
                user = existingUser;
            }

            if (!TimeSpan.TryParse(request.StartTime, out var startTime))
                startTime = TimeSpan.Zero;

            if (!TimeSpan.TryParse(request.EndTime, out var endTime))
                endTime = startTime.Add(TimeSpan.FromMinutes(30));

            var appointment = new Appointment(
                user.Id,
                request.DoctorId,
                clinicId,
                request.AppointmentDate,
                startTime,
                endTime,
                request.AppointmentType,
                request.FullName,
                request.PhoneNumber,
                request.Age ?? 0,
                request.Gender ?? Gender.Male,
                request.Complaint,
                request.ChronicDiseases);

            appointment.Accept();

            await _unitOfWork.AppointmentRepository.AddAsync(appointment);
            await _unitOfWork.SaveChangesAsync();

            return new RegisterPatientResponseDto
            {
                UserId = user.Id,
                AppointmentId = appointment.Id,
                IsNewUser = isNewUser
            };
        }
    }
}
