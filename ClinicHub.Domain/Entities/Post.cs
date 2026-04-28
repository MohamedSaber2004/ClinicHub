using ClinicHub.Domain.Common;
using ClinicHub.Domain.Enums;

namespace ClinicHub.Domain.Entities
{
    public class Post : BaseEntity<Guid>
    {
        public string Content { get; private set; }
        public Guid AuthorId { get; private set; }

        private readonly List<Comment> _comments = [];
        private readonly List<Reaction> _reactions = [];
        private readonly List<Media> _media = [];

        public IReadOnlyCollection<Comment> Comments => [.. _comments];
        public IReadOnlyCollection<Reaction> Reactions => [.. _reactions];
        public IReadOnlyCollection<Media> Media => [.. _media];

        private Post() 
        {
            Content = string.Empty;
        }

        public Post(string content, Guid authorId)
        {
            Content = content;
            AuthorId = authorId;
        }

        public void UpdateContent(string content)
        {
            Content = content;
            MarkAsUpdated(AuthorId.ToString());
        }

        public void AddReaction(ReactionType type, Guid userId)
        {
            var existing = _reactions.FirstOrDefault(r => r.AuthorId == userId);
            
            if (existing != null)
            {
                if (existing.Type == type)
                    _reactions.Remove(existing);
                else
                {
                    _reactions.Remove(existing);
                    _reactions.Add(new Reaction(type, userId, postId: this.Id));
                }
            }
            else
            {
                _reactions.Add(new Reaction(type, userId, postId: this.Id));
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

            _media.Add(new Media(url, type, postId: this.Id));
        }

        public void RemoveMedia(Guid mediaId)
        {
            var item = _media.FirstOrDefault(m => m.Id == mediaId);
            if (item != null) _media.Remove(item);
        }

        public Comment AddComment(string content, Guid userId)
        {
            var comment = new Comment(content, userId, this.Id);
            _comments.Add(comment);
            return comment;
        }
    }
}
