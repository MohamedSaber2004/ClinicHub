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

            appointment.CheckIn();

            await _unitOfWork.AppointmentRepository.AddAsync(appointment);
            await _unitOfWork.SaveChangesAsync();

            var todayStart = DateTime.Today;
            var todayEnd = todayStart.AddDays(1);
            var queueNumber = await _unitOfWork.AppointmentRepository
                .GetAllAsync(a => a.ClinicId == clinicId && !a.IsDeleted
                    && a.AppointmentDate >= todayStart && a.AppointmentDate < todayEnd
                    && (a.Status == AppointmentStatus.Confirmed
                        || a.Status == AppointmentStatus.Completed))
                .CountAsync(cancellationToken);

            return new RegisterPatientResponseDto
            {
                AppointmentId = appointment.Id,
                PatientId = user.Id,
                QueueNumber = queueNumber,
                Message = "\u062A\u0645 \u062A\u0633\u062C\u064A\u0644 \u0627\u0644\u0645\u0631\u064A\u0636 \u0628\u0646\u062C\u0627\u062D"
            };
        }
    }
}
