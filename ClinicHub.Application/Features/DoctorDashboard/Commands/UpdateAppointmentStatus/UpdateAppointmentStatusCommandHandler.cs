using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Appointments.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.DoctorDashboard.Commands.UpdateAppointmentStatus
{
    /// <summary>
    /// Unified handler for updating appointment status from the Doctor Dashboard.
    /// Status codes match the <see cref="AppointmentStatus"/> contract consumed by the
    /// dashboards and the mobile app: 6 = Accept (creates payment + sends link),
    /// 2 = Reject (unpaid request) or Cancel (paid appointment, refunding the patient),
    /// 3 = Complete, 5 = No-show. Legacy code 1 is still accepted as Accept.
    /// </summary>
    public class UpdateAppointmentStatusCommandHandler : IRequestHandler<UpdateAppointmentStatusCommand, AppointmentAcceptanceResultDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAppointmentAcceptanceService _acceptanceService;
        private readonly IAppointmentCancellationService _cancellationService;
        private readonly IFcmService _fcmService;

        public UpdateAppointmentStatusCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IAppointmentAcceptanceService acceptanceService,
            IAppointmentCancellationService cancellationService,
            IFcmService fcmService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _acceptanceService = acceptanceService;
            _cancellationService = cancellationService;
            _fcmService = fcmService;
        }

        public async Task<AppointmentAcceptanceResultDto?> Handle(UpdateAppointmentStatusCommand request, CancellationToken cancellationToken)
        {
            var doctor = await _unitOfWork.DoctorRepository.GetFirstAsync(
                d => d.UserId == _currentUserService.UserId && !d.IsDeleted, cancellationToken);

            if (doctor == null)
                throw new BadRequestException(LocalizationKeys.DoctorMessages.NotFound.Value);

            // Clinic is included because the cancellation notification names it; the plain
            // FindAsync used by GetByIdAsync leaves it unloaded.
            var appointment = await _unitOfWork.AppointmentRepository
                .GetFirstWithIncluding(a => a.Id == request.AppointmentId, a => a.Clinic!)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException(nameof(Appointment), request.AppointmentId);

            if (appointment.DoctorId != doctor.Id)
                throw new BadRequestException(LocalizationKeys.AppointmentMessages.NotAuthorizedToRespond.Value);

            switch (request.Status)
            {
                // 6 = Accept (AppointmentStatus.Accepted) — legacy 1 = Accept
                case 1:
                case (int)AppointmentStatus.Accepted:
                    return await _acceptanceService.AcceptAsync(appointment, cancellationToken, request.PaymentMethod, request.ReturnUrl);

                // 2 = Reject a not-yet-paid request, or Cancel a paid appointment.
                case (int)AppointmentStatus.Cancelled:
                    await CancelOrRejectAsync(appointment, request.Notes, cancellationToken);
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
                    throw new BadRequestException("Invalid status code. Valid values: 6 (Accept), 2 (Reject/Cancel), 3 (Complete), 5 (No-show).");
            }

            return null;
        }

        private async Task CancelOrRejectAsync(Appointment appointment, string? reason, CancellationToken cancellationToken)
        {
            if (appointment.Status is AppointmentStatus.Pending or AppointmentStatus.Reserved)
            {
                // No money was captured at this stage, so there is nothing to refund.
                appointment.Reject(reason);
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled
                                        or AppointmentStatus.NoShow or AppointmentStatus.Rejected)
                    throw new BadRequestException(LocalizationKeys.AppointmentMessages.CannotRespondAppointment.Value);

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _cancellationService.CancelAsync(appointment, reason ?? string.Empty, cancellationToken);
                    await _unitOfWork.CommitAsync();
                }
                catch
                {
                    await _unitOfWork.RollbackAsync();
                    throw;
                }
            }

            await _fcmService.SendToUserAsync(appointment.BookedByUserId, NotificationType.AppointmentCancellation, new()
            {
                ["clinicName"] = appointment.Clinic?.Name ?? "",
                ["date"] = appointment.AppointmentDate.ToString("yyyy-MM-dd"),
                ["reason"] = reason ?? ""
            });

            // Single commit: the notification row added by the send above.
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
