using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.StaffDashboard.Commands.StaffRejectAppointment
{
    public class StaffRejectAppointmentCommandHandler : IRequestHandler<StaffRejectAppointmentCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFcmService _fcmService;

        public StaffRejectAppointmentCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IFcmService fcmService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _fcmService = fcmService;
        }

        public async Task<bool> Handle(StaffRejectAppointmentCommand request, CancellationToken cancellationToken)
        {
            var clinicId = _currentUserService.CurrentClinicId;
            if (clinicId == null)
                throw new BadRequestException(LocalizationKeys.ExceptionMessages.BadRequest.Value);

            var appointment = await _unitOfWork.AppointmentRepository.GetByIdAsync(request.AppointmentId);

            if (appointment.ClinicId != clinicId.Value)
                throw new BadRequestException(LocalizationKeys.AppointmentMessages.NotAuthorizedToRespond.Value);

            // Reject is allowed while the request is still pending (not accepted/paid),
            // and also after an acceptance the patient never paid — the clinic may withdraw it.
            if (appointment.Status is not (AppointmentStatus.Pending or AppointmentStatus.Reserved or AppointmentStatus.Accepted))
                throw new BadRequestException(LocalizationKeys.AppointmentMessages.CannotRespondAppointment.Value);

            var existingPayment = await _unitOfWork.PaymentRepository.GetByAppointmentIdAsync(appointment.Id);
            if (existingPayment is not null && existingPayment.Status == PaymentStatus.Paid)
                throw new BadRequestException(LocalizationKeys.AppointmentMessages.CannotRespondAppointment.Value);
            if (existingPayment is not null)
                existingPayment.MarkAsFailed("تم رفض الموعد من العيادة");

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
