using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ClinicHub.Application.Features.Auth.Commands.UpdateLanguage
{
    public class UpdateLanguageCommandHandler : IRequestHandler<UpdateLanguageCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUserService;

        public UpdateLanguageCommandHandler(
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(UpdateLanguageCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());

            if (user == null)
                return false;

            user.UpdateLanguage(request.Language);

            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded;
        }
    }
}
