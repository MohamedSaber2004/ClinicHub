using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.UserClinics.Commands.UnfollowClinic
{
    public class UnfollowClinicCommandHandler : IRequestHandler<UnfollowClinicCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public UnfollowClinicCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(UnfollowClinicCommand request, CancellationToken cancellationToken)
        {
            var userClinic = await _unitOfWork.GetRepository<UserClinic, Guid>()
                .GetFirstAsync(uc => uc.UserId == _currentUser.UserId && uc.ClinicId == request.ClinicId, cancellationToken);

            if (userClinic == null)
                return true;

            _unitOfWork.GetRepository<UserClinic, Guid>().Delete(userClinic);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
