using ClinicHub.Domain.Common;
using ClinicHub.Domain.Enums;

namespace ClinicHub.Domain.Entities
{
    public class Reaction : BaseEntity<Guid>
    {
        public ReactionType Type { get; private set; }
        public Guid AuthorId { get; private set; }

        public Guid? PostId { get; private set; }
        public Post? Post { get; private set; }
    
        public Guid? CommentId { get; private set; }
        public Comment? Comment { get; private set; }
    
        private Reaction() { }

        [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
        public Reaction(ReactionType type, Guid authorId, Guid? postId = null, Guid? commentId = null)
        {
            Type = type;
            AuthorId = authorId;
            PostId = postId;
            CommentId = commentId;
        }
    }
}
