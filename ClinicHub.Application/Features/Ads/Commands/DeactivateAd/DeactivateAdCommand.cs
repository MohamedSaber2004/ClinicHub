using MediatR;

namespace ClinicHub.Application.Features.Ads.Commands.DeactivateAd;

public class DeactivateAdCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public string? Reason { get; set; }
}
