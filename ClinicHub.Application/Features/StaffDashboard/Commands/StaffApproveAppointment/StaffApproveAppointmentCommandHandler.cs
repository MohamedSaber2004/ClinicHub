using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Appointments.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.StaffDashboard.Commands.StaffApproveAppointment
{
    public class StaffApproveAppointmentCommandHandler : IRequestHandler<StaffApproveAppointmentCommand, AppointmentAcceptanceResultDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAppointmentAcceptanceService _acceptanceService;

        public StaffApproveAppointmentCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IAppointmentAcceptanceService acceptanceService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _acceptanceService = acceptanceService;
        }

        public async Task<AppointmentAcceptanceResultDto> Handle(StaffApproveAppointmentCommand request, CancellationToken cancellationToken)
        {
            var clinicId = _currentUserService.CurrentClinicId;
            if (clinicId == null)
                throw new BadRequestException(LocalizationKeys.ExceptionMessages.BadRequest.Value);

            var appointment = await _unitOfWork.AppointmentRepository.GetByIdAsync(request.AppointmentId);

            if (appointment.ClinicId != clinicId.Value)
                throw new BadRequestException(LocalizationKeys.AppointmentMessages.NotAuthorizedToRespond.Value);

            return await _acceptanceService.AcceptAsync(appointment, cancellationToken);
        }
    }
}
