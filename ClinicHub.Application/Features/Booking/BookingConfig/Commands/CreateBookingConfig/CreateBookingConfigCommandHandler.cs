using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Booking.BookingConfig.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using System;

namespace ClinicHub.Application.Features.Booking.BookingConfig.Commands.CreateBookingConfig
{
    public class CreateBookingConfigCommandHandler : IRequestHandler<CreateBookingConfigCommand, BookingConfigResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public CreateBookingConfigCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<BookingConfigResponseDto> Handle(CreateBookingConfigCommand request, CancellationToken cancellationToken)
        {
            var clinic = await _unitOfWork.ClinicRepository.GetByIdAsync(request.ClinicId);
            if (clinic.ClinicAdminId != _currentUser.UserId)
                throw new UnauthorizedAccessException(LocalizationKeys.ExceptionMessages.Unauthorized.Value);

            var dto = request.Dto;
            var config = new BookingConfiguration(
                request.ClinicId,
                dto.ConsultationFee,
                "EGP",
                dto.MaxAdvanceBookingDays,
                dto.ReservationTtlMinutes);

            await _unitOfWork.BookingConfigurationRepository.AddAsync(config);
            await _unitOfWork.SaveChangesAsync();

            return new BookingConfigResponseDto
            {
                ConsultationFee = config.ConsultationFee,
                Currency = config.Currency,
                MaxAdvanceBookingDays = config.MaxAdvanceBookingDays,
                ReservationTtlMinutes = config.ReservationTtlMinutes
            };
        }
    }
}
