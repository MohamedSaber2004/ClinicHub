using Asp.Versioning;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Users.Commands.AddUser;
using ClinicHub.Application.Features.Users.Commands.AssignUserRole;
using ClinicHub.Application.Features.Users.Commands.DeleteUser;
using ClinicHub.Application.Features.Users.Commands.EditUser;
using ClinicHub.Application.Features.Users.Commands.EditUserRole;
using ClinicHub.Application.Features.Users.Queries.GetAllUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    //[Authorize(Roles = "Admin")]
    public class UsersController : BaseApiController
    {
        public UsersController(IMediator mediator) : base(mediator)
        {
        }

        /// <summary>
        /// Gets a paginated list of all users.
        /// </summary>
        [HttpGet]
        [Route(ApiRoutes.Users.GetAll)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllUsersQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        /// <summary>
        /// Creates a new user and optionally assigns a role.
        /// </summary>
        [HttpPost]
        [Route(ApiRoutes.Users.Add)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Add([FromBody] AddUserCommand command, CancellationToken ct)
        {
            var id = await _mediator.Send(command, ct);
            return Created(ApiRoutes.Users.Edit.Replace("{id:guid}", id.ToString()), new { id });
        }

        /// <summary>
        /// Updates an existing user's profile details.
        /// </summary>
        [HttpPut]
        [Route(ApiRoutes.Users.Edit)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Edit([FromRoute] Guid id, [FromBody] EditUserCommand command, CancellationToken ct)
        {
            command.Id = id;
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        /// <summary>
        /// Soft deletes a user by ID.
        /// </summary>
        [HttpDelete]
        [Route(ApiRoutes.Users.Delete)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeleteUserCommand(id), ct);
            return Ok(result);
        }

        /// <summary>
        /// Assigns a role to a user (adds without removing existing roles).
        /// </summary>
        [HttpPost]
        [Route(ApiRoutes.Users.AssignRole)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AssignRole([FromRoute] Guid id, [FromBody] AssignUserRoleCommand command, CancellationToken ct)
        {
            command.UserId = id;
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        /// <summary>
        /// Replaces the user's current roles with a new role.
        /// </summary>
        [HttpPut]
        [Route(ApiRoutes.Users.EditRole)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> EditRole([FromRoute] Guid id, [FromBody] EditUserRoleCommand command, CancellationToken ct)
        {
            command.UserId = id;
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
    }
}
