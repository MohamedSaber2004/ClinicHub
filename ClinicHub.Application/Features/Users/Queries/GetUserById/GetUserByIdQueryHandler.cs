using ClinicHub.Application.Features.Users.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GetUserByIdQueryHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == request.Id && !u.IsDeleted, cancellationToken)
                ?? throw new KeyNotFoundException($"User with id {request.Id} not found");

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                BirthDate = user.BirthDate,
                Gender = user.Gender,
                Image = user.ProfilePictureUrl,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                Roles = roles
                    .Select(r => Enum.TryParse<UserType>(r, ignoreCase: true, out var ut) ? ut : UserType.None)
                    .Where(ut => ut != UserType.None)
                    .ToList() as IList<UserType>
            };
        }
    }
}
