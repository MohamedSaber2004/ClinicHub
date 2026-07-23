using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Advertisements.Commands.RejectAdvertisement
{
    public class RejectAdvertisementCommandHandler : IRequestHandler<RejectAdvertisementCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RejectAdvertisementCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(RejectAdvertisementCommand request, CancellationToken cancellationToken)
        {
            var ad = await _unitOfWork.GetRepository<Advertisement, Guid>().GetByIdAsync(request.AdvertisementId);

            if (ad.Status != AdvertisementStatus.Inactive)
                throw new BadRequestException("Advertisement is not pending approval.");

            _unitOfWork.GetRepository<Advertisement, Guid>().Delete(ad);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
