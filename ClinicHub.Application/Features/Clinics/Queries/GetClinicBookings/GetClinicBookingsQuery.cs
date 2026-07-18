using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Clinics.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Clinics.Queries.GetClinicBookings
{
    public class GetClinicBookingsQuery : IRequest<PagginatedResult<ClinicBookingDto>>
    {
        public string? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
