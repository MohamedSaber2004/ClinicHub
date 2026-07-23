using AutoMapper;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Advertisements.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Advertisements.Commands.CreateAdvertisement
{
    public class CreateAdvertisementCommandHandler : IRequestHandler<CreateAdvertisementCommand, AdvertisementDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public CreateAdvertisementCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<AdvertisementDto> Handle(CreateAdvertisementCommand request, CancellationToken cancellationToken)
        {
            var advertisement = new Advertisement
            {
                ClinicId = _currentUserService.CurrentClinicId,
                Title = request.Title,
                ImageUrl = request.ImageUrl,
                TargetUrl = request.TargetUrl,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                AmountPaid = request.AmountPaid,
                Status = _currentUserService.CurrentClinicId.HasValue
                    ? AdvertisementStatus.Inactive
                    : AdvertisementStatus.Active
            };

            await _unitOfWork.GetRepository<Advertisement, Guid>().AddAsync(advertisement);
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<AdvertisementDto>(advertisement);
            return dto;
        }
    }
}
