using MediatR;

namespace ClinicHub.Application.Features.Advertisements.Commands.RejectAdvertisement
{
    public class RejectAdvertisementCommand : IRequest<bool>
    {
        public Guid AdvertisementId { get; set; }
    }
}
