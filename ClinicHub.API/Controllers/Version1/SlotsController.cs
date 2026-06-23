using Asp.Versioning;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Availability.Queries.GetAvailableSlots;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    public class SlotsController : BaseApiController
    {
        public SlotsController(IMediator mediator) : base(mediator)
        {
        }

        [HttpGet]
        [Route(ApiRoutes.Slots.GetByDoctor)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByDoctor(
            [FromRoute] Guid clinicId,
            [FromRoute] Guid doctorId,
            [FromQuery] DateTime date)
        {
            var query = new GetAvailableSlotsQuery
            {
                ClinicId = clinicId,
                DoctorId = doctorId,
                Date = date
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
