using Asp.Versioning;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Availability.Commands.CreateNewAvailability;
using ClinicHub.Application.Features.Availability.Commands.DeleteAvailability;
using ClinicHub.Application.Features.Availability.Commands.UpdateExistingAvailability;
using ClinicHub.Application.Features.Availability.Queries.GetAvailableSlots;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    public class AvailabilityController: BaseApiController
    {
        public AvailabilityController(IMediator mediator): base(mediator)
        {
        }

        /// <summary>
        /// Get All Availability
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        [Route(ApiRoutes.Availability.GetAllAvailability)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllAvailability([FromQuery] GetAvailableSlotsQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Create New Availability
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route(ApiRoutes.Availability.Create)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateNewAvailability([FromBody]CreateNewAvailabilityCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Update Existing Availability
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPut]
        [Route(ApiRoutes.Availability.Update)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType (StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAvailaiblity(Guid id,[FromBody] UpdateExistingAvailabilityCommand command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Delete Availability
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete]
        [Route(ApiRoutes.Availability.Delete)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAvailability(Guid id)
        {
            var result = await _mediator.Send(new DeleteAvailabilityCommand { Id = id });
            return Ok(result);
        }
    }
}
