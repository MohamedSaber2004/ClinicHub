using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Appointments.Commands.AcceptAppointment
{
    public class AcceptAppointmentCommandHandler : IRequestHandler<AcceptAppointmentCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public AcceptAppointmentCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(AcceptAppointmentCommand request, CancellationToken cancellationToken)
        {
            var appointment = await _unitOfWork.AppointmentRepository.GetByIdAsync(request.AppointmentId);

            if (appointment.Clinic.ClinicAdminId != _currentUserService.UserId)
                throw new BadRequestException(LocalizationKeys.AppointmentMessages.NotAuthorizedToRespond.Value);

            if (appointment.Status != AppointmentStatus.Pending && appointment.Status != AppointmentStatus.Reserved)
                throw new BadRequestException(LocalizationKeys.AppointmentMessages.CannotRespondAppointment.Value);

            appointment.Accept();
            var result = await _unitOfWork.SaveChangesAsync();

            return result > 0;
        }
    }
}
