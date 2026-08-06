using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.StaffDashboard.Commands.StaffCompleteAppointment
{
    public class StaffCompleteAppointmentCommandHandler : IRequestHandler<StaffCompleteAppointmentCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public StaffCompleteAppointmentCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(StaffCompleteAppointmentCommand request, CancellationToken cancellationToken)
        {
            var clinicId = _currentUserService.CurrentClinicId;
            if (clinicId == null)
                throw new BadRequestException(LocalizationKeys.ExceptionMessages.BadRequest.Value);

            var appointment = await _unitOfWork.AppointmentRepository.GetByIdAsync(request.AppointmentId);

            if (appointment.ClinicId != clinicId.Value)
                throw new BadRequestException(LocalizationKeys.AppointmentMessages.NotAuthorizedToRespond.Value);

            // Complete is only allowed after the appointment is confirmed (paid).
            if (appointment.Status != AppointmentStatus.Confirmed)
                throw new BadRequestException(LocalizationKeys.AppointmentMessages.CannotRespondAppointment.Value);

            // Complete is only allowed after the appointment date and time have passed.
            if (appointment.AppointmentDate.Add(appointment.EndTime) > DateTime.Now)
                throw new BadRequestException(LocalizationKeys.AppointmentMessages.CannotCompleteBeforeTime.Value);

            appointment.Complete();
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
