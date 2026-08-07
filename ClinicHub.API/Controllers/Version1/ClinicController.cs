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
using ClinicHub.Application.Features.Clinics.Queries.GetClinicByIdForUser;
using ClinicHub.Application.Features.Clinics.Queries.GetHybridSearch;
using ClinicHub.Application.Features.Clinics.Queries.GetPaginatedClinics;
using ClinicHub.Application.Features.Clinics.Queries.GetRoute;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    //[RoleAuthorize]
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
        [Route(ApiRoutes.Clinics.GetRoute)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRoute([FromQuery] GetRouteQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a list of all clinics based on the provided query parameters.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.Clinics.GetAll)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllClinicsQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get Clinic By Id For User
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.Clinics.GetByIdForUser)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetClinicById(Guid id)
        {
            var result = await _mediator.Send(new GetClinicByIdForUserQuery(id));
            return Ok(result);
        }
    }
}
