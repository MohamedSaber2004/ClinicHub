using ClinicHub.Application.Features.Advertisements.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Advertisements.Commands.CreateAdvertisement
{
    public class CreateAdvertisementCommand : IRequest<AdvertisementDto>
    {
        public string Title { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string? TargetUrl { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal AmountPaid { get; set; }
    }
}
