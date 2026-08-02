using Asp.Versioning;
using ClinicHub.API.Filters;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Clinics.Commands.AcceptBooking;
using ClinicHub.Application.Features.Clinics.Commands.RegisterClinic;
using ClinicHub.Application.Features.Clinics.Commands.ActivateClinic;
using ClinicHub.Application.Features.Clinics.Commands.CreateClinic;
using ClinicHub.Application.Features.Clinics.Commands.DeactivateClinic;
using ClinicHub.Application.Features.Clinics.Commands.RejectBooking;
using ClinicHub.Application.Features.Clinics.Commands.SetupClinic;
using ClinicHub.Application.Features.Clinics.Commands.UpdateClinic;
using ClinicHub.Application.Features.Clinics.Commands.UpdateClinicSettings;
using ClinicHub.Application.Features.Clinics.Queries.GetClinicBookings;
using ClinicHub.Application.Features.Clinics.Queries.GetClinicDashboardStats;
using ClinicHub.Application.Features.Clinics.DTOs;
using ClinicHub.Application.Features.Clinics.Queries.GetClinicById;
using ClinicHub.Application.Features.Clinics.Queries.GetClinicDetails;
using ClinicHub.Application.Features.Clinics.Queries.GetClinicSettings;
using ClinicHub.Application.Features.Clinics.Queries.GetPaginatedClinics;
using ClinicHub.Application.Features.Auth.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    public class ClinicManagementController: BaseApiController
    {
        public ClinicManagementController(IMediator mediator): base(mediator)
        {
        }

        /// <summary>
        /// Registers a new clinic with owner details for approval.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [Route(ApiRoutes.ClinicRegister.Register)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterClinicCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            if (result.IsPendingApproval)
                return Accepted(result.PendingData);
            return Created(null!, result.AuthData);
        }

        /// <summary>
        /// Creates a new clinic based on the provided data transfer object (DTO).
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [RoleAuthorize(nameof(UserType.SuperAdmin))]
        [Route(ApiRoutes.ClinicManagement.Create)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody]CreateClinicCommand request)
        {
            var result = await _mediator.Send(request);
            return Created(nameof(CreateClinicCommand), result);
        }

        /// <summary>
        /// Sets up a clinic for an already-approved ClinicOwner user.
        /// Creates the clinic and a linked Doctor record.
        /// </summary>
        [HttpPost]
        [Route(ApiRoutes.ClinicManagement.Setup)]
        [RoleAuthorize(nameof(UserType.ClinicOwner))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Setup([FromBody] SetupClinicCommand command)
        {
            var result = await _mediator.Send(command);
            return Created(nameof(SetupClinicCommand), result);
        }

        /// <summary>
        /// Updates an existing clinic identified by the provided ID 
        /// using the data from the provided data transfer object (DTO).
        /// </summary>
        /// <param name="id">The ID of the clinic to update.</param>
        /// <param name="dto">The data transfer object containing the updated clinic information.</param>
        /// <returns>The updated clinic information.</returns>
        [HttpPut]
        [Route(ApiRoutes.ClinicManagement.Update)]
        [RoleAuthorize(nameof(UserType.SuperAdmin))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClinicCommand request)
        {
            var result = await _mediator.Send(request with { Id = id });
            return Ok(result);
        }

        /// <summary>
        /// Activates a clinic identified by the provided ID.
        /// </summary>
        /// <param name="id">The ID of the clinic to activate.</param>
        /// <returns>The activated clinic information.</returns>
        [HttpPatch]
        [Route(ApiRoutes.ClinicManagement.Activate)]
        [RoleAuthorize(nameof(UserType.SuperAdmin))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Activate(Guid id)
        {
            var result = await _mediator.Send(new ActivateClinicCommand(id));
            return Ok(result);
        }

        /// <summary>
        /// Deactivates a clinic identified by the provided ID.
        /// </summary>
        /// <param name="id">The ID of the clinic to deactivate.</param>
        /// <returns>The deactivated clinic information.</returns>
        [HttpPatch]
        [RoleAuthorize(nameof(UserType.SuperAdmin))]
        [Route(ApiRoutes.ClinicManagement.Deactivate)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            var result = await _mediator.Send(new DeactivateClinicCommand(id));
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a clinic by its unique identifier (ID).
        /// </summary>
        /// <param name="id">The ID of the clinic to retrieve.</param>
        /// <returns>The clinic information.</returns>
        [HttpGet]
        [Route(ApiRoutes.ClinicManagement.GetById)]
        [RoleAuthorize(nameof(UserType.SuperAdmin))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetClinicByIdQuery(id));
            return Ok(result);
        }


        /// <summary>
        /// Retrieves clinic details including doctors, staff, and ratings.
        /// </summary>
        [HttpGet]
        [Route(ApiRoutes.ClinicManagement.GetDetails)]
        [RoleAuthorize(nameof(UserType.SuperAdmin))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDetails(Guid id)
        {
            var result = await _mediator.Send(new GetClinicDetailsQuery(id));
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a paginated list of clinics based on the provided query parameters.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.ClinicManagement.GetPaginated)]
        [RoleAuthorize(nameof(UserType.SuperAdmin))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPaginated([FromQuery] GetPaginatedClinicsQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves the clinic settings of the currently authenticated clinic owner.
        /// </summary>
        [HttpGet]
        [Route(ApiRoutes.ClinicManagement.Settings)]
        [RoleAuthorize(nameof(UserType.ClinicOwner))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSettings()
        {
            var result = await _mediator.Send(new GetClinicSettingsQuery());
            return Ok(result);
        }

        /// <summary>
        /// Updates the clinic settings of the currently authenticated clinic owner.
        /// </summary>
        [HttpPut]
        [Route(ApiRoutes.ClinicManagement.Settings)]
        [RoleAuthorize(nameof(UserType.ClinicOwner))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSettings([FromBody] UpdateClinicSettingsCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Returns clinic dashboard statistics with period breakdowns.
        /// </summary>
        [HttpGet]
        [Route(ApiRoutes.ClinicManagement.Dashboard)]
        [RoleAuthorize(nameof(UserType.ClinicOwner))]
        [RequirePlanPermission(SubscriptionPermission.BasicReports)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Dashboard()
        {
            var result = await _mediator.Send(new GetClinicDashboardStatsQuery());
            return Ok(result);
        }

        /// <summary>
        /// Retrieves paginated clinic bookings filtered by status.
        /// </summary>
        [HttpGet]
        [Route(ApiRoutes.ClinicManagement.GetBookings)]
        [RoleAuthorize(nameof(UserType.ClinicOwner))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetBookings([FromQuery] GetClinicBookingsQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Accepts a pending booking.
        /// </summary>
        [HttpPost]
        [Route(ApiRoutes.ClinicManagement.AcceptBooking)]
        [RoleAuthorize(nameof(UserType.ClinicOwner))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AcceptBooking([FromBody] AcceptBookingCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result, LocalizationKeys.AppointmentMessages.AcceptedWithPaymentLink);
        }

        /// <summary>
        /// Rejects a pending booking with an optional reason.
        /// </summary>
        [HttpPost]
        [Route(ApiRoutes.ClinicManagement.RejectBooking)]
        [RoleAuthorize(nameof(UserType.ClinicOwner))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RejectBooking([FromBody] RejectBookingCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
