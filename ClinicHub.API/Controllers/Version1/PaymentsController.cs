using Asp.Versioning;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Payment.Commands.ConfirmPaymentWebhook;
using ClinicHub.Application.Features.Payment.Commands.InitiatePayment;
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

    /// <summary>
    /// Initiate payment.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Handles incoming payment webhook notifications from Paymob and processes the payment confirmation.
    /// </summary>
    /// <param name="request">The payment webhook request containing HMAC and transaction data.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the webhook processing.</returns>
    [HttpPost]
    [Route(ApiRoutes.Payments.Webhook)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Webhook([FromBody] ConfirmPaymentWebhookRequestDto request)
    {
        var command = new ConfirmPaymentWebhookCommand
        {
            Hmac = request.Hmac,
            TransactionData = request.TransactionData
        };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Get payment status for a specific appointment.
    /// </summary>
    /// <param name="appointmentId">The ID of the appointment.</param>
    /// <returns>An <see cref="IActionResult"/> containing the payment status.</returns>
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult PaymentResult([FromQuery] bool success)
    {
        // ✅ FIXED: removed duplicate nested if
        if (success == true)
        {
            return Ok(new { message = "تم الدفع بنجاح!" });
        }

        return BadRequest(new { message = "فشل الدفع" });
    }
}
