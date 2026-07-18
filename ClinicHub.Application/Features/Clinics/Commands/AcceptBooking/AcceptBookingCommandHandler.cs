using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Clinics.Commands.AcceptBooking
{
    public sealed class AcceptBookingCommandHandler : IRequestHandler<AcceptBookingCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFcmService _fcmService;

        public AcceptBookingCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IFcmService fcmService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _fcmService = fcmService;
        }

        public async Task<bool> Handle(AcceptBookingCommand request, CancellationToken cancellationToken)
        {
            var appointment = await _unitOfWork.AppointmentRepository.GetByIdAsync(request.BookingId);

            if (appointment.ClinicId != _currentUserService.CurrentClinicId)
                throw new BadRequestException(LocalizationKeys.AppointmentMessages.NotAuthorizedToRespond.Value);

            if (appointment.Status != AppointmentStatus.Pending && appointment.Status != AppointmentStatus.Reserved)
                throw new BadRequestException(LocalizationKeys.AppointmentMessages.CannotRespondAppointment.Value);

            appointment.Accept();
            var result = await _unitOfWork.SaveChangesAsync();

            await _fcmService.SendToUserAsync(appointment.BookedByUserId, NotificationType.AppointmentConfirmation, new()
            {
                ["clinicName"] = appointment.Clinic.Name,
                ["date"] = appointment.AppointmentDate.ToString("yyyy-MM-dd")
            });

            return result > 0;
        }
    }
}
