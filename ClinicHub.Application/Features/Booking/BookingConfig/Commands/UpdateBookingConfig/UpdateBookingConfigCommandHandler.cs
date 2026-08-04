using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Booking.BookingConfig.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using System;

namespace ClinicHub.Application.Features.Booking.BookingConfig.Commands.UpdateBookingConfig
{
    public class UpdateBookingConfigCommandHandler : IRequestHandler<UpdateBookingConfigCommand, BookingConfigResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public UpdateBookingConfigCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<BookingConfigResponseDto> Handle(UpdateBookingConfigCommand request, CancellationToken cancellationToken)
        {
            var clinic = await _unitOfWork.ClinicRepository.GetByIdAsync(request.ClinicId);
            if (clinic.ClinicAdminId != _currentUser.UserId)
                throw new UnauthorizedAccessException(LocalizationKeys.ExceptionMessages.Unauthorized.Value);

            var config = await _unitOfWork.BookingConfigurationRepository.GetByClinicIdAsync(request.ClinicId);
            if (config == null)
                throw new NotFoundException(LocalizationKeys.BookingMessages.BookingConfigNotFound.Value);

            var dto = request.Dto;
            config.Update(
                dto.ConsultationFee,
                "EGP",
                dto.MaxAdvanceBookingDays,
                dto.ReservationTtlMinutes,
                dto.CancellationWindowMinutes);

            _unitOfWork.BookingConfigurationRepository.Update(config);
            await _unitOfWork.SaveChangesAsync();

            return new BookingConfigResponseDto
            {
                ConsultationFee = config.ConsultationFee,
                Currency = config.Currency,
                MaxAdvanceBookingDays = config.MaxAdvanceBookingDays,
                ReservationTtlMinutes = config.ReservationTtlMinutes,
                CancellationWindowMinutes = config.CancellationWindowMinutes
            };
        }
    }
}
