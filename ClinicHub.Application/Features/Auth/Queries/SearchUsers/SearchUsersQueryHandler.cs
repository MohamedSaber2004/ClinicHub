using MediatR;
using ClinicHub.Application.Features.Auth.DTOs;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Auth.Queries.SearchUsers
{
    public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, List<UserSearchDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public SearchUsersQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<UserSearchDto>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
        {
            var userRepo = _unitOfWork.GetRepository<ApplicationUser, Guid>();
            
            var query = userRepo.GetAllAsync(null);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(u => 
                    u.FullName.ToLower().Contains(term) || 
                    u.Email.ToLower().Contains(term));
            }

            var users = await query
                .Take(20)
                .Select(u => new UserSearchDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    ProfilePictureUrl = u.ProfilePictureUrl ?? string.Empty
                })
                .ToListAsync(cancellationToken);

            return users;
        }
    }
}
