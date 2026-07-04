using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.UserClinics.Commands.FollowClinic
{
    public class FollowClinicCommandHandler : IRequestHandler<FollowClinicCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public FollowClinicCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(FollowClinicCommand request, CancellationToken cancellationToken)
        {
            var alreadyFollowing = await _unitOfWork.GetRepository<UserClinic, Guid>()
                .ExistsAsync(uc => uc.UserId == _currentUser.UserId && uc.ClinicId == request.ClinicId, cancellationToken);

            if (alreadyFollowing)
                return true;

            var userClinic = new UserClinic(_currentUser.UserId, request.ClinicId);
            await _unitOfWork.GetRepository<UserClinic, Guid>().AddAsync(userClinic);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
