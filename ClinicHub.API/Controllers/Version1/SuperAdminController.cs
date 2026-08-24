using Asp.Versioning;
using ClinicHub.API.Filters;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Admin.Commands.ApproveClinic;
using ClinicHub.Application.Features.Admin.Commands.ApproveUserVerification;
using ClinicHub.Application.Features.Admin.Commands.RejectClinic;
using ClinicHub.Application.Features.Admin.Commands.RejectUserVerification;
using ClinicHub.Application.Features.Admin.Queries.GetAdminDashboardStats;
using ClinicHub.Application.Features.Admin.Queries.GetAppointmentsSummary;
using ClinicHub.Application.Features.Admin.Queries.GetClinicAuditLogs;
using ClinicHub.Application.Features.Admin.Queries.GetClinicsLookup;
using ClinicHub.Application.Features.Admin.Queries.GetClinicsGrowth;
using ClinicHub.Application.Features.Admin.Queries.GetPendingClinics;
using ClinicHub.Application.Features.Admin.Queries.GetPendingVerifications;
using ClinicHub.Application.Features.Admin.Queries.GetRevenueTrend;
using ClinicHub.Application.Features.Admin.Queries.GetSubscriptionsByPlan;
using ClinicHub.Application.Features.Admin.Queries.GetUsersGrowth;
using ClinicHub.Application.Features.PlatformSettings.Commands.UpdatePlatformSetting;
using ClinicHub.Application.Features.PlatformSettings.Queries.GetPlatformSetting;
using ClinicHub.Application.Features.Subscriptions.Commands.AdminCreateSubscription;
using ClinicHub.Application.Features.Subscriptions.Commands.RevokeSubscription;
using ClinicHub.Application.Features.Subscriptions.Queries.GetAllSubscriptions;
using ClinicHub.Application.Features.Users.Queries.GetAllUsers;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    [RoleAuthorize(nameof(UserType.SuperAdmin))]
    public class SuperAdminController : BaseApiController
    {
        private readonly IStringLocalizer<Messages> _localizer;

        public SuperAdminController(IMediator mediator, IStringLocalizer<Messages> localizer) : base(mediator)
        {
            _localizer = localizer;
        }

        [HttpGet]
        [Route(ApiRoutes.AdminClinics.PendingClinics)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPendingClinics(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetPendingClinicsQuery(), ct);
            return Ok(result);
        }

        [HttpPost]
        [Route(ApiRoutes.AdminClinics.ApproveClinic)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ApproveClinic(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new ApproveClinicCommand(id), ct);
            return Ok(result);
        }

        [HttpPost]
        [Route(ApiRoutes.AdminClinics.RejectClinic)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RejectClinic(Guid id, [FromBody] RejectClinicCommand command, CancellationToken ct)
        {
            command = command with { ClinicId = id };
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [HttpGet]
        [Route(ApiRoutes.AdminDashboard.Stats)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStats(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAdminDashboardStatsQuery(), ct);
            return Ok(result);
        }

        [HttpGet]
        [Route(ApiRoutes.AdminDashboard.RevenueTrend)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRevenueTrend([FromQuery] GetRevenueTrendQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        [HttpGet]
        [Route(ApiRoutes.AdminDashboard.ClinicsGrowth)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClinicsGrowth([FromQuery] GetClinicsGrowthQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        [HttpGet]
        [Route(ApiRoutes.AdminDashboard.SubscriptionsByPlan)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSubscriptionsByPlan([FromQuery] GetSubscriptionsByPlanQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        [HttpGet]
        [Route(ApiRoutes.AdminDashboard.UsersGrowth)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsersGrowth([FromQuery] GetUsersGrowthQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        [HttpGet]
        [Route(ApiRoutes.AdminDashboard.AppointmentsSummary)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAppointmentsSummary([FromQuery] GetAppointmentsSummaryQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        [HttpGet]
        [Route(ApiRoutes.AdminDashboard.ClinicLogs)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClinicLogs([FromRoute] Guid clinicId, [FromQuery] GetClinicAuditLogsQuery query, CancellationToken ct)
        {
            query.ClinicId = clinicId;
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
        [Route(ApiRoutes.AdminDashboard.ClinicsLookup)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetClinicsLookup(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetClinicsLookupQuery(), ct);
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

        [HttpGet]
        [Route(ApiRoutes.AdminDashboardExt.AllSubscriptions)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSubscriptions([FromQuery] GetAllSubscriptionsQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        [HttpPost]
        [Route(ApiRoutes.AdminDashboardExt.CreateSubscription)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateSubscription([FromBody] AdminCreateSubscriptionCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Created(ApiRoutes.AdminDashboardExt.CreateSubscription, result, _localizer[LocalizationKeys.SubscriptionMessages.Created.Value]);
        }

        [HttpPost]
        [Route(ApiRoutes.AdminDashboardExt.RevokeSubscription)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RevokeSubscription(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new RevokeSubscriptionCommand { SubscriptionId = id }, ct);
            return Ok(result);
        }

        /// <summary>
        /// Returns the platform appointment fee percentage charged on top of every booking.
        /// </summary>
        [HttpGet]
        [Route(ApiRoutes.PlatformSettings.Get)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPlatformSetting(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetPlatformSettingQuery(), ct);
            return Ok(result);
        }

        /// <summary>
        /// Updates the platform appointment fee percentage (0-100).
        /// </summary>
        [HttpPut]
        [Route(ApiRoutes.PlatformSettings.Update)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePlatformSetting([FromBody] UpdatePlatformSettingCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
    }
}
