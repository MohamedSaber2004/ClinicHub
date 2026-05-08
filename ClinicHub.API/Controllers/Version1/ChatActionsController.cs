using Asp.Versioning;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.RealTime.Commands.Typing;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    [Authorize]
    public class ChatActionsController : BaseApiController
    {
        public ChatActionsController(IMediator mediator) : base(mediator)
        {
        }

        /// <summary>
        /// Triggers a typing indicator event to the other participant.
        /// </summary>
        [HttpPost]
        [Route(ApiRoutes.ChatActions.Typing)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Typing([FromBody] TypingCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
