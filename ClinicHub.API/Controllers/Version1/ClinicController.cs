using Asp.Versioning;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Clinics.Queries.GetHybridSearch;
using ClinicHub.Application.Features.Clinics.Queries.GetRoute;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    public class ClinicController : BaseApiController
    {
        public ClinicController(IMediator mediator) : base(mediator)
        {
        }

        [HttpGet]
        [Route(ApiRoutes.Clinics.Search)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize]
        public async Task<IActionResult> Search([FromQuery] GetHybridSearchQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet]
        [Route(ApiRoutes.Clinics.GetRoute)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRoute([FromQuery] GetRouteQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
