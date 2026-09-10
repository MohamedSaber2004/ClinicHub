using AutoMapper;
using ClinicHub.Application.Common;
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
        private readonly IFcmService _fcmService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CreateAppointmentCommandHandler> _logger;

        public CreateAppointmentCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IMapper mapper,
            IFcmService fcmService,
            UserManager<ApplicationUser> userManager,
            ILogger<CreateAppointmentCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _fcmService = fcmService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<AppointmentDto> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
        {
            var config = await _unitOfWork.BookingConfigurationRepository.GetByClinicIdAsync(request.ClinicId);
            if (config == null)
                throw new BadRequestException(LocalizationKeys.BookingMessages.BookingConfigNotFound.Value);

            // Timezone-free: unzoned calendar date vs server wall-clock.
            if (AppDate.ToUnzonedDate(request.AppointmentDate).Add(request.StartTime) <= AppDate.Now)
                throw new BadRequestException(LocalizationKeys.BookingMessages.PastDate.Value);

            if (AppDate.ToUnzonedDate(request.AppointmentDate) > AppDate.Today.AddDays(config.MaxAdvanceBookingDays))
                throw new BadRequestException(LocalizationKeys.BookingMessages.InvalidDate.Value);

            // Ownership comes from the authenticated user via ICurrentUserService:
            // whoever calls the endpoint owns the booking, so it always shows up
            // in their /appointments/my queue. Anonymous callers have no identity
            // to own the booking (UserId is Empty) and are rejected here even if
            // they bypass the controller's auth filter.
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException(LocalizationKeys.ExceptionMessages.Unauthorized.Value);

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

            // Clinic with a consultation fee: hold the slot as Reserved until clinic
            // staff accepts, rejects, or cancels it. Reservations never expire on
            // their own — staff action is the only decision point.
            // Free clinics keep the appointment Pending (0) until staff/doctor approves or rejects it.
            if (config.ConsultationFee > 0)
                appointment.Reserve();

            await _unitOfWork.AppointmentRepository.AddAsync(appointment);
            await _unitOfWork.SaveChangesAsync();

            await NotifyClinicStaffAsync(appointment, request, cancellationToken);

            // Commit the notification rows added by the sends above. The appointment itself
            // was already committed above because the staff-notification re-query requires
            // the row to exist.
            await _unitOfWork.SaveChangesAsync();

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
