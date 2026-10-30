using Asp.Versioning;
using ClinicHub.API.Filters;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.ClinicPayouts.Commands.UpsertClinicPayoutAccount;
using ClinicHub.Application.Features.ClinicPayouts.Queries.GetClinicPayoutAccount;
using ClinicHub.Application.Features.ClinicPayouts.Queries.GetClinicPayoutHistory;
using ClinicHub.Application.Features.ClinicPayouts.Queries.GetClinicPayoutSummary;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1;

[ApiVersion("1.0")]
[RoleAuthorize(nameof(UserType.ClinicOwner))]
public class ClinicPayoutsController : BaseApiController
{
    public ClinicPayoutsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [Route(ApiRoutes.ClinicPayouts.Account)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPayoutAccount(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetClinicPayoutAccountQuery(), ct);
        return Ok(result);
    }

    [HttpPut]
    [Route(ApiRoutes.ClinicPayouts.Account)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpsertPayoutAccount([FromBody] UpsertClinicPayoutAccountCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpGet]
    [Route(ApiRoutes.ClinicPayouts.Summary)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPayoutSummary(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetClinicPayoutSummaryQuery(), ct);
        return Ok(result);
    }

    [HttpGet]
    [Route(ApiRoutes.ClinicPayouts.History)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPayoutHistory([FromQuery] GetClinicPayoutHistoryQuery query, CancellationToken ct)
    {
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }
}
