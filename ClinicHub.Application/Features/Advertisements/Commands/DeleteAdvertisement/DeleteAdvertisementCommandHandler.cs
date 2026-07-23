using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Advertisements.Commands.DeleteAdvertisement
{
    public class DeleteAdvertisementCommandHandler : IRequestHandler<DeleteAdvertisementCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public DeleteAdvertisementCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(DeleteAdvertisementCommand request, CancellationToken cancellationToken)
        {
            var ad = await _unitOfWork.GetRepository<Advertisement, Guid>().GetByIdAsync(request.Id);

            if (_currentUserService.CurrentClinicId.HasValue && ad.ClinicId != _currentUserService.CurrentClinicId)
                throw new BadRequestException(LocalizationKeys.ExceptionMessages.Forbidden.Value);

            _unitOfWork.GetRepository<Advertisement, Guid>().Delete(ad);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
