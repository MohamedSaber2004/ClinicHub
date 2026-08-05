using ClinicHub.Application.Features.Availability.DTOs;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Availability.Queries.GetAvailableSlots
{
    public class GetAvailableSlotsQueryHandler : IRequestHandler<GetAvailableSlotsQuery, GetAvailableSlotsResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAvailableSlotsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GetAvailableSlotsResponse> Handle(GetAvailableSlotsQuery request, CancellationToken cancellationToken)
        {
            var clinic = await _unitOfWork.ClinicRepository.GetByIdAsync(request.ClinicId);

            var allAvailabilities = await _unitOfWork.DoctorAvailabilityRepository
                .GetAllAsync(a => a.DoctorId == request.DoctorId && a.ClinicId == request.ClinicId)
                .ToListAsync(cancellationToken);

            var response = new GetAvailableSlotsResponse
            {
                DoctorId = request.DoctorId,
                ClinicId = request.ClinicId
            };

            if (!allAvailabilities.Any())
                return response;

            if (request.Date.HasValue)
            {
                response.RequestedDate = request.Date.Value.ToString("yyyy-MM-dd");
                var dayOfWeek = request.Date.Value.DayOfWeek;

                var dayAvailabilities = allAvailabilities
                    .Where(a => a.DayOfWeek == dayOfWeek)
                    .ToList();

                var bookedAppointments = await _unitOfWork.AppointmentRepository
                    .GetAppointmentsByDoctorAndDateAsync(request.DoctorId, request.Date.Value);

                foreach (var availability in dayAvailabilities)
                {
                    var window = GetEffectiveWindow(clinic, availability);
                    if (window is null)
                        continue;

                    response.Days.Add(BuildDayAvailability(availability, window.Value, dayOfWeek.ToString(), bookedAppointments));
                }
            }
            else
            {
                foreach (var availability in allAvailabilities)
                {
                    var window = GetEffectiveWindow(clinic, availability);
                    if (window is null)
                        continue;

                    response.Days.Add(BuildDayAvailability(availability, window.Value, availability.DayOfWeek.ToString(), []));
                }
            }

            return response;
        }

        private static (TimeSpan Start, TimeSpan End)? GetEffectiveWindow(
            Domain.Entities.Clinic? clinic,
            Domain.Entities.DoctorAvailability availability)
        {
            var start = availability.StartTime;
            var end = availability.EndTime;

            if (clinic?.WorkingHoursStart is null || clinic.WorkingHoursEnd is null)
                return (start, end);

            var workingDays = ParseWorkingDays(clinic.WorkingDays);
            if (workingDays.Count > 0 && !workingDays.Contains(availability.DayOfWeek))
                return null;

            var clinicStart = clinic.WorkingHoursStart.Value.ToTimeSpan();
            var clinicEnd = clinic.WorkingHoursEnd.Value.ToTimeSpan();

            var effectiveStart = start > clinicStart ? start : clinicStart;
            var effectiveEnd = end < clinicEnd ? end : clinicEnd;

            if (effectiveEnd <= effectiveStart)
                return null;

            return (effectiveStart, effectiveEnd);
        }

        private static HashSet<DayOfWeek> ParseWorkingDays(string? workingDays)
        {
            var result = new HashSet<DayOfWeek>();
            if (string.IsNullOrWhiteSpace(workingDays))
                return result;

            foreach (var part in workingDays.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (Enum.TryParse<DayOfWeek>(part, true, out var day))
                    result.Add(day);
            }

            return result;
        }

        private static DayAvailabilityDto BuildDayAvailability(
            Domain.Entities.DoctorAvailability availability,
            (TimeSpan Start, TimeSpan End) window,
            string dayOfWeek,
            List<Domain.Entities.Appointment> bookedAppointments)
        {
            var slotDurationMinutes = availability.SlotDurationMinutes > 0 ? availability.SlotDurationMinutes : 30;
            var slotDuration = TimeSpan.FromMinutes(slotDurationMinutes);

            var effectiveStart = window.Start;
            var minutesFromAvailabilityStart = (effectiveStart - availability.StartTime).TotalMinutes;
            if (minutesFromAvailabilityStart % slotDurationMinutes != 0)
            {
                effectiveStart = availability.StartTime
                    .Add(TimeSpan.FromMinutes(Math.Ceiling(minutesFromAvailabilityStart / slotDurationMinutes) * slotDurationMinutes));
            }

            var slots = new List<TimeSlotDto>();
            var currentTime = effectiveStart;

            while (currentTime + slotDuration <= window.End)
            {
                var slotEndTime = currentTime + slotDuration;

                var isBooked = bookedAppointments.Any(a =>
                    a.StartTime < slotEndTime && a.EndTime > currentTime);

                slots.Add(new TimeSlotDto
                {
                    Id = Guid.NewGuid(),
                    StartTime = currentTime.ToString(@"hh\:mm"),
                    EndTime = slotEndTime.ToString(@"hh\:mm"),
                    IsAvailable = !isBooked
                });

                currentTime = slotEndTime;
            }

            return new DayAvailabilityDto
            {
                DayOfWeek = dayOfWeek,
                WorkingHours = new WorkingHoursInfo
                {
                    From = window.Start.ToString(@"hh\:mm"),
                    To = window.End.ToString(@"hh\:mm")
                },
                SlotDurationMinutes = slotDurationMinutes,
                Slots = slots.OrderBy(s => s.StartTime).ToList()
            };
        }
    }
}
