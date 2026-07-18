using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Auth.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Auth.Queries.GetUserProfile
{
    public sealed class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public GetUserProfileQueryHandler(
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
        }

        public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());
            var roles = await _userManager.GetRolesAsync(user!);

            var isFreelanceDoctor = await _unitOfWork.DoctorRepository
                .GetAllAsync(d => d.UserId == user!.Id)
                .Select(d => (bool?)d.IsFreelance)
                .FirstOrDefaultAsync(cancellationToken) ?? false;

            return new UserProfileDto(
                user!.Id,
                user.FullName,
                user.Email!,
                user.Gender,
                user.PhoneNumber ?? string.Empty,
                user.BirthDate,
                user.ProfilePictureUrl,
                user.Language,
                roles.FirstOrDefault(),
                isFreelanceDoctor);
        }
    }
}
