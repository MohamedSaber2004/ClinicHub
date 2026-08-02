using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Appointments.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Clinics.Commands.AcceptBooking
{
    public sealed class AcceptBookingCommandHandler : IRequestHandler<AcceptBookingCommand, AppointmentAcceptanceResultDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAppointmentAcceptanceService _acceptanceService;

        public AcceptBookingCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IAppointmentAcceptanceService acceptanceService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _acceptanceService = acceptanceService;
        }

        public async Task<AppointmentAcceptanceResultDto> Handle(AcceptBookingCommand request, CancellationToken cancellationToken)
        {
            var appointment = await _unitOfWork.AppointmentRepository.GetByIdAsync(request.BookingId);

            if (appointment.ClinicId != _currentUserService.CurrentClinicId)
                throw new BadRequestException(LocalizationKeys.AppointmentMessages.NotAuthorizedToRespond.Value);

            return await _acceptanceService.AcceptAsync(appointment, cancellationToken);
        }
    }
}
