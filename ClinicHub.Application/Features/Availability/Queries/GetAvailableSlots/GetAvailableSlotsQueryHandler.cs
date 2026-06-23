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
            var dayOfWeek = request.Date.DayOfWeek;

            var availabilities = await _unitOfWork.DoctorAvailabilityRepository
                .GetAllAsync(a => a.DoctorId == request.DoctorId && a.DayOfWeek == dayOfWeek)
                .ToListAsync(cancellationToken);

            var response = new GetAvailableSlotsResponse
            {
                DoctorId = request.DoctorId,
                ClinicId = request.ClinicId,
                Date = request.Date.ToString("yyyy-MM-dd"),
                SlotDurationMinutes = 30
            };

            if (!availabilities.Any())
                return response;

            var firstAvail = availabilities.First();
            response.SlotDurationMinutes = firstAvail.SlotDurationMinutes > 0 ? firstAvail.SlotDurationMinutes : 30;
            response.WorkingHours = new WorkingHoursInfo
            {
                From = firstAvail.StartTime.ToString(@"hh\:mm"),
                To = firstAvail.EndTime.ToString(@"hh\:mm")
            };

            var bookedAppointments = await _unitOfWork.AppointmentRepository
                .GetAppointmentsByDoctorAndDateAsync(request.DoctorId, request.Date);

            foreach (var availability in availabilities)
            {
                var slotDuration = TimeSpan.FromMinutes(response.SlotDurationMinutes);
                var currentTime = availability.StartTime;

                while (currentTime + slotDuration <= availability.EndTime)
                {
                    var slotEndTime = currentTime + slotDuration;

                    var isBooked = bookedAppointments.Any(a =>
                        a.StartTime < slotEndTime && a.EndTime > currentTime);

                    response.Slots.Add(new TimeSlotDto
                    {
                        Id = Guid.NewGuid(),
                        StartTime = currentTime.ToString(@"hh\:mm"),
                        EndTime = slotEndTime.ToString(@"hh\:mm"),
                        IsAvailable = !isBooked
                    });

                    currentTime = slotEndTime;
                }
            }

            response.Slots = response.Slots.OrderBy(t => t.StartTime).ToList();
            return response;
        }
    }
}
