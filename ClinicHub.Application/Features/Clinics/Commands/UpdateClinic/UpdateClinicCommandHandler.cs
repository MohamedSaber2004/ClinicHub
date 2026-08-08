using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Clinics.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;
using NetTopologySuite.Geometries;

namespace ClinicHub.Application.Features.Clinics.Commands.UpdateClinic
{
    public class UpdateClinicCommandHandler : IRequestHandler<UpdateClinicCommand, ClinicManagementDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly ICurrentUserService _currentUserService;

        public UpdateClinicCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IStringLocalizer<Messages> localizer,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizer = localizer;
            _currentUserService = currentUserService;
        }

        public async Task<ClinicManagementDto> Handle(UpdateClinicCommand request, CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var clinic = await _unitOfWork.ClinicRepository.GetByIdAsync(request.Id);
                if (clinic == null)
                {
                    throw new NotFoundException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);
                }

                var updatedBy = _currentUserService.IsAuthenticated
                    ? _currentUserService.UserId.ToString()
                    : "system";

                var locationPoint = request.Latitude.HasValue && request.Longitude.HasValue
                    ? new Point(request.Longitude.Value, request.Latitude.Value) { SRID = 4326 }
                    : null;

                clinic.UpdateDetails(
                    request.Name,
                    request.NameAr,
                    request.Description,
                    request.ArDescription,
                    request.Address,
                    request.AddressAr,
                    request.Phone,
                    request.Email,
                    request.Website,
                    request.Logo,
                    request.WorkingHours,
                    request.SpecializationId ?? clinic.SpecializationId,
                    updatedBy,
                    request.WorkingHoursStart,
                    request.WorkingHoursEnd,
                    request.WorkingDays != null ? string.Join(",", request.WorkingDays) : null,
                    locationPoint);

                _unitOfWork.ClinicRepository.Update(clinic);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitAsync();

                var clinicDto = _mapper.Map<ClinicManagementDto>(clinic);
                return clinicDto;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}
