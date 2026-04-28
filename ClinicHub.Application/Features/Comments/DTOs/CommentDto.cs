using ClinicHub.Application.Features.Posts.DTOs;

namespace ClinicHub.Application.Features.Comments.DTOs
{
    public record CommentDto(Guid Id, string Content, Guid AuthorId, Guid PostId,
                      Guid? ParentCommentId, DateTime CreatedAt,
                      int ReactionCount, int ReplyCount, IReadOnlyList<MediaDto> Media);
}
