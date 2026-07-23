using Asp.Versioning;
using ClinicHub.API.Filters;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.DoctorDashboard.Commands.DoctorAcceptAppointment;
using ClinicHub.Application.Features.DoctorDashboard.Commands.DoctorCompleteAppointment;
using ClinicHub.Application.Features.DoctorDashboard.Commands.DoctorRejectAppointment;
using ClinicHub.Application.Features.DoctorDashboard.Queries.GetDoctorAppointments;
using ClinicHub.Application.Features.DoctorDashboard.Queries.GetDoctorDashboardStats;
using ClinicHub.Application.Features.DoctorDashboard.Queries.GetDoctorPatients;
using ClinicHub.Application.Features.DoctorDashboard.Queries.GetPatientHistory;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    [RoleAuthorize(nameof(UserType.Doctor))]
    [RequireSubscription]
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
            return Ok(result);
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
    }
}
