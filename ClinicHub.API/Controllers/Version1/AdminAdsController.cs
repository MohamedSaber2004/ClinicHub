using Asp.Versioning;
using ClinicHub.API.Filters;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.AdminPayments.Queries.GetAdPackages;
using ClinicHub.Application.Features.Ads.Commands.AdPackages;
using ClinicHub.Application.Features.Ads.Commands.DeactivateAd;
using ClinicHub.Application.Features.Ads.Commands.UpdateClinicAdSettings;
using ClinicHub.Application.Features.Ads.Queries.GetAllAds;
using ClinicHub.Application.Features.Ads.Queries.GetClinicAdSettings;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace ClinicHub.API.Controllers.Version1;

[ApiVersion("1.0")]
[RoleAuthorize(nameof(UserType.SuperAdmin))]
public class AdminAdsController : BaseApiController
{
    private readonly IStringLocalizer<Messages> _localizer;

    public AdminAdsController(IMediator mediator, IStringLocalizer<Messages> localizer) : base(mediator)
    {
        _localizer = localizer;
    }

    [HttpGet]
    [Route(ApiRoutes.AdminAds.GetAll)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAds([FromQuery] GetAllAdsQuery query, CancellationToken ct)
    {
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpPost]
    [Route(ApiRoutes.AdminAds.Deactivate)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateAd(Guid id, [FromBody] DeactivateAdCommand command, CancellationToken ct)
    {
        command.Id = id;
        var result = await _mediator.Send(command, ct);
        return Ok(result, _localizer[LocalizationKeys.AdsMessages.Deactivated.Value]);
    }

    [HttpGet]
    [Route(ApiRoutes.AdminAds.Packages)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPackages(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAdPackagesQuery(), ct);
        return Ok(result);
    }

    [HttpPost]
    [Route(ApiRoutes.AdminAds.Packages)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePackage([FromBody] CreateAdPackageCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Created(ApiRoutes.AdminAds.Packages, result, _localizer[LocalizationKeys.AdsMessages.PackageCreated.Value]);
    }

    [HttpPut]
    [Route(ApiRoutes.AdminAds.PackageById)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePackage(Guid id, [FromBody] UpdateAdPackageCommand command, CancellationToken ct)
    {
        command.Id = id;
        var result = await _mediator.Send(command, ct);
        return Ok(result, _localizer[LocalizationKeys.AdsMessages.PackageUpdated.Value]);
    }

    [HttpDelete]
    [Route(ApiRoutes.AdminAds.PackageById)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeletePackage(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteAdPackageCommand { Id = id }, ct);
        return Ok(result, _localizer[LocalizationKeys.AdsMessages.PackageDeleted.Value]);
    }

    [HttpGet]
    [Route(ApiRoutes.AdminAds.ClinicAdSettings)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClinicAdSettings(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetClinicAdSettingsQuery(), ct);
        return Ok(result);
    }

    [HttpPut]
    [Route(ApiRoutes.AdminAds.ClinicAdSettingsById)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateClinicAdSettings(Guid clinicId, [FromBody] UpdateClinicAdSettingsCommand command, CancellationToken ct)
    {
        command.ClinicId = clinicId;
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }
}
