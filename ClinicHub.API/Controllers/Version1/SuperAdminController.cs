using Asp.Versioning;
using ClinicHub.API.Filters;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Admin.Commands.ApproveClinic;
using ClinicHub.Application.Features.Admin.Commands.ApproveUserVerification;
using ClinicHub.Application.Features.Admin.Commands.RejectClinic;
using ClinicHub.Application.Features.Admin.Commands.RejectUserVerification;
using ClinicHub.Application.Features.Admin.Commands.UpdateSupportTicketStatus;
using ClinicHub.Application.Features.Admin.Queries.GetAdminDashboardStats;
using ClinicHub.Application.Features.Admin.Queries.GetAllSupportTickets;
using ClinicHub.Application.Features.Admin.Queries.GetClinicAuditLogs;
using ClinicHub.Application.Features.Admin.Queries.GetClinicsLookup;
using ClinicHub.Application.Features.Admin.Queries.GetPendingClinics;
using ClinicHub.Application.Features.Admin.Queries.GetPendingVerifications;
using ClinicHub.Application.Features.Admin.Queries.GetUrgentSupportTickets;
using ClinicHub.Application.Features.Advertisements.Commands.ApproveAdvertisement;
using ClinicHub.Application.Features.Advertisements.Commands.DeleteAdvertisement;
using ClinicHub.Application.Features.Advertisements.Commands.RejectAdvertisement;
using ClinicHub.Application.Features.Advertisements.Queries.GetAllAdvertisements;
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
        [Route(ApiRoutes.AdminDashboard.UrgentTickets)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUrgentTickets(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetUrgentSupportTicketsQuery(), ct);
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
        [Route(ApiRoutes.AdminDashboardExt.Tickets)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTickets([FromQuery] GetAllSupportTicketsQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        [HttpPut]
        [Route(ApiRoutes.AdminDashboardExt.UpdateTicketStatus)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateTicketStatus(Guid id, [FromBody] UpdateSupportTicketStatusCommand command, CancellationToken ct)
        {
            command.TicketId = id;
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

        [HttpGet]
        [Route(ApiRoutes.AdminDashboardExt.Advertisements)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAdvertisements([FromQuery] GetAllAdvertisementsQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        [HttpPost]
        [Route(ApiRoutes.AdminDashboardExt.ApproveAdvertisement)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ApproveAdvertisement(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new ApproveAdvertisementCommand { AdvertisementId = id }, ct);
            return Ok(result);
        }

        [HttpPost]
        [Route(ApiRoutes.AdminDashboardExt.RejectAdvertisement)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RejectAdvertisement(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new RejectAdvertisementCommand { AdvertisementId = id }, ct);
            return Ok(result);
        }

        [HttpDelete]
        [Route(ApiRoutes.AdminDashboardExt.DeleteAdvertisement)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAdvertisement(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeleteAdvertisementCommand { Id = id }, ct);
            return Ok(result);
        }
    }
}
