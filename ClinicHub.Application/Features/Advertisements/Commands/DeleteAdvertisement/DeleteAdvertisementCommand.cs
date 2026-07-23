using MediatR;

namespace ClinicHub.Application.Features.Advertisements.Commands.DeleteAdvertisement
{
    public class DeleteAdvertisementCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }
}
