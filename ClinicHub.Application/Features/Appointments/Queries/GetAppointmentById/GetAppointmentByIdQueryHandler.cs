using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Features.Appointments.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Appointments.Queries.GetAppointmentById
{
    public class GetAppointmentByIdQueryHandler : IRequestHandler<GetAppointmentByIdQuery, AppointmentDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAppointmentByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AppointmentDto> Handle(GetAppointmentByIdQuery request, CancellationToken cancellationToken)
        {
            var appointment = await _unitOfWork.AppointmentRepository
                .GetAllAsync(a => a.Id == request.Id)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Clinic)
                .Include(a => a.Payment)
                .FirstOrDefaultAsync(cancellationToken);

            if (appointment == null)
                throw new NotFoundException(LocalizationKeys.AppointmentMessages.AppointmentNotFound.Value);

            var dto = _mapper.Map<AppointmentDto>(appointment);

            if (appointment.Payment != null)
            {
                dto.Amount = appointment.Payment.Amount;
                dto.Currency = appointment.Payment.Currency;
                dto.PaymentUrl = appointment.Payment.RedirectUrl;
            }
            else
            {
                var config = await _unitOfWork.BookingConfigurationRepository.GetByClinicIdAsync(appointment.ClinicId);
                if (config != null)
                {
                    dto.Amount = config.ConsultationFee;
                    dto.Currency = config.Currency;
                }
            }

            return dto;
        }
    }
}
