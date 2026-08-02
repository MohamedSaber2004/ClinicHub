using Asp.Versioning;
using ClinicHub.API.Filters;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.AdminPayments.Commands.CreateAdsOrder;
using ClinicHub.Application.Features.AdminPayments.Commands.CreateManualPayment;
using ClinicHub.Application.Features.AdminPayments.Commands.RefundPayment;
using ClinicHub.Application.Features.AdminPayments.Queries.GetAdminPaymentDetail;
using ClinicHub.Application.Features.AdminPayments.Queries.GetAdminPayments;
using ClinicHub.Application.Features.AdminPayments.Queries.GetAdminPaymentStats;
using ClinicHub.Application.Features.AdminPayments.Queries.GetAdPackages;
using ClinicHub.Application.Features.AdminPayments.Queries.GetEligibleAdsClinics;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1;

[ApiVersion("1.0")]
[RoleAuthorize(nameof(UserType.SuperAdmin))]
public class AdminPaymentsController : BaseApiController
{
    public AdminPaymentsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [Route(ApiRoutes.AdminPayments.GetAll)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayments([FromQuery] GetAdminPaymentsQuery query, CancellationToken ct)
    {
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet]
    [Route(ApiRoutes.AdminPayments.GetById)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentDetail(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAdminPaymentDetailQuery { PaymentId = id }, ct);
        return Ok(result);
    }

    [HttpGet]
    [Route(ApiRoutes.AdminPayments.Stats)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentStats([FromQuery] GetAdminPaymentStatsQuery query, CancellationToken ct)
    {
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpPost]
    [Route(ApiRoutes.AdminPayments.Manual)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateManualPayment([FromBody] CreateManualPaymentCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Created(ApiRoutes.AdminPayments.GetById.Replace("{id:guid}", result.Id.ToString()), result);
    }

    [HttpPost]
    [Route(ApiRoutes.AdminPayments.Refund)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RefundPayment(Guid id, [FromBody] RefundPaymentCommand command, CancellationToken ct)
    {
        command.PaymentId = id;
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpGet]
    [Route(ApiRoutes.AdminAds.EligibleClinics)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEligibleAdsClinics(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetEligibleAdsClinicsQuery(), ct);
        return Ok(result);
    }

    [HttpGet]
    [Route(ApiRoutes.AdminAds.Packages)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdPackages(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAdPackagesQuery(), ct);
        return Ok(result);
    }

    [HttpPost]
    [Route(ApiRoutes.AdminAds.Orders)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAdsOrder([FromBody] CreateAdsOrderCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Created(ApiRoutes.AdminPayments.GetById.Replace("{id:guid}", result.PaymentId.ToString()), result);
    }
}
