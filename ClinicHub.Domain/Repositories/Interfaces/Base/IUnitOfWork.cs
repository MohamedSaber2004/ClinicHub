using ClinicHub.Domain.Common.Interfaces;
using ClinicHub.Domain.Repositories.Interfaces.Base;
using ClinicHub.Domain.Repositories.Interfaces;
using ClinicHub.Domain.Repositories;

namespace ClinicHub.Infrastructure.UnitOfWork.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T, TKey> GetRepository<T, TKey>() where T : class, IBaseEntity<TKey> where TKey : IEquatable<TKey>;
        IPostRepository PostRepository { get; }
        ICommentRepository CommentRepository { get; }
        IReactionRepository ReactionRepository { get; }
        IClinicRepository ClinicRepository { get; }
        ISpecializationRepository SpecializationRepository { get; }
        IUserFbTokenRepository UserFbTokenRepository { get; }
        INotificationRepository NotificationRepository { get; }
        IUserRefreshTokenRepository UserRefreshTokenRepository { get; }
        IConversationRepository ConversationRepository { get; }
        IMessageRepository MessageRepository { get; }
        IMessageReactionRepository MessageReactionRepository { get; }
        IMessageMediaRepository MessageMediaRepository { get; }
        IReadReceiptRepository ReadReceiptRepository { get; }
        IConversationParticipantRepository ConversationParticipantRepository { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
