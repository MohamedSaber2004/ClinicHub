using Asp.Versioning;
using ClinicHub.API.Filters;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Payment.Commands.ConfirmPaymentWebhook;
using ClinicHub.Application.Features.Payment.Commands.InitiateBookingPayment;
using ClinicHub.Application.Features.Payment.Commands.InitiatePayment;
using ClinicHub.Application.Features.Payment.Commands.VerifyBookingPayment;
using ClinicHub.Application.Features.Payment.Queries.GetPaymentStatus;
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

    [AllowAnonymous]
    [HttpGet]
    [Route(ApiRoutes.Payments.Result)]
    public IActionResult PaymentResult([FromQuery] bool success)
    {
        var html = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{(success ? "Payment Successful - ClinicHub" : "Payment Failed - ClinicHub")}</title>
    <link href=""https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700&display=swap"" rel=""stylesheet"">
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
            font-family: 'Plus Jakarta Sans', -apple-system, BlinkMacSystemFont, sans-serif;
        }}
        body {{
            background: #0f172a;
            color: #f8fafc;
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }}
        .card {{
            background: rgba(30, 41, 59, 0.85);
            border: 1px solid rgba(255, 255, 255, 0.1);
            backdrop-filter: blur(16px);
            border-radius: 24px;
            padding: 44px 36px;
            max-width: 440px;
            width: 100%;
            text-align: center;
            box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.5);
            animation: fadeIn 0.4s ease-out;
        }}
        @keyframes fadeIn {{
            from {{ opacity: 0; transform: translateY(16px); }}
            to {{ opacity: 1; transform: translateY(0); }}
        }}
        .icon-box {{
            width: 84px;
            height: 84px;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0 auto 24px;
            font-size: 38px;
            font-weight: 700;
        }}
        .success-icon {{
            background: rgba(34, 197, 94, 0.15);
            color: #22c55e;
            border: 2px solid #22c55e;
            box-shadow: 0 0 24px rgba(34, 197, 94, 0.35);
        }}
        .error-icon {{
            background: rgba(239, 68, 68, 0.15);
            color: #ef4444;
            border: 2px solid #ef4444;
            box-shadow: 0 0 24px rgba(239, 68, 68, 0.35);
        }}
        h1 {{
            font-size: 24px;
            font-weight: 700;
            margin-bottom: 12px;
            color: #ffffff;
        }}
        p {{
            color: #94a3b8;
            font-size: 15px;
            line-height: 1.6;
            margin-bottom: 28px;
        }}
        .countdown-box {{
            background: rgba(15, 23, 42, 0.6);
            border: 1px solid rgba(255, 255, 255, 0.05);
            border-radius: 14px;
            padding: 14px 18px;
            font-size: 14px;
            color: #cbd5e1;
            margin-bottom: 24px;
        }}
        .counter {{
            font-weight: 700;
            color: {(success ? "#22c55e" : "#ef4444")};
            font-size: 18px;
        }}
        .btn {{
            display: inline-block;
            width: 100%;
            padding: 14px 24px;
            background: {(success ? "linear-gradient(135deg, #10b981, #059669)" : "linear-gradient(135deg, #ef4444, #dc2626)")};
            color: #ffffff;
            border: none;
            border-radius: 12px;
            font-size: 15px;
            font-weight: 600;
            text-decoration: none;
            cursor: pointer;
            transition: all 0.2s ease;
            box-shadow: 0 4px 14px {(success ? "rgba(16, 185, 129, 0.35)" : "rgba(239, 68, 68, 0.35)")};
        }}
        .btn:hover {{
            opacity: 0.95;
            transform: translateY(-1px);
        }}
    </style>
</head>
<body>
    <div class=""card"">
        <div class=""icon-box {(success ? "success-icon" : "error-icon")}"">
            {(success ? "✓" : "✕")}
        </div>
        <h1>{(success ? "Payment Successful!" : "Payment Failed")}</h1>
        <p>{(success ? "Your subscription payment was processed successfully. Your clinic dashboard features are now active." : "We couldn't process your payment. Please try again or choose another payment method.")}</p>
        
        <div class=""countdown-box"">
            Redirecting to home in <span id=""countdown"" class=""counter"">5</span> seconds...
        </div>

        <a href=""/"" id=""redirectBtn"" class=""btn"">Return to Dashboard Now</a>
    </div>

    <script>
        let timeLeft = 5;
        const countdownEl = document.getElementById('countdown');
        const isSuccess = {(success ? "true" : "false")};

        if (window.opener && !window.opener.closed) {{
            try {{
                window.opener.postMessage({{ type: 'PAYMENT_RESULT', success: isSuccess }}, '*');
            }} catch (e) {{ console.error(e); }}
        }}

        function redirectNow() {{
            window.location.href = '/';
        }}

        const timer = setInterval(() => {{
            timeLeft--;
            if (countdownEl) countdownEl.innerText = timeLeft;
            if (timeLeft <= 0) {{
                clearInterval(timer);
                redirectNow();
            }}
        }}, 1000);
    </script>
</body>
</html>";

        return Content(html, "text/html");
    }
}
