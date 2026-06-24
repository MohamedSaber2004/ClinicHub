using Asp.Versioning;
using ClinicHub.API.Filters;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Clinics.Commands.ActivateClinic;
using ClinicHub.Application.Features.Clinics.Commands.CreateClinic;
using ClinicHub.Application.Features.Clinics.Commands.DeactivateClinic;
using ClinicHub.Application.Features.Clinics.Commands.UpdateClinic;
using ClinicHub.Application.Features.Clinics.DTOs;
using ClinicHub.Application.Features.Clinics.Queries.GetAllClinics;
using ClinicHub.Application.Features.Clinics.Queries.GetClinicById;
using ClinicHub.Application.Features.Clinics.Queries.GetHybridSearch;
using ClinicHub.Application.Features.Clinics.Queries.GetPaginatedClinics;
using ClinicHub.Application.Features.Clinics.Queries.GetRoute;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    public class ClinicController : BaseApiController
    {
        public ClinicController(IMediator mediator) : base(mediator)
        {
        }

        /// <summary>
        /// Search clinics based on the provided query parameters.
        /// </summary>
        /// <param name="query">The query parameters for searching clinics.</param>
        /// <returns>A list of clinics matching the search criteria.</returns>
        [HttpGet]
        [RoleAuthorize(nameof(UserType.User))]
        [Route(ApiRoutes.Clinics.Search)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Search([FromQuery] GetHybridSearchQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get the route between two locations based on the provided query parameters.
        /// </summary>
        /// <param name="query">The query parameters for getting the route.</param>
        /// <returns>The route information between the specified locations.</returns>
        [HttpGet]
        [RoleAuthorize(nameof(UserType.User))]
        [Route(ApiRoutes.Clinics.GetRoute)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRoute([FromQuery] GetRouteQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Creates a new clinic based on the provided data transfer object (DTO).
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [RoleAuthorize(nameof(UserType.SuperAdmin))]
        [Route(ApiRoutes.ClinicManagement.Create)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateClinicDto dto)
        {
            var result = await _mediator.Send(new CreateClinicCommand(dto));
            return Created(nameof(GetById), result);
        }

        /// <summary>
        /// Updates an existing clinic identified by the provided ID 
        /// using the data from the provided data transfer object (DTO).
        /// </summary>
        /// <param name="id">The ID of the clinic to update.</param>
        /// <param name="dto">The data transfer object containing the updated clinic information.</param>
        /// <returns>The updated clinic information.</returns>
        [HttpPut]
        [Route(ApiRoutes.ClinicManagement.Update)]
        [RoleAuthorize(nameof(UserType.SuperAdmin), nameof(UserType.ClinicOwner))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClinicDto dto)
        {
            var result = await _mediator.Send(new UpdateClinicCommand(id, dto));
            return Ok(result);
        }

        /// <summary>
        /// Activates a clinic identified by the provided ID.
        /// </summary>
        /// <param name="id">The ID of the clinic to activate.</param>
        /// <returns>The activated clinic information.</returns>
        [HttpPatch]
        [Route(ApiRoutes.ClinicManagement.Activate)]
        [RoleAuthorize(nameof(UserType.SuperAdmin))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Activate(Guid id)
        {
            var result = await _mediator.Send(new ActivateClinicCommand(id));
            return Ok(result);
        }

        /// <summary>
        /// Deactivates a clinic identified by the provided ID.
        /// </summary>
        /// <param name="id">The ID of the clinic to deactivate.</param>
        /// <returns>The deactivated clinic information.</returns>
        [HttpPatch]
        [RoleAuthorize(nameof(UserType.SuperAdmin))]
        [Route(ApiRoutes.ClinicManagement.Deactivate)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            var result = await _mediator.Send(new DeactivateClinicCommand(id));
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a clinic by its unique identifier (ID).
        /// </summary>
        /// <param name="id">The ID of the clinic to retrieve.</param>
        /// <returns>The clinic information.</returns>
        [HttpGet]
        [Route(ApiRoutes.ClinicManagement.GetById)]
        [RoleAuthorize(nameof(UserType.SuperAdmin), nameof(UserType.ClinicOwner), nameof(UserType.Doctor), nameof(UserType.User))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetClinicByIdQuery(id));
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a list of all clinics based on the provided query parameters.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.ClinicManagement.GetAll)]
        [RoleAuthorize(nameof(UserType.SuperAdmin), nameof(UserType.ClinicOwner), nameof(UserType.User))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllClinicsQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a paginated list of clinics based on the provided query parameters.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.ClinicManagement.GetPaginated)]
        [RoleAuthorize(nameof(UserType.SuperAdmin))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPaginated([FromQuery] GetPaginatedClinicsQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
