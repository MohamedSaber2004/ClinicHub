using MediatR;

namespace ClinicHub.Application.Features.Ads.Commands.AdPackages;

public class DeleteAdPackageCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
