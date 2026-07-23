using Asp.Versioning;
using ClinicHub.API.Filters;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Subscriptions.Commands.CancelMySubscription;
using ClinicHub.Application.Features.Subscriptions.Commands.CreateSubscription;
using ClinicHub.Application.Features.Subscriptions.Queries.GetMyClinicSubscription;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    public class SubscriptionsController : BaseApiController
    {
        public SubscriptionsController(IMediator mediator) : base(mediator)
        {
        }

        [AllowAnonymous]
        [HttpPost]
        [Route(ApiRoutes.Subscriptions.Create)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateSubscriptionCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Created(ApiRoutes.Subscriptions.Create, result);
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
