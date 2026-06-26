using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Features.Booking.BookingConfig.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Booking.BookingConfig.Queries
{
    public class GetBookingConfigurationQueryHandler : IRequestHandler<GetBookingConfigurationQuery, BookingConfigResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetBookingConfigurationQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BookingConfigResponseDto> Handle(GetBookingConfigurationQuery request, CancellationToken cancellationToken)
        {
            var config = await _unitOfWork.BookingConfigurationRepository.GetByClinicIdAsync(request.ClinicId);

            if (config == null)
                throw new NotFoundException(LocalizationKeys.BookingMessages.BookingConfigNotFound.Value);

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
