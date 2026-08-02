using Asp.Versioning;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Ads.Queries.GetActiveAdsPublic;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1;

[ApiVersion("1.0")]
[AllowAnonymous]
public class PublicAdsController : BaseApiController
{
    public PublicAdsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [Route(ApiRoutes.PublicAds.Active)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveAds(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetActiveAdsPublicQuery(), ct);
        return Ok(result);
    }
}
