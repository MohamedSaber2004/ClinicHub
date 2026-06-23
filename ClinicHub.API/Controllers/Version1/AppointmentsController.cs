using Asp.Versioning;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Appointments.Commands.CreateAppointment;
using ClinicHub.Application.Features.Appointments.Commands.UpdateAppointment;
using ClinicHub.Application.Features.Appointments.Commands.DeleteAppointment;
using ClinicHub.Application.Features.Appointments.Queries.GetAllAppointmentsWithFilters;
using ClinicHub.Application.Features.Appointments.Queries.GetAppointmentById;
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
        /// Get all appointments with filters.
        /// </summary>
        [Authorize]
        [HttpGet]
        [Route(ApiRoutes.Appointments.GetAll)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllAppointmentsWithFiltersQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get appointment by ID.
        /// </summary>
        [HttpGet]
        [Route(ApiRoutes.Appointments.GetById)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var query = new GetAppointmentByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Create a new appointment.
        /// </summary>
        [Authorize]
        [HttpPost]
        [Route(ApiRoutes.Appointments.Create)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentCommand command)
        {
            var result = await _mediator.Send(command);
            return Created(ApiRoutes.Appointments.GetById, result);
        }

        /// <summary>
        /// Update an existing appointment.
        /// </summary>
        [Authorize]
        [HttpPut]
        [Route(ApiRoutes.Appointments.Update)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAppointmentCommand command)
        {
            command.AppointmentId = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Delete an appointment.
        /// </summary>
        [Authorize]
        [HttpDelete]
        [Route(ApiRoutes.Appointments.Delete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteAppointmentCommand { AppointmentId = id };
            var result = await _mediator.Send(command);
                return NoContent();
        }
    }
}
