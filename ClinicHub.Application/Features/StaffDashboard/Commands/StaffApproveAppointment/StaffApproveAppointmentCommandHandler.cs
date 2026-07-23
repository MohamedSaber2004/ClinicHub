using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.StaffDashboard.Commands.StaffApproveAppointment
{
    public class StaffApproveAppointmentCommandHandler : IRequestHandler<StaffApproveAppointmentCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFcmService _fcmService;

        public StaffApproveAppointmentCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IFcmService fcmService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _fcmService = fcmService;
        }

        public async Task<bool> Handle(StaffApproveAppointmentCommand request, CancellationToken cancellationToken)
        {
            var clinicId = _currentUserService.CurrentClinicId;
            if (clinicId == null)
                throw new BadRequestException(LocalizationKeys.ExceptionMessages.BadRequest.Value);

            var appointment = await _unitOfWork.AppointmentRepository.GetByIdAsync(request.AppointmentId);

            if (appointment.ClinicId != clinicId.Value)
                throw new BadRequestException(LocalizationKeys.AppointmentMessages.NotAuthorizedToRespond.Value);

            if (appointment.Status != AppointmentStatus.Pending && appointment.Status != AppointmentStatus.Reserved
                && appointment.Status != AppointmentStatus.Confirmed)
                throw new BadRequestException(LocalizationKeys.AppointmentMessages.CannotRespondAppointment.Value);

            appointment.Accept();
            await _unitOfWork.SaveChangesAsync();

            await _fcmService.SendToUserAsync(appointment.BookedByUserId, NotificationType.AppointmentConfirmation, new()
            {
                ["clinicName"] = appointment.Clinic?.Name ?? "",
                ["date"] = appointment.AppointmentDate.ToString("yyyy-MM-dd")
            });

            return true;
        }
    }
}
