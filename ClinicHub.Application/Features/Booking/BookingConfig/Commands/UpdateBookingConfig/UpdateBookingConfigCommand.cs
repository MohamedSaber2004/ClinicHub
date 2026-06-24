using ClinicHub.Application.Features.Booking.BookingConfig.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Booking.BookingConfig.Commands.UpdateBookingConfig
{
    public record UpdateBookingConfigCommand(Guid ClinicId, UpdateBookingConfigDto Dto) : IRequest<BookingConfigResponseDto>;
}
