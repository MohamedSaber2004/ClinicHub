using ClinicHub.Domain.Common;
using ClinicHub.Domain.Enums;

namespace ClinicHub.Domain.Entities
{
    public class Comment : BaseEntity<Guid>
    {
        public string Content { get; private set; } = null!;
        public Guid AuthorId { get; private set; }
        public Guid PostId { get; private set; }

        public Post Post { get; private set; } = null!;
        public Guid? ParentCommentId { get; private set; }
        public Comment? ParentComment { get; private set; }

        private readonly List<Comment> _replies = [];
        private readonly List<Reaction> _reactions = [];
        private readonly List<Media> _media = [];

        public IReadOnlyCollection<Comment> Replies => [.. _replies];
        public IReadOnlyCollection<Reaction> Reactions => [.. _reactions];
        public IReadOnlyCollection<Media> Media => [.. _media];

        private Comment() { }

        [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
        public Comment(string content, Guid authorId, Guid postId, Guid? parentCommentId = null)
        {
            Content = content;
            AuthorId = authorId;
            PostId = postId;
            ParentCommentId = parentCommentId;
        }

        public void UpdateContent(string content)
        {
            Content = content;
            MarkAsUpdated(AuthorId.ToString());
        }

        public Comment AddReply(string content, Guid userId)
        {
            var reply = new Comment(content, userId, this.PostId, parentCommentId: this.Id);
            _replies.Add(reply);
            return reply;
        }

        public void ToggleReaction(ReactionType type, Guid userId)
        {
            var existing = _reactions.FirstOrDefault(r => r.AuthorId == userId);

            if (existing != null)
            {
                if (existing.Type == type)
                    _reactions.Remove(existing);
                else
                {
                    _reactions.Remove(existing);
                    _reactions.Add(new Reaction(type, userId, commentId: this.Id));
                }
            }
            else
            {
                _reactions.Add(new Reaction(type, userId, commentId: this.Id));
            }
        }

        public void RemoveReaction(Guid userId)
        {
            var reaction = _reactions.FirstOrDefault(r => r.AuthorId == userId);
            if (reaction != null)
            {
                _reactions.Remove(reaction);
            }
        }

        public void AddMedia(string url, MediaType type)
        {
            if (_media.Any(m => m.Url == url)) return;

            _media.Add(new Media(url, type, commentId: this.Id));
        }

        public void RemoveMedia(Guid mediaId)
        {
            var item = _media.FirstOrDefault(m => m.Id == mediaId);
            if (item != null) _media.Remove(item);
        }
    }
}
