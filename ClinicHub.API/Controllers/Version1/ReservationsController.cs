using Asp.Versioning;
using ClinicHub.API.Filters;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Appointments.Commands.CreateAppointment;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    [RoleAuthorize]
    public class ReservationsController : BaseApiController
    {
        public ReservationsController(IMediator mediator) : base(mediator)
        {
        }

        [HttpPost]
        [Route(ApiRoutes.Reservations.Create)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentCommand command)
        {
            var result = await _mediator.Send(command);
            return Created(ApiRoutes.Reservations.Create, result);
        }
    }
}
