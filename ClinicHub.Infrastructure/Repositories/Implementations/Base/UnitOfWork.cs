using ClinicHub.Domain.Common.Interfaces;
using ClinicHub.Domain.Repositories.Interfaces.Base;
using ClinicHub.Infrastructure.Repositories.Implementations;
using ClinicHub.Domain.Repositories.Interfaces;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using ClinicHub.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace ClinicHub.Infrastructure.Repositories.Implementations.Base
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ClinicHubContext _context;
        private readonly Dictionary<Type, object> _repositories;
        private IPostRepository? _postRepository;
        private ICommentRepository? _commentRepository;
        private IClinicRepository? _clinicRepository;
        private ISpecializationRepository? _specializationRepository;
        private IUserFbTokenRepository? _userFbTokenRepository;
        private INotificationRepository? _notificationRepository;
        private IUserRefreshTokenRepository? _userRefreshTokenRepository;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(ClinicHubContext context)
        {
            _context = context;
            _repositories = new Dictionary<Type, object>();
        }

        public IGenericRepository<T, TKey> GetRepository<T, TKey>() where T : class, IBaseEntity<TKey> where TKey : IEquatable<TKey>
        {
            var type = typeof(T);
            if (!_repositories.ContainsKey(type))
            {
                var repositoryInstance = new GenericRepository<T, TKey>(_context);
                _repositories.Add(type, repositoryInstance);
            }
            return (IGenericRepository<T, TKey>)_repositories[type];
        }

        public IPostRepository PostRepository => _postRepository ??= new PostRepository(_context);
        public ICommentRepository CommentRepository => _commentRepository ??= new CommentRepository(_context);
        public IClinicRepository ClinicRepository => _clinicRepository ??= new ClinicRepository(_context);
        public ISpecializationRepository SpecializationRepository => _specializationRepository ??= new SpecializationRepository(_context);
        public IUserFbTokenRepository UserFbTokenRepository => _userFbTokenRepository ??= new UserFbTokenRepository(_context);
        public INotificationRepository NotificationRepository => _notificationRepository ??= new NotificationRepository(_context);
        public IUserRefreshTokenRepository UserRefreshTokenRepository => _userRefreshTokenRepository ??= new UserRefreshTokenRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    _transaction.Dispose();
                    _transaction = null!;
                }
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                _transaction.Dispose();
                _transaction = null!;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context?.Dispose();
        }
    }
}