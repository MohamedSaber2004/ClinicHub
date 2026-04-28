using Asp.Versioning;
using ClinicHub.Application.Features.Comments.Commands.AddComment;
using ClinicHub.Application.Features.Comments.Commands.DeleteComment;
using ClinicHub.Application.Features.Comments.Commands.ToggleCommentReaction;
using ClinicHub.Application.Features.Comments.Commands.UpdateComment;
using ClinicHub.Application.Features.Comments.Queries.GetCommentReplies;
using ClinicHub.Application.Features.Comments.Queries.GetCommentsByPost;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Comments.Queries.GetCommentReactions;
using ClinicHub.Application.Features.Comments.Queries.GetAllCommentsPagginated;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    [Authorize]
    public class CommentsController : BaseApiController
    {
        public CommentsController(IMediator mediator) : base(mediator)
        {
        }

        /// <summary>
        /// Get All Comments Pagginated.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.Comments.GetAllPagginated)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllComments([FromQuery] GetAllCommentsPagginatedQuery query, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get Comments By PostId.
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.Comments.GetCommentsByPost)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCommentsByPost(Guid PostId,[FromQuery] GetCommentsByPostQuery query, CancellationToken ct)
        { 
            var result = await _mediator.Send(query with { PostId = PostId}, ct);
            return Ok(result);
        }

        /// <summary>
        /// Get Comment Replies.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.Comments.GetReplies)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCommentReplies(Guid id, [FromQuery] GetCommentRepliesQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query with { CommentId = id }, ct);
            return Ok(result);
        }

        /// <summary>
        /// Add New Comment
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="command"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Comments.Create)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddComment([FromBody] AddCommentCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Created(string.Empty, result);
        }

        /// <summary>
        /// Update Comment.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPut]
        [Route(ApiRoutes.Comments.Update)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateComment([FromBody] UpdateCommentCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        /// <summary>
        /// Delete Comment.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route(ApiRoutes.Comments.Delete)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteComment([FromBody] DeleteCommentCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Deleted(result);
        }

        /// <summary>
        /// Toggle new reaction or update.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Comments.ToggleReaction)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ToggleReaction(Guid id, [FromQuery] ToggleCommentReactionCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command with { CommentId = id }, ct);
            return Ok(result);
        }

        /// <summary>
        /// Get Comment Reactions.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.Comments.GetCommentReactions)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCommentReactions(Guid id, [FromQuery] GetCommentReactionsQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query with { CommentId = id }, ct);
            return Ok(result);
        }
    }
}
