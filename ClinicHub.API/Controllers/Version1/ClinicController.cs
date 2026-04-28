using Asp.Versioning;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Clinics.Queries.GetHybridSearch;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    [Authorize]
    public class ClinicController : BaseApiController
    {
        public ClinicController(IMediator mediator) : base(mediator)
        {
        }

        /// <summary>
        /// Search on clinic or hospital nearby
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.Clinics.Search)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Search([FromQuery] GetHybridSearchQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
