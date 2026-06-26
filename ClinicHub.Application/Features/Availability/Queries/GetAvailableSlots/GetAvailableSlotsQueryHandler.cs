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
            var allAvailabilities = await _unitOfWork.DoctorAvailabilityRepository
                .GetAllAsync(a => a.DoctorId == request.DoctorId)
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
                    response.Days.Add(BuildDayAvailability(availability, dayOfWeek.ToString(), bookedAppointments));
                }
            }
            else
            {
                foreach (var availability in allAvailabilities)
                {
                    response.Days.Add(BuildDayAvailability(availability, availability.DayOfWeek.ToString(), []));
                }
            }

            return response;
        }

        private static DayAvailabilityDto BuildDayAvailability(
            Domain.Entities.DoctorAvailability availability,
            string dayOfWeek,
            List<Domain.Entities.Appointment> bookedAppointments)
        {
            var slotDurationMinutes = availability.SlotDurationMinutes > 0 ? availability.SlotDurationMinutes : 30;
            var slotDuration = TimeSpan.FromMinutes(slotDurationMinutes);

            var slots = new List<TimeSlotDto>();
            var currentTime = availability.StartTime;

            while (currentTime + slotDuration <= availability.EndTime)
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
                    From = availability.StartTime.ToString(@"hh\:mm"),
                    To = availability.EndTime.ToString(@"hh\:mm")
                },
                Slots = slots.OrderBy(s => s.StartTime).ToList()
            };
        }
    }
}
