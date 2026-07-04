using Asp.Versioning;
using ClinicHub.API.Filters;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Ratings.Commands.CreateRating;
using ClinicHub.Application.Features.Ratings.Queries.GetDoctorRatings;
using ClinicHub.Application.Features.Ratings.Queries.GetClinicRatings;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    [RoleAuthorize]
    public class RatingsController : BaseApiController
    {
        public RatingsController(IMediator mediator)
            : base(mediator)
        {
        }

        [HttpPost]
        [Route(ApiRoutes.Ratings.Create)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateRatingCommand command)
        {
            var result = await _mediator.Send(command);
            return Created("", result);
        }

        [HttpGet]
        [Route(ApiRoutes.Ratings.GetDoctorRatings)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDoctorRatings([FromRoute] Guid doctorId)
        {
            var query = new GetDoctorRatingsQuery { DoctorId = doctorId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet]
        [Route(ApiRoutes.Ratings.GetClinicRatings)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetClinicRatings([FromRoute] Guid clinicId)
        {
            var query = new GetClinicRatingsQuery { ClinicId = clinicId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
