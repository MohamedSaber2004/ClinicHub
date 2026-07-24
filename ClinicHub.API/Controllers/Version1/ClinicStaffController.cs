using Asp.Versioning;
using ClinicHub.API.Filters;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.ClinicStaff.Commands.CreateStaff;
using ClinicHub.Application.Features.ClinicStaff.Commands.DeleteStaff;
using ClinicHub.Application.Features.ClinicStaff.Commands.UpdateStaff;
using ClinicHub.Application.Features.ClinicStaff.Queries.GetClinicStaff;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    [RoleAuthorize(nameof(UserType.ClinicOwner))]
    [RequirePlanPermission(SubscriptionPermission.ManageStaff)]
    public class ClinicStaffController : BaseApiController
    {
        public ClinicStaffController(IMediator mediator) : base(mediator)
        {
        }

        [HttpGet]
        [Route(ApiRoutes.ClinicStaff.GetAll)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] GetClinicStaffQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        [HttpPost]
        [Route(ApiRoutes.ClinicStaff.Create)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateStaffCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Created(ApiRoutes.ClinicStaff.Create, result);
        }

        [HttpPut]
        [Route(ApiRoutes.ClinicStaff.Update)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStaffCommand command, CancellationToken ct)
        {
            command.StaffId = id;
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [HttpDelete]
        [Route(ApiRoutes.ClinicStaff.Delete)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeleteStaffCommand { StaffId = id }, ct);
            return Ok(result);
        }
    }
}
