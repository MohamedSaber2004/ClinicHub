using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Appointments.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Application.Features.Appointments.Commands.CreateAppointment
{
    public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, AppointmentDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly IBackgroundJobScheduler _jobScheduler;
        private readonly ILogger<CreateAppointmentCommandHandler> _logger;

        public CreateAppointmentCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IMapper mapper,
            IBackgroundJobScheduler jobScheduler,
            ILogger<CreateAppointmentCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _jobScheduler = jobScheduler;
            _logger = logger;
        }

        public async Task<AppointmentDto> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
        {
            var config = await _unitOfWork.BookingConfigurationRepository.GetByClinicIdAsync(request.ClinicId);
            if (config == null)
                throw new BadRequestException(LocalizationKeys.BookingMessages.BookingConfigNotFound.Value);

            if (request.AppointmentDate.Add(request.StartTime) <= DateTime.Now)
                throw new BadRequestException(LocalizationKeys.BookingMessages.PastDate.Value);

            if (request.AppointmentDate > DateTime.UtcNow.Date.AddDays(config.MaxAdvanceBookingDays))
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

            var dto = _mapper.Map<AppointmentDto>(appointment);
            dto.Amount = config.ConsultationFee;
            dto.Currency = config.Currency;
            return dto;
        }
    }
}
