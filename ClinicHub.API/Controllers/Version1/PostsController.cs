using Asp.Versioning;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Posts.Commands.CreatePost;
using ClinicHub.Application.Features.Posts.Commands.DeletePost;
using ClinicHub.Application.Features.Posts.Commands.TogglePostReaction;
using ClinicHub.Application.Features.Posts.Commands.UpdatePost;
using ClinicHub.Application.Features.Posts.Queries.GetPostById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Posts.Queries.GetPostsPagginated;
using ClinicHub.Application.Features.Posts.Queries.GetPostReactions;
using ClinicHub.API.Filters;
using ClinicHub.Domain.Enums;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    [RoleAuthorize]
    public class PostsController : BaseApiController
    {
        private readonly IDeepLinkService _deepLinkService;
        private readonly ICurrentUserService _currentUserService;

        public PostsController(IMediator mediator, IDeepLinkService deepLinkService, ICurrentUserService currentUserService) : base(mediator)
        {
            _deepLinkService = deepLinkService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Get All Posts Pagginated.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.Posts.GetAllPagginated)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPosts([FromQuery] GetPostsQueryPagginated query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        /// <summary>
        /// Get Post By Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.Posts.GetById)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPostById(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetPostByIdQuery(id), ct);
            return Ok(result);
        }

        /// <summary>
        /// Create New Post.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Posts.Create)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        /// <summary>
        /// Update Post
        /// </summary>
        /// <param name="command"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPut]
        [Route(ApiRoutes.Posts.Update)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePost([FromBody] UpdatePostCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        /// <summary>
        /// Delete Post.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route(ApiRoutes.Posts.Delete)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeletePost([FromBody] DeletePostCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Deleted(result);
        }

        /// <summary>
        /// Toggle Reaction To Post.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Posts.ToggleReaction)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ToggleReaction(Guid id, [FromBody] TogglePostReactionCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command with { PostId = id }, ct);
            return Ok(result);
        }

        /// <summary>
        /// Get Post Reactions Pagginated.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ApiRoutes.Posts.GetPostReactions)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPostReactions(Guid id, [FromQuery] GetPostReactionsQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query with { PostId = id }, ct);
            return Ok(result);
        }

        /// <summary>
        /// Get shareable deep link for a post.
        /// </summary>
        [HttpGet]
        [Route(ApiRoutes.Posts.ShareLink)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetShareLink(Guid id, CancellationToken ct)
        {
            var post = await _mediator.Send(new GetPostByIdQuery(id), ct);
            if (post.AuthorId != _currentUserService.UserId)
                return NotFound();

            var deepLink = _deepLinkService.GeneratePostLink(post.Id);
            return Ok(new { link = deepLink });
        }
    }
}
