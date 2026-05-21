using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Appointments.Commands.CreateAppointment
{
    public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, AppointmentDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public CreateAppointmentCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<AppointmentDto> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
        {
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

            await _unitOfWork.AppointmentRepository.AddAsync(appointment);
            await _unitOfWork.SaveChangesAsync();

            var dto = new AppointmentDto
            {
                Id = appointment.Id,
                BookedByUserId = appointment.BookedByUserId,
                DoctorId = appointment.DoctorId,
                ClinicId = appointment.ClinicId,
                AppointmentDate = appointment.AppointmentDate,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                AppointmentType = appointment.AppointmentType,
                Status = appointment.Status,
                PatientFullName = appointment.PatientFullName,
                PatientPhoneNumber = appointment.PatientPhoneNumber,
                PatientAge = appointment.PatientAge,
                PatientGender = appointment.PatientGender,
                Complaint = appointment.Complaint,
                ChronicDiseases = appointment.ChronicDiseases,
                CancellationReason = appointment.CancellationReason
            };

            return dto;
        }
    }
}
