using ClinicHub.Application.Features.Advertisements.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Advertisements.Commands.UpdateAdvertisement
{
    public class UpdateAdvertisementCommand : IRequest<AdvertisementDto>
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? ImageUrl { get; set; }
        public string? TargetUrl { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? AmountPaid { get; set; }
    }
}
