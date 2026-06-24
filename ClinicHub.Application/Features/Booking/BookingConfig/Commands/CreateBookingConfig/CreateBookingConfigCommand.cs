using ClinicHub.Application.Features.Booking.BookingConfig.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Booking.BookingConfig.Commands.CreateBookingConfig
{
    public record CreateBookingConfigCommand(Guid ClinicId, CreateBookingConfigDto Dto) : IRequest<BookingConfigResponseDto>;
}
