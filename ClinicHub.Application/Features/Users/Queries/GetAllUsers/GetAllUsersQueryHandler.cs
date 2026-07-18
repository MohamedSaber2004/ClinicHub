using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Users.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Users.Queries.GetAllUsers
{
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PagginatedResult<UserDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IClinicHubContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetAllUsersQueryHandler(
            UserManager<ApplicationUser> userManager,
            IClinicHubContext context,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _context = context;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<PagginatedResult<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var query = _userManager.Users.Where(u => !u.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(u => 
                    u.FullName.ToLower().Contains(term) || 
                    (u.Email != null && u.Email.ToLower().Contains(term)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(term)));
            }

            if(request.UserId.HasValue)
            {
                query = query.Where(u => u.Id == request.UserId.Value);
            }

            if (request.UserType.HasValue)
            {
                var flags = Enum.GetValues<UserType>()
                    .Where(ut => ut != UserType.None && request.UserType.Value.HasFlag(ut))
                    .ToList();

                if (flags.Count != 0)
                {
                    var userIds = new HashSet<Guid>();
                    foreach (var flag in flags)
                    {
                        if (flag == UserType.Doctor && _currentUserService.CurrentClinicId.HasValue)
                        {
                            var doctorUserIds = await _unitOfWork.DoctorRepository
                                .GetAllAsync(d => d.ClinicId == _currentUserService.CurrentClinicId.Value)
                                .Select(d => d.UserId)
                                .ToListAsync(cancellationToken);

                            foreach (var id in doctorUserIds)
                                userIds.Add(id);
                        }
                        else
                        {
                            var usersInRole = await _userManager.GetUsersInRoleAsync(flag.ToString());
                            foreach (var user in usersInRole)
                                userIds.Add(user.Id);
                        }
                    }
                    query = query.Where(u => userIds.Contains(u.Id));
                }
            }

            if (request.IsUnassigned.HasValue)
            {
                var doctorUserIds = await _unitOfWork.DoctorRepository
                    .GetAllAsync(null)
                    .Select(d => d.UserId)
                    .ToListAsync(cancellationToken);

                if (request.IsUnassigned.Value)
                    query = query.Where(u => !doctorUserIds.Contains(u.Id));
                else
                    query = query.Where(u => doctorUserIds.Contains(u.Id));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var roleIdToName = await _context.Roles
                .ToDictionaryAsync(r => r.Id, r => r.Name!, cancellationToken);

            var userRoleLookup = (await _context.UserRoles
                .Where(ur => users.Select(u => u.Id).Contains(ur.UserId))
                .ToListAsync(cancellationToken))
                .ToLookup(ur => ur.UserId, ur => ur.RoleId);

            var dtos = users.Select(user => new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber ?? string.Empty,
                    BirthDate = user.BirthDate,
                    Gender = user.Gender,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    Roles = (userRoleLookup[user.Id]
                        .Select(roleId => roleIdToName.GetValueOrDefault(roleId, string.Empty))
                        .Select(r => Enum.TryParse<UserType>(r, ignoreCase: true, out var ut) ? ut : UserType.None)
                        .Where(ut => ut != UserType.None)
                        .ToList() as IList<UserType>) ?? []
                }).ToList();

            return new PagginatedResult<UserDto>(dtos.AsReadOnly(), totalCount, request.PageNumber, request.PageSize);
        }
    }
}
