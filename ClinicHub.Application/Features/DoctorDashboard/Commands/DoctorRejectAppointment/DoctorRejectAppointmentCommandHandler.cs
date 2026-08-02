using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.DoctorDashboard.Commands.DoctorRejectAppointment
{
    public class DoctorRejectAppointmentCommandHandler : IRequestHandler<DoctorRejectAppointmentCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFcmService _fcmService;

        public DoctorRejectAppointmentCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IFcmService fcmService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _fcmService = fcmService;
        }

        public async Task<bool> Handle(DoctorRejectAppointmentCommand request, CancellationToken cancellationToken)
        {
            var doctor = await _unitOfWork.DoctorRepository.GetFirstAsync(
                d => d.UserId == _currentUserService.UserId && !d.IsDeleted, cancellationToken);

            if (doctor == null)
                throw new BadRequestException(LocalizationKeys.DoctorMessages.NotFound.Value);

            var appointment = await _unitOfWork.AppointmentRepository.GetByIdAsync(request.AppointmentId);

            if (appointment.DoctorId != doctor.Id)
                throw new BadRequestException(LocalizationKeys.AppointmentMessages.NotAuthorizedToRespond.Value);

            // Reject is only allowed while the request is still pending (not accepted/paid).
            if (appointment.Status != AppointmentStatus.Pending && appointment.Status != AppointmentStatus.Reserved)
                throw new BadRequestException(LocalizationKeys.AppointmentMessages.CannotRespondAppointment.Value);

            appointment.Reject(request.Reason);
            await _unitOfWork.SaveChangesAsync();

            await _fcmService.SendToUserAsync(appointment.BookedByUserId, NotificationType.AppointmentCancellation, new()
            {
                ["clinicName"] = appointment.Clinic?.Name ?? "",
                ["date"] = appointment.AppointmentDate.ToString("yyyy-MM-dd"),
                ["reason"] = request.Reason ?? ""
            });

            return true;
        }
    }
}
