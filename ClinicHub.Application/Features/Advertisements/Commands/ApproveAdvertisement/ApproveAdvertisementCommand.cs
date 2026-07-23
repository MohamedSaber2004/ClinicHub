using MediatR;

namespace ClinicHub.Application.Features.Advertisements.Commands.ApproveAdvertisement
{
    public class ApproveAdvertisementCommand : IRequest<bool>
    {
        public Guid AdvertisementId { get; set; }
    }
}
