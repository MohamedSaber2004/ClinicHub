using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Appointments.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.DoctorDashboard.Commands.DoctorAcceptAppointment
{
    public class DoctorAcceptAppointmentCommandHandler : IRequestHandler<DoctorAcceptAppointmentCommand, AppointmentAcceptanceResultDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAppointmentAcceptanceService _acceptanceService;

        public DoctorAcceptAppointmentCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IAppointmentAcceptanceService acceptanceService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _acceptanceService = acceptanceService;
        }

        public async Task<AppointmentAcceptanceResultDto> Handle(DoctorAcceptAppointmentCommand request, CancellationToken cancellationToken)
        {
            var doctor = await _unitOfWork.DoctorRepository.GetFirstAsync(
                d => d.UserId == _currentUserService.UserId && !d.IsDeleted, cancellationToken);

            if (doctor == null)
                throw new BadRequestException(LocalizationKeys.DoctorMessages.NotFound.Value);

            var appointment = await _unitOfWork.AppointmentRepository.GetByIdAsync(request.AppointmentId);

            if (appointment.DoctorId != doctor.Id)
                throw new BadRequestException(LocalizationKeys.AppointmentMessages.NotAuthorizedToRespond.Value);

            return await _acceptanceService.AcceptAsync(appointment, cancellationToken);
        }
    }
}
