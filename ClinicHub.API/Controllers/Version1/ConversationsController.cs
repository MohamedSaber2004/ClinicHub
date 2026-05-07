using Asp.Versioning;
using ClinicHub.Application.Features.Conversations.Commands.CreateConversation;
using ClinicHub.Application.Features.Conversations.Commands.DeleteConversation;
using ClinicHub.Application.Features.Conversations.Commands.DeleteMessage;
using ClinicHub.Application.Features.Conversations.Commands.SendMessage;
using ClinicHub.Application.Features.Conversations.Queries.GetConversationById;
using ClinicHub.Application.Features.Conversations.Queries.GetConversationMessages;
using ClinicHub.Application.Features.Conversations.Queries.GetConversations;
using ClinicHub.API.Routes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    [Authorize]
    public class ConversationsController : BaseApiController
    {
        public ConversationsController(IMediator mediator) : base(mediator)
        {
        }

        /// <summary>
        /// Get All Conversations Paginated.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.Conversations.GetAll)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetConversations([FromQuery] GetConversationsQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        /// <summary>
        /// Get Conversation By Id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.Conversations.GetById)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetConversationById(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetConversationByIdQuery(id), ct);
            return Ok(result);
        }

        /// <summary>
        /// Create New Conversation.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Conversations.Create)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateConversation([FromBody] CreateConversationCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        /// <summary>
        /// Send Message In Conversation.
        /// </summary>
        /// <param name="conversationId"></param>
        /// <param name="command"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Conversations.SendMessage)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SendMessage(Guid conversationId, [FromBody] SendMessageCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command with { ConversationId = conversationId }, ct);
            return Ok(result);
        }

        /// <summary>
        /// Get Messages In Conversation Paginated.
        /// </summary>
        /// <param name="conversationId"></param>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.Conversations.GetMessages)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMessages(Guid conversationId, [FromQuery] GetConversationMessagesQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query with { ConversationId = conversationId }, ct);
            return Ok(result);
        }

        /// <summary>
        /// Delete Message.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route(ApiRoutes.Conversations.DeleteMessage)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteMessage(Guid messageId, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeleteMessageCommand(messageId), ct);
            return Ok(result);
        }

        /// <summary>
        /// Delete Conversation.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route(ApiRoutes.Conversations.Delete)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteConversation(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeleteConversationCommand(id), ct);
            return Ok(result);
        }
    }
}
