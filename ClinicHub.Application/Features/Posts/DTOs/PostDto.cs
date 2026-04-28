namespace ClinicHub.Application.Features.Posts.DTOs
{
    public record PostDto(Guid Id, string Content, Guid AuthorId, DateTime CreatedAt,
                   int ReactionCount, int CommentCount, IReadOnlyList<MediaDto> Media);
}
