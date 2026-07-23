using Asp.Versioning;
using ClinicHub.API.Filters;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Specializations.Commands.CreateSpecialization;
using ClinicHub.Application.Features.Specializations.Commands.DeleteSpecialization;
using ClinicHub.Application.Features.Specializations.Commands.UpdateSpecialization;
using ClinicHub.Application.Features.Specializations.Queries.GetActiveSpecializations;
using ClinicHub.Application.Features.Specializations.Queries.GetAllSpecializations;
using ClinicHub.Application.Features.Specializations.Queries.GetSpecializationById;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    public class SpecializationsController : BaseApiController
    {
        public SpecializationsController(IMediator mediator) : base(mediator)
        {
        }

        /// <summary>
        /// Get all active specializations (public, no auth required).
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [Route(ApiRoutes.Specializations.GetActive)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActive([FromQuery] GetActiveSpecializationsQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        /// <summary>
        /// Get all specializations with pagination support.
        /// </summary>
        [HttpGet]
        [RoleAuthorize(nameof(UserType.SuperAdmin))]
        [Route(ApiRoutes.Specializations.GetAll)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery]GetAllSpecializationsQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        /// <summary>
        /// get specialization by id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet]
        [RoleAuthorize(nameof(UserType.SuperAdmin))]
        [Route(ApiRoutes.Specializations.GetById)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetSpecializationByIdQuery(id), ct);
            return result != null ? Ok(result) : NotFound();
        }

        /// <summary>
        /// Create a new specialization.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost]
        [RoleAuthorize(nameof(UserType.SuperAdmin))]
        [Route(ApiRoutes.Specializations.Create)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateSpecializationCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        /// <summary>
        /// Update an existing specialization.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPut]
        [RoleAuthorize(nameof(UserType.SuperAdmin))]
        [Route(ApiRoutes.Specializations.Update)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromBody] UpdateSpecializationCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        /// <summary>
        /// Delete a specialization by id.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpDelete]
        [RoleAuthorize(nameof(UserType.SuperAdmin))]
        [Route(ApiRoutes.Specializations.Delete)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromBody] DeleteSpecializationCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Deleted(result);
        }
    }
}
