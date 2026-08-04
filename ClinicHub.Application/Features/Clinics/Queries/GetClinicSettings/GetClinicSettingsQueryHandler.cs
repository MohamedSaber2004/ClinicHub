using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Clinics.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Clinics.Queries.GetClinicSettings
{
    public class GetClinicSettingsQueryHandler : IRequestHandler<GetClinicSettingsQuery, ClinicSettingsDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public GetClinicSettingsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<ClinicSettingsDto> Handle(GetClinicSettingsQuery request, CancellationToken cancellationToken)
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

            var bookingConfig = await _unitOfWork.BookingConfigurationRepository.GetByClinicIdAsync(clinicId);

            var dto = _mapper.Map<ClinicSettingsDto>(clinic);
            dto.ResponsibleDoctor = clinic.ClinicAdmin?.FullName ?? dto.ResponsibleDoctor;
            dto.ConsultationFee = bookingConfig?.ConsultationFee ?? 0;
            dto.Currency = bookingConfig?.Currency ?? "EGP";
            dto.MaxAdvanceBookingDays = bookingConfig?.MaxAdvanceBookingDays ?? 30;
            dto.ReservationTtlMinutes = bookingConfig?.ReservationTtlMinutes ?? 10;
            dto.CancellationWindowMinutes = bookingConfig?.CancellationWindowMinutes ?? 120;
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
