using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Appointments.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Appointments.Commands.CreateAppointment
{
    public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, AppointmentDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public CreateAppointmentCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<AppointmentDto> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
        {
            var config = await _unitOfWork.BookingConfigurationRepository.GetByClinicIdAsync(request.ClinicId);
            if (config == null)
                throw new BadRequestException(LocalizationKeys.BookingMessages.BookingConfigNotFound.Value);

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
                request.PatientPhoneNumber,
                request.PatientAge,
                request.PatientGender,
                request.Complaint,
                request.ChronicDiseases);

            appointment.Reserve(config.ReservationTtlMinutes);

            await _unitOfWork.AppointmentRepository.AddAsync(appointment);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AppointmentDto>(appointment);
        }
    }
}
