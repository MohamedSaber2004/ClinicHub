using Asp.Versioning;
using ClinicHub.API.Filters;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Payment.Commands.ConfirmPaymentWebhook;
using ClinicHub.Application.Features.Payment.Commands.InitiateBookingPayment;
using ClinicHub.Application.Features.Payment.Commands.InitiatePayment;
using ClinicHub.Application.Features.Payment.Commands.VerifyBookingPayment;
using ClinicHub.Application.Features.Payment.Queries.GetPaymentStatus;
using ClinicHub.Application.Features.Payment.Queries.VerifyLatestSubscriptionPayment;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1;

[ApiVersion("1.0")]
[RoleAuthorize]
public class PaymentsController : BaseApiController
{
    public PaymentsController(IMediator mediator) : base(mediator)
    {
    }

    [Authorize]
    [HttpPost]
    [Route(ApiRoutes.Payments.Initiate)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Initiate([FromBody] InitiatePaymentCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    [Route(ApiRoutes.Payments.CreateBooking)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBookingPayment([FromBody] InitiateBookingPaymentCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    [Route(ApiRoutes.Payments.VerifyBooking)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyBookingPayment([FromBody] VerifyBookingPaymentCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost]
    [Route(ApiRoutes.Payments.Webhook)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Webhook(
        [FromQuery] string? hmac,
        [FromBody] ConfirmPaymentWebhookRequestDto request)
    {
        var command = new ConfirmPaymentWebhookCommand
        {
            Hmac = !string.IsNullOrWhiteSpace(hmac) ? hmac : request.Hmac,
            Type = request.Type,
            Transaction = request.Transaction
        };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [Authorize]
    [HttpGet]
    [Route(ApiRoutes.Payments.GetStatus)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetStatus([FromRoute] Guid appointmentId)
    {
        var query = new GetPaymentStatusQuery { AppointmentId = appointmentId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Verifies the clinic's most recent subscription payment (local state first,
    /// then a direct Paymob inquiry) and activates the subscription idempotently
    /// when Paymob confirms the money was captured. Used after the user returns
    /// from the payment gateway when the webhook has not arrived yet.
    /// </summary>
    [Authorize]
    [HttpGet]
    [Route(ApiRoutes.Payments.VerifyLatestSubscription)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyLatestSubscriptionPayment(CancellationToken ct)
    {
        var result = await _mediator.Send(new VerifyLatestSubscriptionPaymentQuery(), ct);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet]
    [Route(ApiRoutes.Payments.Result)]
    public IActionResult PaymentResult(
        [FromQuery] bool success)
    {
        var redirectUrl = $"/payment/result.html?success={success}";
        return Redirect(redirectUrl);
    }
}
