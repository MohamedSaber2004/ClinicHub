using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Appointments.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Appointments.Commands.AcceptAppointment
{
    public class AcceptAppointmentCommandHandler : IRequestHandler<AcceptAppointmentCommand, AppointmentAcceptanceResultDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAppointmentAcceptanceService _acceptanceService;

        public AcceptAppointmentCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IAppointmentAcceptanceService acceptanceService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _acceptanceService = acceptanceService;
        }

        public async Task<AppointmentAcceptanceResultDto> Handle(AcceptAppointmentCommand request, CancellationToken cancellationToken)
        {
            var appointment = await _unitOfWork.AppointmentRepository.GetByIdAsync(request.AppointmentId);

            if (appointment.Clinic.ClinicAdminId != _currentUserService.UserId)
                throw new BadRequestException(LocalizationKeys.AppointmentMessages.NotAuthorizedToRespond.Value);

            return await _acceptanceService.AcceptAsync(appointment, cancellationToken);
        }
    }
}
