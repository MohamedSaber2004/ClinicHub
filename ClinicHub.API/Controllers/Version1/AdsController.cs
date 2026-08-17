using Asp.Versioning;
using ClinicHub.API.Filters;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Ads.Commands.CreateClinicAdOrder;
using ClinicHub.Application.Features.Ads.Queries.GetActiveAdPackages;
using ClinicHub.Application.Features.Ads.Queries.GetMyAds;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1;

[ApiVersion("1.0")]
[RoleAuthorize(nameof(UserType.ClinicOwner))]
[RequirePlanPermission(SubscriptionPermission.MarketingTools)]
public class AdsController : BaseApiController
{
    public AdsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [Route(ApiRoutes.Ads.MyAds)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyAds([FromRoute] Guid clinicId, [FromQuery] AdvertisementStatus? status, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMyAdsQuery { ClinicId = clinicId, Status = status }, ct);
        return Ok(result);
    }

    [HttpGet]
    [Route(ApiRoutes.Ads.Packages)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPackages(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetActiveAdPackagesQuery(), ct);
        return Ok(result);
    }

    [HttpPost]
    [Route(ApiRoutes.Ads.CreateOrder)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateOrder([FromRoute] Guid clinicId, [FromBody] CreateClinicAdOrderCommand command, CancellationToken ct)
    {
        command.ClinicId = clinicId;
        var result = await _mediator.Send(command, ct);
        return Created(ApiRoutes.Ads.CreateOrder, result);
    }
}
