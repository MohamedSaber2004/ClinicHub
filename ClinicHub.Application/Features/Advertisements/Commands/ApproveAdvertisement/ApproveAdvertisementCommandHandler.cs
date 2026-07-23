using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Advertisements.Commands.ApproveAdvertisement
{
    public class ApproveAdvertisementCommandHandler : IRequestHandler<ApproveAdvertisementCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ApproveAdvertisementCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(ApproveAdvertisementCommand request, CancellationToken cancellationToken)
        {
            var ad = await _unitOfWork.GetRepository<Advertisement, Guid>().GetByIdAsync(request.AdvertisementId);

            if (ad.Status != AdvertisementStatus.Inactive)
                throw new BadRequestException("Advertisement is not pending approval.");

            ad.Status = AdvertisementStatus.Active;
            _unitOfWork.GetRepository<Advertisement, Guid>().Update(ad);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
