using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Users.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Users.Queries.GetAllUsers
{
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PagginatedResult<UserDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public GetAllUsersQueryHandler(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
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

            if (request.UserTypes.HasValue && request.UserTypes.Value != UserType.None)
            {
                var roleNames = new List<string>();
                foreach (var userType in Enum.GetValues<UserType>())
                {
                    if (userType != UserType.None && request.UserTypes.Value.HasFlag(userType))
                    {
                        roleNames.Add(userType.ToString());
                    }
                }

                var userIds = new HashSet<Guid>();
                foreach (var roleName in roleNames)
                {
                    var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
                    foreach (var user in usersInRole)
                    {
                        userIds.Add(user.Id);
                    }
                }

                query = query.Where(u => userIds.Contains(u.Id));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var dtos = new List<UserDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                dtos.Add(new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber ?? string.Empty,
                    BirthDate = user.BirthDate,
                    Gender = user.Gender,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    Roles = roles
                        .Select(r => Enum.TryParse<UserType>(r, ignoreCase: true, out var ut) ? ut : UserType.None)
                        .Where(ut => ut != UserType.None)
                        .ToList() as IList<UserType>
                });
            }

            return new PagginatedResult<UserDto>(dtos.AsReadOnly(), totalCount, request.PageNumber, request.PageSize);
        }
    }
}
