using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Users.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Users.Queries.GetAllUsers
{
    public class GetAllUsersQueryValidator : AbstractValidator<GetAllUsersQuery>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GetAllUsersQueryValidator(IStringLocalizer<Messages> localizer, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;

            var validTypes = Enum.GetValues<UserType>()
                .Where(ut => ut != UserType.None)
                .ToHashSet();

            RuleFor(x => x.UserId)
                .MustAsync((userId, cancellationToken) => UserExists(userId, cancellationToken))
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.UserNotFound.Value]))
                .When(x => x.UserId.HasValue);

            RuleFor(x => x.UserTypes)
                .Must(types => types == null || types.All(t => validTypes.Contains(t)))
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.InvalidUserType.Value]))
                .When(x => x.UserTypes is { Count: > 0 });
        }

        private async Task<bool> UserExists(Guid? userId, CancellationToken cancellationToken)
        {
            return await _userManager.Users.AnyAsync(u => u.Id == userId, cancellationToken);
        }
    }
}
