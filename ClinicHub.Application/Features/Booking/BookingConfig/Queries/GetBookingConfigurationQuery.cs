using ClinicHub.Application.Features.Booking.BookingConfig.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Booking.BookingConfig.Queries
{
    public class GetBookingConfigurationQuery : IRequest<BookingConfigResponseDto>
    {
        public Guid ClinicId { get; set; }
    }
}
