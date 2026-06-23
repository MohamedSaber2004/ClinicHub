using Asp.Versioning;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Booking.BookingConfig.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    public class BookingConfigurationsController : BaseApiController
    {
        public BookingConfigurationsController(IMediator mediator) : base(mediator)
        {
        }

        [HttpGet]
        [Route(ApiRoutes.BookingConfig.GetByClinic)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByClinic([FromRoute] Guid clinicId)
        {
            var query = new GetBookingConfigurationQuery { ClinicId = clinicId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
