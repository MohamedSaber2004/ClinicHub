using Asp.Versioning;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.RealTime.Commands.AuthenticatePusher;
using ClinicHub.Application.Features.RealTime.Commands.ConnectUser;
using ClinicHub.Application.Features.RealTime.Commands.DisconnectUser;
using ClinicHub.Application.Features.RealTime.Commands.HandlePusherWebhook;
using ClinicHub.Application.Features.RealTime.Commands.SetActiveConversation;
using ClinicHub.Application.Features.RealTime.Commands.SetTyping;
using ClinicHub.Application.Features.RealTime.Queries.GetOnlineUsers;
using ClinicHub.Application.Features.RealTime.Queries.GetTypingUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    [Authorize]
    public class RealTimeController : BaseApiController
    {
        public RealTimeController(IMediator mediator) : base(mediator)
        {
        }

        /// <summary>
        /// Authenticates Pusher private and presence channels.
        /// </summary>
        [HttpPost]
        [Route(ApiRoutes.RealTime.Auth)]
        [Consumes("application/x-www-form-urlencoded")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Authenticate([FromForm] string socket_id, [FromForm] string channel_name)
        {
            var authJson = await _mediator.Send(new AuthenticatePusherCommand { SocketId = socket_id, ChannelName = channel_name });
            return Content(authJson, "application/json");
        }

        /// <summary>
        /// Handles Webhooks from Pusher (e.g. channel_vacated, member_removed)
        /// </summary>
        [HttpPost]
        [Route(ApiRoutes.RealTime.Webhook)]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Webhook([FromBody] PusherWebhookPayload payload)
        {
            var result = await _mediator.Send(new HandlePusherWebhookCommand { Events = payload.Events });
            return Ok(result);
        }

        /// <summary>
        /// Update typing status for the current user.
        /// </summary>
        [HttpPost]
        [Route(ApiRoutes.RealTime.Typing)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetTyping([FromBody] SetTypingCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Set the user's active conversation. Set to null to leave the conversation.
        /// </summary>
        [HttpPost]
        [Route(ApiRoutes.RealTime.SetActiveConversation)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> SetActiveConversation([FromBody] SetActiveConversationCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Get current typing users in a conversation.
        /// </summary>
        [HttpGet]
        [Route(ApiRoutes.RealTime.GetTypingUsers)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTypingUsers([FromRoute] Guid conversationId)
        {
            var result = await _mediator.Send(new GetTypingUsersQuery { ConversationId = conversationId });
            return Ok(result);
        }

        /// <summary>
        /// Get all currently online users.
        /// </summary>
        [HttpGet]
        [Route(ApiRoutes.RealTime.OnlineUsers)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOnlineUsers()
        {
            var result = await _mediator.Send(new GetOnlineUsersQuery());
            return Ok(result);
        }

        /// <summary>
        /// Explicitly connect user (in-memory tracking) using their Pusher SocketId.
        /// </summary>
        [HttpPost]
        [Route(ApiRoutes.RealTime.Connect)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Connect([FromBody] ConnectUserCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Explicitly disconnect user (in-memory tracking) using their Pusher SocketId.
        /// </summary>
        [HttpPost]
        [Route(ApiRoutes.RealTime.Disconnect)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Disconnect([FromBody] DisconnectUserCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }

    public class PusherWebhookPayload
    {
        public long TimeMs { get; set; }
        public List<PusherWebhookEvent> Events { get; set; } = new();
    }
}
