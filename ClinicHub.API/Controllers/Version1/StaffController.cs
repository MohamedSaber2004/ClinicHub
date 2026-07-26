using Asp.Versioning;
using ClinicHub.API.Filters;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.StaffDashboard.Commands.CheckInPatient;
using ClinicHub.Application.Features.StaffDashboard.Commands.RegisterWalkInPatient;
using ClinicHub.Application.Features.StaffDashboard.Commands.StaffApproveAppointment;
using ClinicHub.Application.Features.StaffDashboard.Commands.StaffCompleteAppointment;
using ClinicHub.Application.Features.StaffDashboard.Commands.StaffRejectAppointment;
using ClinicHub.Application.Features.StaffDashboard.Queries.GetStaffAppointments;
using ClinicHub.Application.Features.StaffDashboard.Queries.GetStaffDashboardStats;
using ClinicHub.Application.Features.StaffDashboard.Queries.GetStaffDoctorSchedule;
using ClinicHub.Application.Features.StaffDashboard.Queries.GetStaffDoctors;
using ClinicHub.Application.Features.StaffDashboard.Queries.GetStaffQueue;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    [RoleAuthorize(nameof(UserType.Staff))]
    [RequirePlanPermission(SubscriptionPermission.ManageStaff)]
    public class StaffController : BaseApiController
    {
        public StaffController(IMediator mediator) : base(mediator)
        {
        }

        [HttpGet]
        [Route(ApiRoutes.StaffDashboard.Stats)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStats(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetStaffDashboardStatsQuery(), ct);
            return Ok(result);
        }

        [HttpGet]
        [Route(ApiRoutes.StaffDashboard.Appointments)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAppointments([FromQuery] GetStaffAppointmentsQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        [HttpPut]
        [Route(ApiRoutes.StaffDashboard.ApproveAppointment)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApproveAppointment(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new StaffApproveAppointmentCommand { AppointmentId = id }, ct);
            return Ok(result);
        }

        [HttpPut]
        [Route(ApiRoutes.StaffDashboard.RejectAppointment)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RejectAppointment(Guid id, [FromBody] StaffRejectAppointmentCommand command, CancellationToken ct)
        {
            command.AppointmentId = id;
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [HttpPut]
        [Route(ApiRoutes.StaffDashboard.CheckIn)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CheckIn(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new CheckInPatientCommand { AppointmentId = id }, ct);
            return Ok(result);
        }

        [HttpPut]
        [Route(ApiRoutes.StaffDashboard.CompleteAppointment)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CompleteAppointment(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new StaffCompleteAppointmentCommand { AppointmentId = id }, ct);
            return Ok(result);
        }

        [HttpPost]
        [Route(ApiRoutes.StaffDashboard.RegisterPatient)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterPatient([FromBody] RegisterWalkInPatientCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Created(ApiRoutes.StaffDashboard.RegisterPatient, result);
        }

        [HttpGet]
        [Route(ApiRoutes.StaffDashboard.Doctors)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDoctors(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetStaffDoctorsQuery(), ct);
            return Ok(result);
        }

        [HttpGet]
        [Route(ApiRoutes.StaffDashboard.DoctorSchedule)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDoctorSchedule([FromRoute] Guid doctorId, [FromQuery] GetStaffDoctorScheduleQuery query, CancellationToken ct)
        {
            query.DoctorId = doctorId;
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        [HttpGet]
        [Route(ApiRoutes.StaffDashboard.Queue)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetQueue(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetStaffQueueQuery(), ct);
            return Ok(result);
        }
    }
}
