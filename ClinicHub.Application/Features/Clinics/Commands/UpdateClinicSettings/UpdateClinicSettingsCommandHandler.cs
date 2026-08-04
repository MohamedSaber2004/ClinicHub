using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Clinics.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace ClinicHub.Application.Features.Clinics.Commands.UpdateClinicSettings
{
    public class UpdateClinicSettingsCommandHandler : IRequestHandler<UpdateClinicSettingsCommand, ClinicSettingsDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public UpdateClinicSettingsCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<ClinicSettingsDto> Handle(UpdateClinicSettingsCommand request, CancellationToken cancellationToken)
        {
            var clinicId = _currentUserService.CurrentClinicId
                ?? throw new ForbiddenException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

            var clinic = await _unitOfWork.ClinicRepository
                .GetAllAsync(c => c.Id == clinicId)
                .Include(c => c.Specialization)
                .Include(c => c.ClinicAdmin)
                .FirstOrDefaultAsync(cancellationToken);

            if (clinic == null)
                throw new NotFoundException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

            var responsibleDoctor = clinic.ClinicAdmin?.FullName ?? request.ResponsibleDoctor;

            var locationPoint = request.Latitude.HasValue && request.Longitude.HasValue
                ? new Point(request.Longitude.Value, request.Latitude.Value) { SRID = 4326 }
                : null;

            var updatedBy = _currentUserService.IsAuthenticated
                ? _currentUserService.UserId.ToString()
                : "system";

            clinic.UpdateSettings(
                request.Name,
                responsibleDoctor,
                request.Description,
                request.Phone,
                request.ManagerName,
                request.Location,
                request.SpecializationId,
                locationPoint,
                request.IsActive,
                updatedBy);

            _unitOfWork.ClinicRepository.Update(clinic);

            var bookingConfig = await _unitOfWork.BookingConfigurationRepository.GetByClinicIdAsync(clinicId);
            if (bookingConfig == null)
            {
                bookingConfig = new BookingConfiguration(
                    clinicId,
                    request.ConsultationFee,
                    request.Currency,
                    request.MaxAdvanceBookingDays,
                    request.ReservationTtlMinutes,
                    request.CancellationWindowMinutes);
                bookingConfig.MarkAsCreated(updatedBy);
                await _unitOfWork.BookingConfigurationRepository.AddAsync(bookingConfig);
            }
            else
            {
                bookingConfig.Update(
                    request.ConsultationFee,
                    request.Currency,
                    request.MaxAdvanceBookingDays,
                    request.ReservationTtlMinutes,
                    request.CancellationWindowMinutes);
                _unitOfWork.BookingConfigurationRepository.Update(bookingConfig);
            }

            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<ClinicSettingsDto>(clinic);
            dto.ConsultationFee = bookingConfig.ConsultationFee;
            dto.Currency = bookingConfig.Currency;
            dto.MaxAdvanceBookingDays = bookingConfig.MaxAdvanceBookingDays;
            dto.ReservationTtlMinutes = bookingConfig.ReservationTtlMinutes;
            dto.CancellationWindowMinutes = bookingConfig.CancellationWindowMinutes;
            dto.SlotDurationMinutes = await GetReservationDurationAsync(clinicId, cancellationToken);
            return dto;
        }

        private async Task<int> GetReservationDurationAsync(Guid clinicId, CancellationToken cancellationToken)
        {
            var durations = await _unitOfWork.DoctorAvailabilityRepository
                .GetAllAsync(a => a.ClinicId == clinicId && a.SlotDurationMinutes > 0)
                .Select(a => a.SlotDurationMinutes)
                .ToListAsync(cancellationToken);

            if (durations.Count == 0)
                return 30;

            return durations
                .GroupBy(d => d)
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key)
                .First()
                .Key;
        }
    }
}
