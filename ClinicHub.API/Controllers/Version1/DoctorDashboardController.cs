using Asp.Versioning;
using ClinicHub.API.Filters;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.DoctorDashboard.Availability.Commands.CreateMyAvailability;
using ClinicHub.Application.Features.DoctorDashboard.Availability.Commands.DeleteMyAvailability;
using ClinicHub.Application.Features.DoctorDashboard.Availability.Commands.ReplaceWeeklyAvailability;
using ClinicHub.Application.Features.DoctorDashboard.Availability.Commands.UpdateMyAvailability;
using ClinicHub.Application.Features.DoctorDashboard.Availability.Queries.GetMyAvailability;
using ClinicHub.Application.Features.DoctorDashboard.Commands.DoctorAcceptAppointment;
using ClinicHub.Application.Features.DoctorDashboard.Commands.DoctorCompleteAppointment;
using ClinicHub.Application.Features.DoctorDashboard.Commands.DoctorRejectAppointment;
using ClinicHub.Application.Features.DoctorDashboard.Commands.UpdateAppointmentStatus;
using ClinicHub.Application.Features.DoctorDashboard.Queries.GetDoctorAppointments;
using ClinicHub.Application.Features.DoctorDashboard.Queries.GetDoctorDashboardStats;
using ClinicHub.Application.Features.DoctorDashboard.Queries.GetDoctorPatients;
using ClinicHub.Application.Features.DoctorDashboard.Queries.GetPatientHistory;
using ClinicHub.Application.Features.DoctorDashboard.Queries.GetRecentAppointments;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    [RoleAuthorize(nameof(UserType.Doctor), nameof(UserType.ClinicOwner))]
    [RequirePlanPermission(SubscriptionPermission.ManageAppointments)]
    public class DoctorDashboardController : BaseApiController
    {
        public DoctorDashboardController(IMediator mediator) : base(mediator)
        {
        }

        [HttpGet]
        [Route(ApiRoutes.DoctorDashboard.Stats)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStats(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetDoctorDashboardStatsQuery(), ct);
            return Ok(result);
        }

        [HttpGet]
        [Route(ApiRoutes.DoctorDashboard.RecentAppointments)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRecentAppointments([FromQuery] int limit = 5, CancellationToken ct = default)
        {
            var result = await _mediator.Send(new GetRecentAppointmentsQuery { Limit = limit }, ct);
            return Ok(result);
        }

        [HttpGet]
        [Route(ApiRoutes.DoctorDashboard.Appointments)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAppointments([FromQuery] GetDoctorAppointmentsQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        [HttpPut]
        [Route(ApiRoutes.DoctorDashboard.AcceptAppointment)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AcceptAppointment(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new DoctorAcceptAppointmentCommand { AppointmentId = id }, ct);
            return Ok(result, LocalizationKeys.AppointmentMessages.AcceptedWithPaymentLink);
        }

        [HttpPut]
        [Route(ApiRoutes.DoctorDashboard.RejectAppointment)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RejectAppointment(Guid id, [FromBody] DoctorRejectAppointmentCommand command, CancellationToken ct)
        {
            command.AppointmentId = id;
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [HttpPut]
        [Route(ApiRoutes.DoctorDashboard.CompleteAppointment)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CompleteAppointment(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new DoctorCompleteAppointmentCommand { AppointmentId = id }, ct);
            return Ok(result);
        }

        /// <summary>
        /// Unified status update: 1=Accept, 2=Reject/Cancel, 3=Complete.
        /// </summary>
        [HttpPut]
        [Route(ApiRoutes.DoctorDashboard.UpdateAppointmentStatus)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAppointmentStatus(Guid id, [FromBody] UpdateAppointmentStatusCommand command, CancellationToken ct)
        {
            command.AppointmentId = id;
            var result = await _mediator.Send(command, ct);
            return result is null
                ? Ok(result)
                : Ok(result, LocalizationKeys.AppointmentMessages.AcceptedWithPaymentLink);
        }

        [HttpGet]
        [Route(ApiRoutes.DoctorDashboard.Patients)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPatients([FromQuery] GetDoctorPatientsQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        [HttpGet]
        [Route(ApiRoutes.DoctorDashboard.PatientHistory)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPatientHistory(Guid patientId, [FromQuery] GetPatientHistoryQuery query, CancellationToken ct)
        {
            query.PatientUserId = patientId;
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        /// <summary>
        /// Gets the logged-in doctor's full weekly schedule (raw availability rows).
        /// </summary>
        [HttpGet]
        [Route(ApiRoutes.DoctorDashboard.Availability)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyAvailability(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetMyAvailabilityQuery(), ct);
            return Ok(result);
        }

        /// <summary>
        /// Creates a new availability slot for the logged-in doctor.
        /// </summary>
        [HttpPost]
        [Route(ApiRoutes.DoctorDashboard.Availability)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateMyAvailability([FromBody] CreateMyAvailabilityCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        /// <summary>
        /// Updates one of the logged-in doctor's availability slots.
        /// </summary>
        [HttpPut]
        [Route(ApiRoutes.DoctorDashboard.UpdateAvailability)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateMyAvailability(Guid id, [FromBody] UpdateMyAvailabilityCommand command, CancellationToken ct)
        {
            command.Id = id;
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        /// <summary>
        /// Deletes one of the logged-in doctor's availability slots (soft delete).
        /// </summary>
        [HttpDelete]
        [Route(ApiRoutes.DoctorDashboard.DeleteAvailability)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMyAvailability(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeleteMyAvailabilityCommand { Id = id }, ct);
            return Ok(result);
        }

        /// <summary>
        /// Replaces the logged-in doctor's whole weekly schedule in one call
        /// (creates new rows, updates existing ones, deletes rows not sent).
        /// </summary>
        [HttpPut]
        [Route(ApiRoutes.DoctorDashboard.ReplaceWeeklyAvailability)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReplaceWeeklyAvailability([FromBody] ReplaceWeeklyAvailabilityCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
    }
}
