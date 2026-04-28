using ClinicHub.Domain.Common;
using ClinicHub.Domain.Enums;

namespace ClinicHub.Domain.Entities
{
    public class Media : BaseEntity<Guid>
    {
        public string Url { get; private set; }
        public MediaType Type { get; private set; }
    
        public Guid? PostId { get; private set; }
        public Post? Post { get; private set; }
    
        public Guid? CommentId { get; private set; }
        public Comment? Comment { get; private set; }
    
        private Media() 
        {
            Url = string.Empty;
        }

        public Media(string url, MediaType type, Guid? postId = null, Guid? commentId = null)
        {
            Url = url;
            Type = type;
            PostId = postId;
            CommentId = commentId;
        }
    }
}
