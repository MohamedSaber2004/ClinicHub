using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Auth.DTOs;
using ClinicHub.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ClinicHub.Application.Features.Auth.Queries.GetUserProfile
{
    public sealed class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUserService;

        public GetUserProfileQueryHandler(
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _currentUserService = currentUserService;
        }

        public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());
            
            return new UserProfileDto(
                user!.Id,
                user.FullName,
                user.Email!,
                user.Gender,
                user.PhoneNumber ?? string.Empty,
                user.BirthDate,
                user.ProfilePictureUrl,
                user.Language);
        }
    }
}
