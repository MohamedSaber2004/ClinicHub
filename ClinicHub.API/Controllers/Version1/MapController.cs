using Asp.Versioning;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Maps.Queries.GetRoute;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    public class MapController : BaseApiController
    {
        public MapController(IMediator mediator) : base(mediator)
        {
        }

        /// <summary>
        /// Get driving route between two coordinates.
        /// Returns distance (meters), duration (seconds), and geometry (list of [lng, lat] points).
        /// </summary>
        [HttpGet]
        [Route(ApiRoutes.Maps.Route)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRoute([FromQuery] GetRouteQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
