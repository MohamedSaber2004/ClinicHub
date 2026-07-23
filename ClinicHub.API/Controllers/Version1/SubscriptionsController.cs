using Asp.Versioning;
using ClinicHub.API.Filters;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Subscriptions.Commands.CancelMySubscription;
using ClinicHub.Application.Features.Subscriptions.Commands.InitiateSubscriptionPayment;
using ClinicHub.Application.Features.Subscriptions.Queries.GetMyClinicSubscription;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    public class SubscriptionsController : BaseApiController
    {
        public SubscriptionsController(IMediator mediator) : base(mediator)
        {
        }

        [HttpPost]
        [Route(ApiRoutes.Subscriptions.InitiatePayment)]
        [RoleAuthorize(nameof(UserType.ClinicOwner))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> InitiatePayment([FromBody] InitiateSubscriptionPaymentCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [HttpGet]
        [Route(ApiRoutes.Subscriptions.MySubscription)]
        [RoleAuthorize(nameof(UserType.ClinicOwner))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMySubscription(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetMyClinicSubscriptionQuery(), ct);
            return Ok(result);
        }

        [HttpPost]
        [Route(ApiRoutes.Subscriptions.CancelMySubscription)]
        [RoleAuthorize(nameof(UserType.ClinicOwner))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CancelMySubscription(CancellationToken ct)
        {
            var result = await _mediator.Send(new CancelMySubscriptionCommand(), ct);
            return Ok(result);
        }
    }
}
