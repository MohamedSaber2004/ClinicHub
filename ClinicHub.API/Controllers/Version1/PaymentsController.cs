using Asp.Versioning;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Payment.Commands.ConfirmPaymentWebhook;
using ClinicHub.Application.Features.Payment.Commands.InitiateBookingPayment;
using ClinicHub.Application.Features.Payment.Commands.InitiatePayment;
using ClinicHub.Application.Features.Payment.Commands.VerifyBookingPayment;
using ClinicHub.Application.Features.Payment.Queries.GetPaymentStatus;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1;

[ApiVersion("1.0")]
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

    [HttpPost]
    [Route(ApiRoutes.Payments.Webhook)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Webhook(
        [FromQuery] string hmac,
        [FromBody] ConfirmPaymentWebhookRequestDto request)
    {
        var command = new ConfirmPaymentWebhookCommand
        {
            Hmac = hmac,
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

    [HttpGet]
    [Route(ApiRoutes.Payments.Result)]
    public IActionResult PaymentResult(
        [FromQuery] long id,
        [FromQuery] bool success,
        [FromQuery] long order)
    {
        var redirectUrl = $"/payment/result.html?success={success}&transactionId={id}&orderId={order}";
        return Redirect(redirectUrl);
    }
}
