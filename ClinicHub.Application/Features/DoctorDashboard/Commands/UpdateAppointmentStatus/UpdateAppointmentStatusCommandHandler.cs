using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.DoctorDashboard.Commands.UpdateAppointmentStatus
{
    /// <summary>
    /// Unified handler for updating appointment status from the Doctor Dashboard.
    /// Dispatches to the correct domain action (Accept, Reject, Complete) based on the requested status code.
    /// </summary>
    public class UpdateAppointmentStatusCommandHandler : IRequestHandler<UpdateAppointmentStatusCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFcmService _fcmService;

        public UpdateAppointmentStatusCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IFcmService fcmService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _fcmService = fcmService;
        }

        public async Task<bool> Handle(UpdateAppointmentStatusCommand request, CancellationToken cancellationToken)
        {
            var doctor = await _unitOfWork.DoctorRepository.GetFirstAsync(
                d => d.UserId == _currentUserService.UserId && !d.IsDeleted, cancellationToken);

            if (doctor == null)
                throw new BadRequestException(LocalizationKeys.DoctorMessages.NotFound.Value);

            var appointment = await _unitOfWork.AppointmentRepository.GetByIdAsync(request.AppointmentId);

            if (appointment.DoctorId != doctor.Id)
                throw new BadRequestException(LocalizationKeys.AppointmentMessages.NotAuthorizedToRespond.Value);

            switch (request.Status)
            {
                // 1 = Accept / Confirm
                case 1:
                    if (appointment.Status != AppointmentStatus.Pending
                        && appointment.Status != AppointmentStatus.Reserved
                        && appointment.Status != AppointmentStatus.Confirmed)
                        throw new BadRequestException(LocalizationKeys.AppointmentMessages.CannotRespondAppointment.Value);

                    appointment.Accept();
                    await _unitOfWork.SaveChangesAsync();

                    await _fcmService.SendToUserAsync(appointment.BookedByUserId, NotificationType.AppointmentConfirmation, new()
                    {
                        ["clinicName"] = appointment.Clinic?.Name ?? "",
                        ["date"] = appointment.AppointmentDate.ToString("yyyy-MM-dd")
                    });
                    break;

                // 2 = Reject / Cancel
                case 2:
                    if (appointment.Status != AppointmentStatus.Pending
                        && appointment.Status != AppointmentStatus.Reserved
                        && appointment.Status != AppointmentStatus.Confirmed)
                        throw new BadRequestException(LocalizationKeys.AppointmentMessages.CannotRespondAppointment.Value);

                    appointment.Reject(request.Notes);
                    await _unitOfWork.SaveChangesAsync();

                    await _fcmService.SendToUserAsync(appointment.BookedByUserId, NotificationType.AppointmentCancellation, new()
                    {
                        ["clinicName"] = appointment.Clinic?.Name ?? "",
                        ["date"] = appointment.AppointmentDate.ToString("yyyy-MM-dd"),
                        ["reason"] = request.Notes ?? ""
                    });
                    break;

                // 3 = Complete
                case 3:
                    if (appointment.Status != AppointmentStatus.Accepted
                        && appointment.Status != AppointmentStatus.Confirmed)
                        throw new BadRequestException(LocalizationKeys.AppointmentMessages.CannotRespondAppointment.Value);

                    appointment.Complete();
                    await _unitOfWork.SaveChangesAsync();
                    break;

                default:
                    throw new BadRequestException("Invalid status code. Valid values: 1 (Accept), 2 (Reject), 3 (Complete).");
            }

            return true;
        }
    }
}
