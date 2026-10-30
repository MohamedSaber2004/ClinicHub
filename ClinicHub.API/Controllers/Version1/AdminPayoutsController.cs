using Asp.Versioning;
using ClinicHub.API.Filters;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.AdminPayouts.Commands.ApprovePayoutBatch;
using ClinicHub.Application.Features.AdminPayouts.Commands.CancelPayoutBatch;
using ClinicHub.Application.Features.AdminPayouts.Commands.CreatePayoutBatch;
using ClinicHub.Application.Features.AdminPayouts.Commands.CreditClinicLedger;
using ClinicHub.Application.Features.AdminPayouts.Commands.ExecutePayoutBatch;
using ClinicHub.Application.Features.AdminPayouts.Commands.RejectPayoutAccount;
using ClinicHub.Application.Features.AdminPayouts.Commands.RetryPayoutItem;
using ClinicHub.Application.Features.AdminPayouts.Commands.UpsertPayoutAccountAsAdmin;
using ClinicHub.Application.Features.AdminPayouts.Commands.VerifyPayoutAccount;
using ClinicHub.Application.Features.AdminPayouts.Queries.GetPayoutBatchDetails;
using ClinicHub.Application.Features.AdminPayouts.Queries.GetPayoutBatches;
using ClinicHub.Application.Features.AdminPayouts.Queries.GetPayoutBudget;
using ClinicHub.Application.Features.AdminPayouts.Queries.GetPayoutAccounts;
using ClinicHub.Application.Features.AdminPayouts.Queries.GetPayoutPreview;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1;

[ApiVersion("1.0")]
[RoleAuthorize(nameof(UserType.SuperAdmin))]
public class AdminPayoutsController : BaseApiController
{
    public AdminPayoutsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [Route(ApiRoutes.AdminPayouts.Preview)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPreview([FromQuery] GetPayoutPreviewQuery query, CancellationToken ct)
    {
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet]
    [Route(ApiRoutes.AdminPayouts.Accounts)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccounts([FromQuery] GetPayoutAccountsQuery query, CancellationToken ct)
    {
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpPost]
    [Route(ApiRoutes.AdminPayouts.VerifyAccount)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyAccount(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new VerifyPayoutAccountCommand { AccountId = id }, ct);
        return Ok(result);
    }

    [HttpPost]
    [Route(ApiRoutes.AdminPayouts.RejectAccount)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectAccount(Guid id, [FromBody] RejectPayoutAccountCommand command, CancellationToken ct)
    {
        command.AccountId = id;
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPut]
    [Route(ApiRoutes.AdminPayouts.UpsertClinicAccount)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpsertClinicAccount(Guid clinicId, [FromBody] UpsertPayoutAccountAsAdminCommand command, CancellationToken ct)
    {
        command.ClinicId = clinicId;
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPost]
    [Route(ApiRoutes.AdminPayouts.CreditClinic)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreditClinic([FromBody] CreditClinicLedgerCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpGet]
    [Route(ApiRoutes.AdminPayouts.Budget)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBudget(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPayoutBudgetQuery(), ct);
        return Ok(result);
    }

    [HttpGet]
    [Route(ApiRoutes.AdminPayouts.Batches)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBatches([FromQuery] GetPayoutBatchesQuery query, CancellationToken ct)
    {
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet]
    [Route(ApiRoutes.AdminPayouts.BatchById)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBatch(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPayoutBatchDetailsQuery { BatchId = id }, ct);
        return Ok(result);
    }

    [HttpPost]
    [Route(ApiRoutes.AdminPayouts.CreateBatch)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBatch([FromBody] CreatePayoutBatchCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPost]
    [Route(ApiRoutes.AdminPayouts.ApproveBatch)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveBatch(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ApprovePayoutBatchCommand { BatchId = id }, ct);
        return Ok(result);
    }

    [HttpPost]
    [Route(ApiRoutes.AdminPayouts.ExecuteBatch)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExecuteBatch(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ExecutePayoutBatchCommand { BatchId = id }, ct);
        return Ok(result);
    }

    [HttpPost]
    [Route(ApiRoutes.AdminPayouts.CancelBatch)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelBatch(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelPayoutBatchCommand { BatchId = id }, ct);
        return Ok(result);
    }

    [HttpPost]
    [Route(ApiRoutes.AdminPayouts.RetryItem)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetryItem(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new RetryPayoutItemCommand { ItemId = id }, ct);
        return Ok(result);
    }
}
