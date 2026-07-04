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
using ClinicHub.Application.Features.UserClinics.Commands.FollowClinic;
using ClinicHub.Application.Features.UserClinics.Commands.UnfollowClinic;
using ClinicHub.Application.Features.UserClinics.Queries.GetFollowedClinics;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    [RoleAuthorize]
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

        /// <summary>
        /// Follow a clinic for the authenticated user.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Clinics.Follow)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> FollowClinic(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new FollowClinicCommand { ClinicId = id }, ct);
            return Ok(result);
        }

        /// <summary>
        /// Unfollow a clinic for the authenticated user.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Clinics.Unfollow)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UnfollowClinic(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new UnfollowClinicCommand { ClinicId = id }, ct);
            return Ok(result);
        }

        /// <summary>
        /// Get the list of clinics followed by the authenticated user.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.Clinics.Followed)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetFollowedClinics(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetFollowedClinicsQuery(), ct);
            return Ok(result);
        }
    }
}
