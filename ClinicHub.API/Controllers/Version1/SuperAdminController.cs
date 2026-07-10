using Asp.Versioning;
using ClinicHub.API.Filters;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Admin.Commands.ApproveUserVerification;
using ClinicHub.Application.Features.Admin.Commands.RejectUserVerification;
using ClinicHub.Application.Features.Admin.Queries.GetAdminDashboardStats;
using ClinicHub.Application.Features.Admin.Queries.GetClinicAuditLogs;
using ClinicHub.Application.Features.Admin.Queries.GetPendingVerifications;
using ClinicHub.Application.Features.Admin.Queries.GetUrgentSupportTickets;
using ClinicHub.Application.Features.Users.Queries.GetAllUsers;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    [RoleAuthorize(nameof(UserType.SuperAdmin))]
    public class SuperAdminController : BaseApiController
    {
        public SuperAdminController(IMediator mediator) : base(mediator)
        {
        }

        [HttpGet]
        [Route(ApiRoutes.AdminDashboard.Stats)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetStats(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAdminDashboardStatsQuery(), ct);
            return Ok(result);
        }

        [HttpGet]
        [Route(ApiRoutes.AdminDashboard.UrgentTickets)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUrgentTickets([FromQuery] GetUrgentSupportTicketsQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        [HttpGet]
        [Route(ApiRoutes.AdminDashboard.Users)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUsers([FromQuery] GetAllUsersQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        [HttpGet]
        [Route(ApiRoutes.AdminDashboard.ClinicLogs)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetClinicLogs(Guid clinicId, [FromQuery] GetClinicAuditLogsQuery query, CancellationToken ct)
        {
            query.ClinicId = clinicId;
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        [HttpGet]
        [Route(ApiRoutes.UserVerifications.GetPending)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPendingVerifications([FromQuery] GetPendingVerificationsQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        [HttpPost]
        [Route(ApiRoutes.UserVerifications.Approve)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ApproveUser(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new ApproveUserVerificationCommand(id), ct);
            return Ok(result);
        }

        [HttpPost]
        [Route(ApiRoutes.UserVerifications.Reject)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RejectUser(Guid id, [FromBody] RejectUserVerificationCommand command, CancellationToken ct)
        {
            command = command with { UserId = id };
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
    }
}
