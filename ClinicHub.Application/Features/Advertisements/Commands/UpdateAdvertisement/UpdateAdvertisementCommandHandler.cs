using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Advertisements.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Advertisements.Commands.UpdateAdvertisement
{
    public class UpdateAdvertisementCommandHandler : IRequestHandler<UpdateAdvertisementCommand, AdvertisementDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public UpdateAdvertisementCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<AdvertisementDto> Handle(UpdateAdvertisementCommand request, CancellationToken cancellationToken)
        {
            var ad = await _unitOfWork.GetRepository<Advertisement, Guid>().GetByIdAsync(request.Id);

            if (_currentUserService.CurrentClinicId.HasValue && ad.ClinicId != _currentUserService.CurrentClinicId)
                throw new BadRequestException(LocalizationKeys.ExceptionMessages.Forbidden.Value);

            if (request.Title != null) ad.Title = request.Title;
            if (request.ImageUrl != null) ad.ImageUrl = request.ImageUrl;
            if (request.TargetUrl != null) ad.TargetUrl = request.TargetUrl;
            if (request.StartDate.HasValue) ad.StartDate = request.StartDate.Value;
            if (request.EndDate.HasValue) ad.EndDate = request.EndDate.Value;
            if (request.AmountPaid.HasValue) ad.AmountPaid = request.AmountPaid.Value;

            _unitOfWork.GetRepository<Advertisement, Guid>().Update(ad);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AdvertisementDto>(ad);
        }
    }
}
