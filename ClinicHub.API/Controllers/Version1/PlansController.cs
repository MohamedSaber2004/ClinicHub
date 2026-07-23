using Asp.Versioning;
using ClinicHub.API.Filters;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Plans.Commands.CreatePlan;
using ClinicHub.Application.Features.Plans.Commands.DeletePlan;
using ClinicHub.Application.Features.Plans.Commands.UpdatePlan;
using ClinicHub.Application.Features.Plans.Queries.GetActivePlans;
using ClinicHub.Application.Features.Plans.Queries.GetAllPlans;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    public class PlansController : BaseApiController
    {
        public PlansController(IMediator mediator) : base(mediator)
        {
        }

        [AllowAnonymous]
        [HttpGet]
        [Route(ApiRoutes.Plans.GetAllActive)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActivePlans(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetActivePlansQuery(), ct);
            return Ok(result);
        }

        [HttpGet]
        [Route(ApiRoutes.Plans.GetAll)]
        [RoleAuthorize(nameof(UserType.SuperAdmin))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllPlans([FromQuery] GetAllPlansQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        [HttpPost]
        [Route(ApiRoutes.Plans.Create)]
        [RoleAuthorize(nameof(UserType.SuperAdmin))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePlan([FromBody] CreatePlanCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Created(ApiRoutes.Plans.Create, result);
        }

        [HttpPut]
        [Route(ApiRoutes.Plans.Update)]
        [RoleAuthorize(nameof(UserType.SuperAdmin))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePlan(Guid id, [FromBody] UpdatePlanCommand command, CancellationToken ct)
        {
            command.Id = id;
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [HttpDelete]
        [Route(ApiRoutes.Plans.Delete)]
        [RoleAuthorize(nameof(UserType.SuperAdmin))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePlan(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeletePlanCommand { Id = id }, ct);
            return Ok(result);
        }
    }
}
