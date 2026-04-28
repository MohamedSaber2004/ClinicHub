namespace ClinicHub.Application.Features.Posts.DTOs
{
    public record ReactionDto(Guid Id, Guid AuthorId, string Type, DateTime CreatedAt);
}
