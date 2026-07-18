using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Clinics.Commands.RejectBooking
{
    public sealed class RejectBookingCommandHandler : IRequestHandler<RejectBookingCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public RejectBookingCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(RejectBookingCommand request, CancellationToken cancellationToken)
        {
            var appointment = await _unitOfWork.AppointmentRepository.GetByIdAsync(request.BookingId);

            if (appointment.ClinicId != _currentUserService.CurrentClinicId)
                throw new BadRequestException(LocalizationKeys.AppointmentMessages.NotAuthorizedToRespond.Value);

            if (appointment.Status != AppointmentStatus.Pending && appointment.Status != AppointmentStatus.Reserved)
                throw new BadRequestException(LocalizationKeys.AppointmentMessages.CannotRespondAppointment.Value);

            appointment.Reject(request.Reason);
            var result = await _unitOfWork.SaveChangesAsync();

            return result > 0;
        }
    }
}
