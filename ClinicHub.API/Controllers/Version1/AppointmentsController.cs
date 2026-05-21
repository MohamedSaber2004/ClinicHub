using Asp.Versioning;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Appointments.Commands.CreateAppointment;
using ClinicHub.Application.Features.Appointments.Queries.GetAvailableSlots;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    public class AppointmentsController : BaseApiController
    {
        public AppointmentsController(IMediator mediator)
            : base(mediator)
        {
        }

        /// <summary>
        /// Create a new appointment.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route(ApiRoutes.Appointments.Create)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Get Available Time Slots for a Doctor on a Specific Date.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        [Route(ApiRoutes.Appointments.GetAvailableSlots)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAvailableSlots([FromQuery] GetAvailableSlotsQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
