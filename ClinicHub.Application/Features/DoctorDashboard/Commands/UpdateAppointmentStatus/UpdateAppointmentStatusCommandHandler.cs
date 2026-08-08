using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Appointments.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.DoctorDashboard.Commands.UpdateAppointmentStatus
{
    /// <summary>
    /// Unified handler for updating appointment status from the Doctor Dashboard.
    /// Status codes match the <see cref="AppointmentStatus"/> contract consumed by the
    /// dashboards and the mobile app: 6 = Accept (creates payment + sends link),
    /// 2 = Reject, 3 = Complete, 5 = No-show. Legacy code 1 is still accepted as Accept.
    /// </summary>
    public class UpdateAppointmentStatusCommandHandler : IRequestHandler<UpdateAppointmentStatusCommand, AppointmentAcceptanceResultDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAppointmentAcceptanceService _acceptanceService;
        private readonly IFcmService _fcmService;

        public UpdateAppointmentStatusCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IAppointmentAcceptanceService acceptanceService,
            IFcmService fcmService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _acceptanceService = acceptanceService;
            _fcmService = fcmService;
        }

        public async Task<AppointmentAcceptanceResultDto?> Handle(UpdateAppointmentStatusCommand request, CancellationToken cancellationToken)
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
                // 6 = Accept (AppointmentStatus.Accepted) — legacy 1 = Accept
                case 1:
                case (int)AppointmentStatus.Accepted:
                    return await _acceptanceService.AcceptAsync(appointment, cancellationToken);

                // 2 = Reject / Cancel — only pending requests can be rejected
                case (int)AppointmentStatus.Cancelled:
                    if (appointment.Status != AppointmentStatus.Pending && appointment.Status != AppointmentStatus.Reserved)
                        throw new BadRequestException(LocalizationKeys.AppointmentMessages.CannotRespondAppointment.Value);

                    appointment.Reject(request.Notes);

                    await _fcmService.SendToUserAsync(appointment.BookedByUserId, NotificationType.AppointmentCancellation, new()
                    {
                        ["clinicName"] = appointment.Clinic?.Name ?? "",
                        ["date"] = appointment.AppointmentDate.ToString("yyyy-MM-dd"),
                        ["reason"] = request.Notes ?? ""
                    });

                    // Single commit: the rejection and the notification row in one transaction.
                    await _unitOfWork.SaveChangesAsync();
                    break;

                // 3 = Complete — only paid/confirmed appointments can be completed
                case (int)AppointmentStatus.Completed:
                    if (appointment.Status != AppointmentStatus.Confirmed)
                        throw new BadRequestException(LocalizationKeys.AppointmentMessages.CannotRespondAppointment.Value);

                    appointment.Complete();
                    await _unitOfWork.SaveChangesAsync();
                    break;

                // 5 = No-show — only paid/confirmed appointments can be marked no-show
                case (int)AppointmentStatus.NoShow:
                    if (appointment.Status != AppointmentStatus.Confirmed)
                        throw new BadRequestException(LocalizationKeys.AppointmentMessages.CannotRespondAppointment.Value);

                    appointment.MarkNoShow();
                    await _unitOfWork.SaveChangesAsync();
                    break;

                default:
                    throw new BadRequestException("Invalid status code. Valid values: 6 (Accept), 2 (Reject), 3 (Complete), 5 (No-show).");
            }

            return null;
        }
    }
}
