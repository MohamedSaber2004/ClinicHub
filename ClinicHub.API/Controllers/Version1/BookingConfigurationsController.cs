using Asp.Versioning;
using ClinicHub.API.Filters;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Booking.BookingConfig.Commands.CreateBookingConfig;
using ClinicHub.Application.Features.Booking.BookingConfig.Commands.UpdateBookingConfig;
using ClinicHub.Application.Features.Booking.BookingConfig.DTOs;
using ClinicHub.Application.Features.Booking.BookingConfig.Queries;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    public class BookingConfigurationsController : BaseApiController
    {
        public BookingConfigurationsController(IMediator mediator) : base(mediator)
        {
        }

        /// <summary>
        /// Retrieves the booking configuration for a specific clinic.
        /// </summary>
        /// <param name="clinicId">The ID of the clinic.</param>
        /// <returns>The booking configuration for the specified clinic.</returns>
        [HttpGet]
        [RoleAuthorize]
        [Route(ApiRoutes.BookingConfig.GetByClinic)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByClinic([FromRoute] Guid clinicId)
        {
            var query = new GetBookingConfigurationQuery { ClinicId = clinicId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Creates a new booking configuration for a specific clinic.
        /// </summary>
        /// <param name="clinicId">The ID of the clinic.</param>
        /// <param name="dto">The booking configuration data transfer object.</param>
        /// <returns>The created booking configuration.</returns>
        [HttpPost]
        [RoleAuthorize]
        [Route(ApiRoutes.BookingConfig.Create)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(Guid clinicId, [FromBody] CreateBookingConfigDto dto)
        {
            var result = await _mediator.Send(new CreateBookingConfigCommand(clinicId, dto));
            return Created(nameof(GetByClinic), result);
        }

        /// <summary>
        /// Updates the booking configuration for a specific clinic.
        /// </summary>
        /// <param name="clinicId">The ID of the clinic.</param>
        /// <param name="dto">The booking configuration data transfer object.</param>
        /// <returns>The updated booking configuration.</returns>
        [HttpPut]
        [RoleAuthorize]
        [Route(ApiRoutes.BookingConfig.Update)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid clinicId, [FromBody] UpdateBookingConfigDto dto)
        {
            var result = await _mediator.Send(new UpdateBookingConfigCommand(clinicId, dto));
            return Ok(result);
        }
    }
}
