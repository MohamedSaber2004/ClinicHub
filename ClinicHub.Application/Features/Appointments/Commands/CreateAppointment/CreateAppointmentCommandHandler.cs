using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Appointments.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Application.Features.Appointments.Commands.CreateAppointment
{
    public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, AppointmentDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly IBackgroundJobScheduler _jobScheduler;
        private readonly IFcmService _fcmService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CreateAppointmentCommandHandler> _logger;

        public CreateAppointmentCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IMapper mapper,
            IBackgroundJobScheduler jobScheduler,
            IFcmService fcmService,
            UserManager<ApplicationUser> userManager,
            ILogger<CreateAppointmentCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _jobScheduler = jobScheduler;
            _fcmService = fcmService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<AppointmentDto> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
        {
            var config = await _unitOfWork.BookingConfigurationRepository.GetByClinicIdAsync(request.ClinicId);
            if (config == null)
                throw new BadRequestException(LocalizationKeys.BookingMessages.BookingConfigNotFound.Value);

            if (request.AppointmentDate.Add(request.StartTime) <= DateTime.Now)
                throw new BadRequestException(LocalizationKeys.BookingMessages.PastDate.Value);

            if (request.AppointmentDate > DateTime.Now.Date.AddDays(config.MaxAdvanceBookingDays))
                throw new BadRequestException(LocalizationKeys.BookingMessages.InvalidDate.Value);

            var userId = _currentUserService.UserId;

            var appointment = new Appointment(
                userId,
                request.DoctorId,
                request.ClinicId,
                request.AppointmentDate,
                request.StartTime,
                request.EndTime,
                request.AppointmentType,
                request.PatientFullName,
                request.PatientAge,
                request.PatientGender,
                request.Complaint,
                request.ChronicDiseases);

            // Clinic with a consultation fee: hold the slot as Reserved until payment is completed
            // (auto-expired by ReservationExpirationJob when the reservation TTL passes).
            // Free clinics keep the appointment Pending (0) until staff/doctor approves or rejects it.
            if (config.ConsultationFee > 0)
                appointment.Reserve(config.ReservationTtlMinutes);

            await _unitOfWork.AppointmentRepository.AddAsync(appointment);
            await _unitOfWork.SaveChangesAsync();

            if (appointment.ExpiresAt.HasValue)
            {
                try
                {
                    await _jobScheduler.ScheduleReservationExpirationAsync(appointment.Id, appointment.ExpiresAt.Value);
                }
                catch (Exception ex)
                {
                    // Never turn a Hangfire scheduling failure into a 500 after the appointment
                    // is already committed. If the exact-time job was not scheduled, the hourly
                    // reservations-expiration sweep expires the reservation instead.
                    _logger.LogWarning(ex, "Failed to schedule reservation expiration for appointment {AppointmentId}; the hourly sweep will handle it.", appointment.Id);
                }
            }

            await NotifyClinicStaffAsync(appointment, request, cancellationToken);

            var dto = _mapper.Map<AppointmentDto>(appointment);
            dto.Amount = config.ConsultationFee;
            dto.Currency = config.Currency;
            return dto;
        }

        private async Task NotifyClinicStaffAsync(Appointment appointment, CreateAppointmentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var loaded = await _unitOfWork.AppointmentRepository
                    .GetFirstWithIncluding(a => a.Id == appointment.Id, a => a.Doctor, a => a.Clinic!)
                    .FirstOrDefaultAsync(cancellationToken);

                if (loaded == null)
                    return;

                var doctorName = loaded.Doctor?.User?.FullName ?? "";
                var parameters = new Dictionary<string, object>
                {
                    ["patientName"] = request.PatientFullName,
                    ["clinicName"] = loaded.Clinic?.Name ?? "",
                    ["doctorName"] = doctorName,
                    ["date"] = request.AppointmentDate.ToString("yyyy-MM-dd"),
                    ["time"] = $"{request.StartTime:hh\\:mm} - {request.EndTime:hh\\:mm}",
                    ["appointmentId"] = appointment.Id.ToString()
                };

                var recipients = new HashSet<Guid>();

                if (loaded.Doctor != null)
                    recipients.Add(loaded.Doctor.UserId);

                if (loaded.Clinic?.ClinicAdminId.HasValue == true)
                    recipients.Add(loaded.Clinic.ClinicAdminId.Value);

                var staffUsers = await _userManager.GetUsersInRoleAsync(UserType.Staff.ToString());
                foreach (var staff in staffUsers.Where(u => u.ClinicId == loaded.ClinicId && !u.IsDeleted))
                    recipients.Add(staff.Id);

                foreach (var userId in recipients)
                {
                    await _fcmService.SendToUserAsync(userId, NotificationType.NewBookingRequest, parameters);
                }
            }
            catch (Exception ex)
            {
                // Never block the booking on a push failure — the in-app notification
                // record and dispatch are best-effort.
                _logger.LogError(ex, "Failed to send new-booking notifications for appointment {AppointmentId}.", appointment.Id);
            }
        }
    }
}
